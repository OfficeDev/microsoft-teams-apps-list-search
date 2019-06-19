// <copyright file="ParentReference.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// List Parent Reference
    /// </summary>
    public class ParentReference
    {
        /// <summary>
        /// Gets or sets SiteId
        /// </summary>
        [JsonProperty("siteId")]
        public string SiteId { get; set; }
    }
}
