// <copyright file="GraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.ListSearch.Common.Models;

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
        /// <param name="httpClient">HTTP client.</param>
        /// <param name="tokenHelper">Token helper</param>
        public GraphHelper(HttpClient httpClient, TokenHelper tokenHelper)
        {
            this.httpClient = httpClient;
            this.tokenHelper = tokenHelper;
        }

        /// <summary>
        /// Gets contents of the SharePoint list from graph.
        /// </summary>
        /// <param name="listId">Id of the list to be fetched.</param>
        /// <param name="fieldsToFetch">Fields to fetch from list.</param>
        /// <param name="sharePointSiteId">Site id of SharePoint site.</param>
        /// <param name="odataNextUrl">URL to fetch next page of data</param>
        /// <returns><see cref="Task"/> That resolves to <see cref="string"/> representing contents of the file.</returns>
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

        /// <summary>
        /// This method will construct Graph API endpoint using SharePoint site URL and gets columns list
        /// </summary>
        /// <param name="sharePointSiteUrl">SharePoint site URL</param>
        /// <returns>Graph result</returns>
        public async Task<string> GetListInfoAsync(string sharePointSiteUrl)
        {
            // Get list name from the URL
            // If URL is https://microsoft.sharepoint.com/teams/Mysite/Lists/TestListSearch/AllItems.aspx then listName would be "TestListSearch"
            string[] urlSplitArray = sharePointSiteUrl.Split('/');

            int listIndex = Array.FindIndex(urlSplitArray, element => element.ToLower().Equals("lists"));
            if (listIndex == -1)
            {
                throw new Exception("Not a valid SharePoint URL");
            }

            string listName = urlSplitArray[listIndex + 1];
            string siteName = this.GetSharePointSiteName(urlSplitArray, listIndex);

            // By using siteName and listName we can construct graph API endpoint like shown below
            // https://graph.microsoft.com/v1.0/sites/{siteName}:/lists/{listName}/?expand=columns
            string uri = $"{GraphV1Endpoint}/sites/{siteName}:/lists/{listName}/?expand=columns";

            var accessToken = await this.tokenHelper.GetAccessTokenAsync(TokenTypes.GraphTokenType);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await this.httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// This method will return the SharePoint site name  based on the given SharePoint site URL
        /// Scenario 1 :If URL = https://microsoft.sharepoint.com/teams/MyTestSite/Lists/TestListSearch/AllItems.aspx
        /// then siteName = "microsoft.sharepoint.com:/teams/MyTestSite"
        /// Scenario 2 :If URL = https://acco365.sharepoint.com/sites/test/Lists/TestListSearch/AllItems.aspx
        /// then  siteName = "acco365.sharepoint.com:/sites/test"
        /// Scenario 3 :If URL = https://microsoft.sharepoint.com/Lists/TestListSearch/AllItems.aspx
        /// then  siteName = "microsoft.sharepoint.com"
        /// </summary>
        /// <param name="urlSplitArray">SharePoint site URL split array</param>
        /// <param name="listIndex">Index of the SharePoint list from the URL</param>
        /// <returns>SharePoint site name</returns>
        private string GetSharePointSiteName(string[] urlSplitArray, int listIndex)
        {
            string siteName = string.Empty;
            if (urlSplitArray.Length > 3)
            {
                for (int i = 3; i < listIndex; i++)
                {
                    siteName += "/" + urlSplitArray[i];
                }

                if (siteName.Length > 0)
                {
                    siteName = urlSplitArray[2] + ":" + siteName;
                }
                else
                {
                    siteName = urlSplitArray[2];
                }
            }

            return siteName;
        }
    }
}
