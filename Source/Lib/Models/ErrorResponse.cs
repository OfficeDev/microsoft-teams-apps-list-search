// <copyright file="ErrorResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Lib.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Error Response
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets error
        /// </summary>
        [JsonProperty("error")]
        public Error Error { get; set; }
    }
}