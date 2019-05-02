// <copyright file="GraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Helpers
{
    using System;
    using System.Configuration;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using ListSearch.Models;

    /// <summary>
    /// Helper for Graph.
    /// </summary>
    public class GraphHelper
    {
        private const string Scope = "offline_access%20https://graph.microsoft.com/Sites.Read.All";
        private const string GraphV1Endpoint = "https://graph.microsoft.com/v1.0";

        private readonly string clientId;
        private readonly string clientSecret;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphHelper"/> class.
        /// </summary>
        public GraphHelper()
        {
            this.clientId = ConfigurationManager.AppSettings["LoginAppClientId"];
            this.clientSecret = ConfigurationManager.AppSettings["LoginAppClientSecret"];
        }

        /// <summary>
        /// Gets contents of the sharepoint list from graph.
        /// </summary>
        /// <param name="httpClient">Http client.</param>
        /// <param name="refreshToken">Refresh token of the user.</param>
        /// <param name="listId">Id of the list to be fetched.</param>
        /// <param name="fieldsToFetch">fields to fetch from list.</param>
        /// <param name="sharePointSiteId">site id of sharepoint site.</param>
        /// <param name="odataNextUrl">url to fetch next page of data</param>
        /// <returns><see cref="Task"/> that resolves to <see cref="string"/> representing contents of the file.</returns>
        public async Task<string> GetListContents(HttpClient httpClient, string refreshToken, string listId, string fieldsToFetch, string sharePointSiteId, string odataNextUrl = null)
        {
            TokenHelper tokenHelper = new TokenHelper();
            RefreshTokenResponse refreshTokenResponse = await tokenHelper.GetRefreshToken(httpClient, this.clientId, this.clientSecret, Scope, refreshToken, TokenTypes.GraphTokenType);
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