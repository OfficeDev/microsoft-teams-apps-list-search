// <copyright file="Details.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Details
    /// </summary>
    public class Details
    {
        /// <summary>
        /// Gets or sets code
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets target
        /// </summary>
        [JsonProperty("target")]
        public string Target { get; set; }
    }
}