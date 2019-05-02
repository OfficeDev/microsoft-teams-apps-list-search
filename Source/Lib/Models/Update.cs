// <copyright file="Update.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Lib.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Update data
    /// </summary>
    public class Update
    {
        /// <summary>
        /// Gets or sets the list of files
        /// </summary>
        [JsonProperty("files")]
        public List<File> Files { get; set; }
    }
}
