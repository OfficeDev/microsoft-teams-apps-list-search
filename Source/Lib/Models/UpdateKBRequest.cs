// <copyright file="UpdateKBRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Lib.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Update KB request
    /// </summary>
    public class UpdateKBRequest
    {
        /// <summary>
        /// Gets or sets the add operation
        /// </summary>
        [JsonProperty("add")]
        public Add Add { get; set; }

        /// <summary>
        /// Gets or sets the delete operation
        /// </summary>
        [JsonProperty("delete")]
        public Delete Delete { get; set; }
    }
}
