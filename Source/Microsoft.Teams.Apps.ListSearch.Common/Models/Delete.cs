// <copyright file="Delete.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Delete data
    /// </summary>
    public class Delete
    {
        /// <summary>
        /// Gets or sets sources
        /// </summary>
        [JsonProperty("sources")]
        public List<string> Sources { get; set; }
    }
}
