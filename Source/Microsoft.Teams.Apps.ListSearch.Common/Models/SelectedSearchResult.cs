// <copyright file="SelectedSearchResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Structure of response from Task Module for generating the adaptive card.
    /// </summary>
    public class SelectedSearchResult
    {
        /// <summary>
        /// Gets or sets Kb id
        /// </summary>
        [JsonProperty("kbId")]
        public string KBId { get; set; }

        /// <summary>
        /// Gets or sets answers
        /// </summary>
        [JsonProperty("answers")]
        public List<DeserializedAnswer> Answers { get; set; }

        /// <summary>
        /// Gets or sets question
        /// </summary>
        [JsonProperty("question")]
        public string Question { get; set; }

        /// <summary>
        /// Gets or sets the URL of the SharePoint list
        /// </summary>
        [JsonProperty("sharePointUrl")]
        public string SharePointListUrl { get; set; }

        /// <summary>
        /// Gets or sets the id of the list item
        /// </summary>
        [JsonProperty("listItemId")]
        public string ListItemId { get; set; }

        /// <summary>
        /// Gets or sets the search session id
        /// </summary>
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
    }
}
