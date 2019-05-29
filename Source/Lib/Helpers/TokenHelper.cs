// <copyright file="TokenHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Lib.Helpers
{
    using System;
    using System.Configuration;
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
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient cloudTableClient;
        private readonly string tokenEndpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenHelper"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage.</param>
        /// <param name="tenantId">tenant Id.</param>
        public TokenHelper(string connectionString, string tenantId)
        {
            this.storageAccount = CloudStorageAccount.Parse(connectionString);
            this.cloudTableClient = this.storageAccount.CreateCloudTableClient();
            this.tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
        }

        /// <summary>
        /// Decrypt Token.
        /// </summary>
        /// <param name="token">Token to be decrypted.</param>
        /// <param name="key">key to be used for decryption.</param>
        /// <returns>Decrypted token.</returns>
        public static string DecryptToken(string token, string key)
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

        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        /// <param name="httpClient">http client</param>
        /// <param name="clientId">client id of the auth app</param>
        /// <param name="clientSecret">client secret for the app</param>
        /// <param name="scope">scope</param>
        /// <param name="refreshToken">refresh token</param>
        /// <param name="tokenType">type of token to be fetched</param>
        /// <param name="key">key to be used for encryption and decryption</param>
        /// <returns><see cref="Task"/> that resolves to <see cref="RefreshTokenResponse"/></returns>
        public async Task<RefreshTokenResponse> GetRefreshToken(HttpClient httpClient, string clientId, string clientSecret, string scope, string refreshToken, string tokenType, string key)
        {
            try
            {
                string body = $"&client_id={clientId}" +
                    $"&scope={Uri.EscapeDataString(scope)}" +
                    $"&refresh_token={Uri.EscapeDataString(DecryptToken(refreshToken, key))}" +
                    $"&grant_type={RefreshTokenGrantType}" +
                    $"&client_secret={Uri.EscapeDataString(clientSecret)}";

                var request = new HttpRequestMessage(HttpMethod.Post, this.tokenEndpoint)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded")
                };
                HttpResponseMessage response = await httpClient.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();
                RefreshTokenResponse refreshTokenResponse = JsonConvert.DeserializeObject<RefreshTokenResponse>(responseBody);

                TokenEntity tokenEntity = new TokenEntity()
                {
                    PartitionKey = PartitionKey,
                    RowKey = tokenType,
                    AccessToken = this.EncryptToken(refreshTokenResponse.AccessToken, key),
                    RefreshToken = this.EncryptToken(refreshTokenResponse.RefreshToken, key),
                };

                TableResult storeTokenResponse = await this.StoreToken(tokenEntity, tokenType);

                if (storeTokenResponse.HttpStatusCode == (int)System.Net.HttpStatusCode.NoContent)
                {
                    return refreshTokenResponse;
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
    }
}
