// <copyright file="TokenHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Helpers
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;
    using ListSearch.Models;
    using Microsoft.Azure;
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

        private readonly int insertSuccessResponseCode = 204;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenHelper"/> class.
        /// </summary>
        public TokenHelper()
        {
            this.storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            this.cloudTableClient = this.storageAccount.CreateCloudTableClient();
            this.tokenEndpoint = $"https://login.microsoftonline.com/{ConfigurationManager.AppSettings["TenantId"]}/oauth2/v2.0/token";
        }

        /// <summary>
        /// Decrypt Token.
        /// </summary>
        /// <param name="token">Token to be decrypted.</param>
        /// <returns>Decrypted token.</returns>
        public static string DecryptToken(string token)
        {
            string key = ConfigurationManager.AppSettings["LoginAppClientSecret"];
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
        /// <returns><see cref="Task"/> that resolves to <see cref="RefreshTokenResponse"/></returns>
        public async Task<RefreshTokenResponse> GetRefreshToken(HttpClient httpClient, string clientId, string clientSecret, string scope, string refreshToken, string tokenType)
        {
            try
            {
                string body = $"&client_id={clientId}" +
                    $"&scope={scope}" +
                    $"&refresh_token={DecryptToken(refreshToken)}" +
                    $"&grant_type={RefreshTokenGrantType}" +
                    $"&client_secret={clientSecret}";

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
                    AccessToken = this.EncryptToken(refreshTokenResponse.AccessToken),
                    RefreshToken = this.EncryptToken(refreshTokenResponse.RefreshToken),
                };

                TableResult storeTokenResponse = await this.StoreToken(tokenEntity, tokenType);
                if (storeTokenResponse.HttpStatusCode == this.insertSuccessResponseCode)
                {
                    return refreshTokenResponse;
                }
                else
                {
                    throw new HttpResponseException((System.Net.HttpStatusCode)storeTokenResponse.HttpStatusCode); // TODO: Handle Exception
                }
            }
            catch
            {
                // TODO: Handle if refresh token has expired.
                throw;
            }
        }

        /// <summary>
        /// Gets token from storage. // TODO: change to key vault.
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
        /// Stores token to storage. // TODO: move to key vault.
        /// </summary>
        /// <param name="tokenEntity">entity to be stored.</param>
        /// <param name="tokenType">Token type</param>
        /// <returns><see cref="Task"/> that resolves to <see cref="TableResult"/></returns>
        private async Task<TableResult> StoreToken(TokenEntity tokenEntity, string tokenType)
        {
            try
            {
                CloudTable cloudTable = this.cloudTableClient.GetTableReference(TokenTableName);
                TableOperation insertOperation = TableOperation.InsertOrMerge(tokenEntity);
                TableResult insertResult = await cloudTable.ExecuteAsync(insertOperation);
                return insertResult;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Encrypt Token.
        /// </summary>
        /// <param name="token">Token to be encrypted.</param>
        /// <returns>Encrypted token.</returns>
        private string EncryptToken(string token)
        {
            string key = ConfigurationManager.AppSettings["LoginAppClientSecret"];
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