// <copyright file="CreateKBRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Create KB Request
    /// </summary>
    public class CreateKBRequest
    {
        /// <summary>
        /// Gets or sets name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets files
        /// </summary>
        [JsonProperty("files")]
        public List<File> Files { get; set; }
    }
}
