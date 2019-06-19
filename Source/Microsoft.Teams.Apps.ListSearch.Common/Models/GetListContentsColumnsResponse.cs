// <copyright file="GetListContentsColumnsResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Get List Contents Columns Response
    /// </summary>
    public class GetListContentsColumnsResponse
    {
        /// <summary>
        /// Gets or sets odata context
        /// </summary>
        [JsonProperty("@odata.context")]
        public string ODataContext { get; set; }

        /// <summary>
        /// Gets or sets listId
        /// </summary>
        [JsonProperty("id")]
        public string ListId { get; set; }

        /// <summary>
        /// Gets or sets SiteId
        /// </summary>
        [JsonProperty("parentReference")]
        public ParentReference ParentReference { get; set; }

        /// <summary>
        /// Gets or sets List display name
        /// </summary>
        [JsonProperty("displayName")]
        public string ListDisplayName { get; set; }

        /// <summary>
        /// Gets or sets Columns array
        /// </summary>
        [JsonProperty("columns")]
        public ColumnInfo[] Columns { get; set; }
    }
}
