// <copyright file="GraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Helpers
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using ListSearch.Common.Models;

    /// <summary>
    /// Helper for Graph.
    /// </summary>
    public class GraphHelper
    {
        private const string GraphV1Endpoint = "https://graph.microsoft.com/v1.0";

        private readonly HttpClient httpClient;
        private readonly TokenHelper tokenHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphHelper"/> class.
        /// </summary>
        /// <param name="httpClient">Http client.</param>
        /// <param name="tokenHelper">Token helper</param>
        public GraphHelper(HttpClient httpClient, TokenHelper tokenHelper)
        {
            this.httpClient = httpClient;
            this.tokenHelper = tokenHelper;
        }

        /// <summary>
        /// Gets contents of the sharepoint list from graph.
        /// </summary>
        /// <param name="listId">Id of the list to be fetched.</param>
        /// <param name="fieldsToFetch">fields to fetch from list.</param>
        /// <param name="sharePointSiteId">site id of sharepoint site.</param>
        /// <param name="odataNextUrl">url to fetch next page of data</param>
        /// <returns><see cref="Task"/> that resolves to <see cref="string"/> representing contents of the file.</returns>
        public async Task<string> GetListContentsAsync(string listId, IEnumerable<string> fieldsToFetch, string sharePointSiteId, string odataNextUrl = null)
        {
            var accessToken = await this.tokenHelper.GetAccessTokenAsync(TokenTypes.GraphTokenType);

            string uri;
            if (string.IsNullOrEmpty(odataNextUrl))
            {
                var fieldsSpec = string.Join(",", fieldsToFetch);
                uri = $"{GraphV1Endpoint}/sites/{sharePointSiteId}/lists/{listId}/items?expand=fields(select={fieldsSpec})";
            }
            else
            {
                uri = odataNextUrl;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await this.httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
