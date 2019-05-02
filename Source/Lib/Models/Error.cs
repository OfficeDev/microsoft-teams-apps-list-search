// <copyright file="Error.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Lib.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Error
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Gets or sets code
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets code
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets details
        /// </summary>
        [JsonProperty("details")]
        public List<Details> Details { get; set; }
    }
}
