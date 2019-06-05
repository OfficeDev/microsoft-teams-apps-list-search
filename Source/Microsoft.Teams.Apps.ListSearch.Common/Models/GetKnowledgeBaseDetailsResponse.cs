// <copyright file="GetKnowledgeBaseDetailsResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Get Knowledge Base Details Response
    /// </summary>
    public class GetKnowledgeBaseDetailsResponse
    {
        /// <summary>
        /// Gets or sets id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets hostname
        /// </summary>
        [JsonProperty("hostName")]
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets last accessed time stamp
        /// </summary>
        [JsonProperty("lastAccessedTimestamp")]
        public string LastAccessedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets last changed time stamp
        /// </summary>
        [JsonProperty("lastChangedTimestamp")]
        public string LastChangedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets user id
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets urls
        /// </summary>
        [JsonProperty("urls")]
        public List<string> Urls { get; set; }

        /// <summary>
        /// Gets or sets sources
        /// </summary>
        [JsonProperty("sources")]
        public List<string> Sources { get; set; }

        /// <summary>
        /// Gets or sets language
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets created time stamp
        /// </summary>
        [JsonProperty("createdTimestamp")]
        public string CreatedTimestamp { get; set; }
    }
}
