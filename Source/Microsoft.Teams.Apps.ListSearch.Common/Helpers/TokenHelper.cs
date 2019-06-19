// <copyright file="TokenHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.ListSearch.Common.Models;
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
        private const double TokenExpiryAllowanceInMinutes = 5;
        private const string Scope = "offline_access https://graph.microsoft.com/Sites.Read.All";

        private readonly CloudTableClient cloudTableClient;
        private readonly CloudTable cloudTable;
        private readonly string tokenEndpoint;
        private readonly HttpClient httpClient;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string tokenKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenHelper"/> class.
        /// </summary>
        /// <param name="httpClient">Http client</param>
        /// <param name="connectionString">Connection string of storage.</param>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="clientId">Client id of the auth app</param>
        /// <param name="clientSecret">Client secret for the app</param>
        /// <param name="tokenKey">Key used to secure the token</param>
        public TokenHelper(HttpClient httpClient, string connectionString, string tenantId, string clientId, string clientSecret, string tokenKey)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            this.cloudTableClient = storageAccount.CreateCloudTableClient();
            this.cloudTable = this.cloudTableClient.GetTableReference(StorageInfo.TokenTableName);

            this.httpClient = httpClient;
            this.tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.tokenKey = tokenKey;
        }

        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        /// <param name="tokenType">type of token to be fetched</param>
        /// <returns><see cref="Task"/> that resolves to an access token</returns>
        public async Task<string> GetAccessTokenAsync(string tokenType)
        {
            TokenEntity token = await this.GetTokenEntityAsync(TokenTypes.GraphTokenType);
            if (token.ExpiryDateTime.ToUniversalTime() < DateTime.UtcNow.AddMinutes(TokenExpiryAllowanceInMinutes))
            {
                token = await this.RefreshTokenAsync(token);
            }

            return this.DecryptToken(token.AccessToken, this.tokenKey);
        }

        /// <summary>
        /// To configure the user who have access to share point
        /// </summary>
        /// <param name="userEmail">Logged in user email</param>
        /// <param name="accessToken">Authorization code</param>
        /// <returns>Token status</returns>
        public async Task<bool> SetSharePointUserAsync(string userEmail, string accessToken)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                    new KeyValuePair<string, string>("client_id", this.clientId),
                    new KeyValuePair<string, string>("client_secret", this.clientSecret),
                    new KeyValuePair<string, string>("assertion", accessToken),
                    new KeyValuePair<string, string>("scope", Scope),
                    new KeyValuePair<string, string>("requested_token_use", "on_behalf_of"),
                });

            HttpResponseMessage res = await this.httpClient.PostAsync(this.tokenEndpoint, content);
            res.EnsureSuccessStatusCode();

            string json = await res.Content.ReadAsStringAsync();
            AzureADTokenResponse tokenResponse = JsonConvert.DeserializeObject<AzureADTokenResponse>(json);
            TokenEntity tokenEntity = new TokenEntity()
            {
                PartitionKey = PartitionKey,
                RowKey = TokenTypes.GraphTokenType,
                AccessToken = this.EncryptToken(tokenResponse.AccessToken, this.tokenKey),
                RefreshToken = this.EncryptToken(tokenResponse.RefreshToken, this.tokenKey),
                Scopes = Scope,
                UserPrincipalName = userEmail,
                ExpiryDateTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
            };

            var result = await this.StoreTokenEntityAsync(tokenEntity);
            if (result.HttpStatusCode != (int)System.Net.HttpStatusCode.NoContent)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets token from storage.
        /// </summary>
        /// <param name="tokenType">Type of token to be retrieved.</param>
        /// <returns>TokenEntity</returns>
        public async Task<TokenEntity> GetTokenEntityAsync(string tokenType)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<TokenEntity>(PartitionKey, tokenType);
            TableResult retrievedResult = await this.cloudTable.ExecuteAsync(retrieveOperation);

            return (TokenEntity)retrievedResult.Result;
        }

        /// <summary>
        /// Refresh the token
        /// </summary>
        /// <param name="token">Token Entity</param>
        /// <returns>Returns refresh token entity</returns>
        private async Task<TokenEntity> RefreshTokenAsync(TokenEntity token)
        {
            var body = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", RefreshTokenGrantType),
                    new KeyValuePair<string, string>("client_id", this.clientId),
                    new KeyValuePair<string, string>("client_secret", this.clientSecret),
                    new KeyValuePair<string, string>("refresh_token", this.DecryptToken(token.RefreshToken, this.tokenKey)),
                    new KeyValuePair<string, string>("scope", token.Scopes),
                });

            var response = await this.httpClient.PostAsync(this.tokenEndpoint, body);
            string responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<AzureADTokenResponse>(responseBody);

            TokenEntity tokenEntity = new TokenEntity()
            {
                PartitionKey = PartitionKey,
                RowKey = token.TokenType,
                AccessToken = this.EncryptToken(tokenResponse.AccessToken, this.tokenKey),
                RefreshToken = this.EncryptToken(tokenResponse.RefreshToken, this.tokenKey),
                Scopes = token.Scopes,
                UserPrincipalName = token.UserPrincipalName,
                ExpiryDateTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
            };

            var storeTokenResponse = await this.StoreTokenEntityAsync(tokenEntity);
            if (storeTokenResponse.HttpStatusCode == (int)System.Net.HttpStatusCode.NoContent)
            {
                return tokenEntity;
            }
            else
            {
                throw new Exception($"HTTP Error code - {response.StatusCode}");
            }
        }

        /// <summary>
        /// Stores token to storage.
        /// </summary>
        /// <param name="tokenEntity">Entity to be stored.</param>
        /// <returns><see cref="Task"/> That resolves to <see cref="TableResult"/></returns>
        private async Task<TableResult> StoreTokenEntityAsync(TokenEntity tokenEntity)
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(StorageInfo.TokenTableName);
            TableOperation insertOperation = TableOperation.InsertOrMerge(tokenEntity);

            return await cloudTable.ExecuteAsync(insertOperation);
        }

        /// <summary>
        /// Encrypt Token.
        /// </summary>
        /// <param name="token">Token to be encrypted.</param>
        /// <param name="key">Key to be used for encryption and decryption.</param>
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
        /// <param name="key">Key to be used for decryption.</param>
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
