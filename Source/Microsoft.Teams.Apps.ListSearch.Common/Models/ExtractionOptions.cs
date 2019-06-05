// <copyright file="ExtractionOptions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Extraction Option
    /// </summary>
    public class ExtractionOptions
    {
        /// <summary>
        /// Fixed string for format
        /// </summary>
        [JsonProperty("format")]
        public const string Format = "SharepointListJson";

        /// <summary>
        /// Gets or sets column mapping
        /// </summary>
        [JsonProperty("columnMapping")]
        public ColumnMapping ColumnMapping { get; set; }
    }
}
