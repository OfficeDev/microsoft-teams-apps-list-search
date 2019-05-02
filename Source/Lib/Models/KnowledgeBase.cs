// <copyright file="KnowledgeBase.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Lib.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Structure of Knowledgebase.
    /// </summary>
    public class KnowledgeBase
    {
        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the hostname.
        /// </summary>
        [JsonProperty("hostName")]
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the last accessed time stamp.
        /// </summary>
        [JsonProperty("lastAccessedTimestamp")]
        public string LastAccessedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the last changed time stamp.
        /// </summary>
        [JsonProperty("lastChangedTimestamp")]
        public string LastChangedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the last published time stamp.
        /// </summary>
        [JsonProperty("lastPublishedTimestamp")]
        public string LastPublishedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the Urls.
        /// </summary>
        [JsonProperty("urls")]
        public List<string> Urls { get; set; }

        /// <summary>
        /// Gets or sets the sources.
        /// </summary>
        [JsonProperty("sources")]
        public List<string> Sources { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the Created time stamp.
        /// </summary>
        [JsonProperty("createdTimestamp")]
        public string CreatedTimestamp { get; set; }
    }
}
