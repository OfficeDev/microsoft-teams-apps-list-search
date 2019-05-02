// <copyright file="GetListContentsResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Get List Contents Response
    /// </summary>
    public class GetListContentsResponse
    {
        /// <summary>
        /// Gets or sets odata context
        /// </summary>
        [JsonProperty("@odata.context")]
        public string ODataContext { get; set; }

        /// <summary>
        /// Gets or sets odata next link
        /// </summary>
        [JsonProperty("@odata.nextLink")]
        public string ODataNextLink { get; set; }

        /// <summary>
        /// Gets or sets value
        /// </summary>
        [JsonProperty("value")]
        public object Value { get; set; }
    }
}