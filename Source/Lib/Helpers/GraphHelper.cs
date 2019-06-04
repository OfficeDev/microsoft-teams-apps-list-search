// <copyright file="GraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Lib.Helpers
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Lib.Models;

    /// <summary>
    /// Helper for Graph.
    /// </summary>
    public class GraphHelper
    {
        private const string Scope = "offline_access https://graph.microsoft.com/Sites.Read.All";
        private const string GraphV1Endpoint = "https://graph.microsoft.com/v1.0";

        private readonly string clientId;
        private readonly string clientSecret;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphHelper"/> class.
        /// </summary>
        /// <param name="appId">id of the app used for login.</param>
        /// <param name="appSecret">secret for the app used for login.</param>
        public GraphHelper(string appId, string appSecret)
        {
            this.clientId = appId;
            this.clientSecret = appSecret;
        }

        /// <summary>
        /// Gets contents of the sharepoint list from graph.
        /// </summary>
        /// <param name="httpClient">Http client.</param>
        /// <param name="refreshToken">Refresh token of the user.</param>
        /// <param name="listId">Id of the list to be fetched.</param>
        /// <param name="fieldsToFetch">fields to fetch from list.</param>
        /// <param name="sharePointSiteId">site id of sharepoint site.</param>
        /// <param name="connectionString">connection string of storage.</param>
        /// <param name="tenantId">tenant Id.</param>
        /// <param name="encryptionDecryptionKey">encryption decryption key.</param>
        /// <param name="odataNextUrl">url to fetch next page of data</param>
        /// <returns><see cref="Task"/> that resolves to <see cref="string"/> representing contents of the file.</returns>
        public async Task<string> GetListContents(HttpClient httpClient, string refreshToken, string listId, string fieldsToFetch, string sharePointSiteId, string connectionString, string tenantId, string encryptionDecryptionKey, string odataNextUrl = null)
        {
            TokenHelper tokenHelper = new TokenHelper(httpClient, connectionString, tenantId, this.clientId, this.clientSecret, encryptionDecryptionKey);
            RefreshTokenResponse refreshTokenResponse = await tokenHelper.GetRefreshToken(Scope, refreshToken, TokenTypes.GraphTokenType);
            string uri;
            if (string.IsNullOrEmpty(odataNextUrl))
            {
                uri = $"{GraphV1Endpoint}/sites/{sharePointSiteId}/lists/{listId}/items?expand=fields(select={fieldsToFetch})";
            }
            else
            {
                uri = odataNextUrl;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshTokenResponse.AccessToken);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(responseBody);
            }

            return responseBody;
        }
    }
}
