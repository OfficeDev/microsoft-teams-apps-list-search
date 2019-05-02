// <copyright file="File.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Lib.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// File
    /// </summary>
    public class File
    {
        /// <summary>
        /// Gets or sets file name
        /// </summary>
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets file uri
        /// </summary>
        [JsonProperty("fileUri")]
        public string FileUri { get; set; }

        /// <summary>
        /// Gets or sets list of extraction options
        /// </summary>
        [JsonProperty("extractionOptions")]
        public ExtractionOptions ExtractionOptions { get; set; }
    }
}
