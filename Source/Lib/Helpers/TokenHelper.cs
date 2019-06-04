// <copyright file="TokenHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Lib.Helpers
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Lib.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Token Helper
    /// </summary>
    public class TokenHelper
    {
        private const string RefreshTokenGrantType = "refresh_token";
        private const string PartitionKey = "Token";

        private static readonly string TokenTableName = StorageInfo.TokenTableName;
        private readonly CloudTableClient cloudTableClient;
        private readonly string tokenEndpoint;
        private readonly HttpClient httpClient;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string tokenKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenHelper"/> class.
        /// </summary>
        /// <param name="httpClient">http client</param>
        /// <param name="connectionString">connection string of storage.</param>
        /// <param name="tenantId">tenant Id.</param>
        /// <param name="clientId">client id of the auth app</param>
        /// <param name="clientSecret">client secret for the app</param>
        /// <param name="tokenKey">key used to secure the token</param>
        public TokenHelper(HttpClient httpClient, string connectionString, string tenantId, string clientId, string clientSecret, string tokenKey)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            this.cloudTableClient = storageAccount.CreateCloudTableClient();

            this.httpClient = httpClient;
            this.tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.tokenKey = tokenKey;
        }

        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        /// <param name="scope">scope</param>
        /// <param name="refreshToken">refresh token</param>
        /// <param name="tokenType">type of token to be fetched</param>
        /// <returns><see cref="Task"/> that resolves to an access token</returns>
        public async Task<string> GetAccessTokenAsync(string scope, string refreshToken, string tokenType)
        {
            try
            {
                string body = $"&client_id={this.clientId}" +
                    $"&scope={Uri.EscapeDataString(scope)}" +
                    $"&refresh_token={Uri.EscapeDataString(this.DecryptToken(refreshToken, this.tokenKey))}" +
                    $"&grant_type={RefreshTokenGrantType}" +
                    $"&client_secret={Uri.EscapeDataString(this.clientSecret)}";

                var request = new HttpRequestMessage(HttpMethod.Post, this.tokenEndpoint)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded")
                };

                var response = await this.httpClient.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();
                var refreshTokenResponse = JsonConvert.DeserializeObject<RefreshTokenResponse>(responseBody);

                TokenEntity tokenEntity = new TokenEntity()
                {
                    PartitionKey = PartitionKey,
                    RowKey = tokenType,
                    AccessToken = this.EncryptToken(refreshTokenResponse.AccessToken, this.tokenKey),
                    RefreshToken = this.EncryptToken(refreshTokenResponse.RefreshToken, this.tokenKey),
                };

                TableResult storeTokenResponse = await this.StoreToken(tokenEntity, tokenType);

                if (storeTokenResponse.HttpStatusCode == (int)System.Net.HttpStatusCode.NoContent)
                {
                    return refreshTokenResponse.AccessToken;
                }
                else
                {
                    throw new Exception($"HTTP Error code - {response.StatusCode}"); // TODO: Handle Exception
                }
            }
            catch
            {
                // TODO: Handle if refresh token has expired.
                throw;
            }
        }

        /// <summary>
        /// Gets token from storage.
        /// </summary>
        /// <param name="tokenType">type of token to be retrieved.</param>
        /// <returns>TokenEntity</returns>
        public async Task<TokenEntity> GetTokenEntity(string tokenType)
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(TokenTableName);
            TableOperation retrieveOperation = TableOperation.Retrieve<TokenEntity>(PartitionKey, tokenType);
            TableResult retrievedResult = await cloudTable.ExecuteAsync(retrieveOperation);
            TokenEntity result = (TokenEntity)retrievedResult.Result;
            return result;
        }

        /// <summary>
        /// Stores token to storage.
        /// </summary>
        /// <param name="tokenEntity">entity to be stored.</param>
        /// <param name="tokenType">Token type</param>
        /// <returns><see cref="Task"/> that resolves to <see cref="TableResult"/></returns>
        private Task<TableResult> StoreToken(TokenEntity tokenEntity, string tokenType)
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(TokenTableName);
            TableOperation insertOperation = TableOperation.InsertOrMerge(tokenEntity);
            return cloudTable.ExecuteAsync(insertOperation);
        }

        /// <summary>
        /// Encrypt Token.
        /// </summary>
        /// <param name="token">Token to be encrypted.</param>
        /// <param name="key">key to be used for encryption and decryption.</param>
        /// <returns>Encrypted token.</returns>
        private string EncryptToken(string token, string key)
        {
            byte[] clearBytes = Encoding.UTF8.GetBytes(token);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, Encoding.UTF8.GetBytes(key));
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }

                    token = Convert.ToBase64String(ms.ToArray());
                }
            }

            return token;
        }

        /// <summary>
        /// Decrypt Token.
        /// </summary>
        /// <param name="token">Token to be decrypted.</param>
        /// <param name="key">key to be used for decryption.</param>
        /// <returns>Decrypted token.</returns>
        private string DecryptToken(string token, string key)
        {
            byte[] cipherBytes = Convert.FromBase64String(token);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, Encoding.UTF8.GetBytes(key));
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }

                    token = Encoding.UTF8.GetString(ms.ToArray());
                }
            }

            return token;
        }
    }
}
