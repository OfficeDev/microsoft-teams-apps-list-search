// <copyright file="ColumnInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Column Info for SharePoint response
    /// </summary>
    public class ColumnInfo
    {
        /// <summary>
        /// Gets or sets display name
        /// </summary>
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
