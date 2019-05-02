// <copyright file="TokenEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Models
{
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents Token entity in storage.
    /// </summary>
    public class TokenEntity : TableEntity // TODO: remove this and change to key vault.
    {
        /// <summary>
        /// Gets or sets access token
        /// </summary>
        [JsonProperty("AccessToken")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets refresh token
        /// </summary>
        [JsonProperty("RefreshToken")]
        public string RefreshToken { get; set; }
    }
}