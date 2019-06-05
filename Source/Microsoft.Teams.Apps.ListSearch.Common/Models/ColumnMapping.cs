// <copyright file="ColumnMapping.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Column mapping
    /// </summary>
    public class ColumnMapping
    {
        /// <summary>
        /// Gets or sets question field
        /// </summary>
        [JsonProperty("question")]
        public string Question { get; set; }
    }
}
