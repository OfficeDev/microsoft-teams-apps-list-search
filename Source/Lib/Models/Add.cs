// <copyright file="Add.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Lib.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Add data
    /// </summary>
    public class Add
    {
        /// <summary>
        /// Gets or sets the list of files
        /// </summary>
        [JsonProperty("files")]
        public List<File> Files { get; set; }
    }
}
