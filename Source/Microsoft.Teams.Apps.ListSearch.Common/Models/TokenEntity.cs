// <copyright file="TokenEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using System;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents Token entity in storage.
    /// </summary>
    public class TokenEntity : TableEntity
    {
        /// <summary>
        /// Gets the token type
        /// </summary>
        public string TokenType
        {
            get { return this.RowKey; }
        }

        /// <summary>
        /// Gets or sets email
        /// </summary>
        [JsonProperty("UserPrincipalName")]
        public string UserPrincipalName { get; set; }

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

        /// <summary>
        /// Gets or sets the time when the access token expires
        /// </summary>
        [JsonProperty("ExpiresIn")]
        public DateTime ExpiryDateTime { get; set; }

        /// <summary>
        /// Gets or sets token scopes
        /// </summary>
        [JsonProperty("Scopes")]
        public string Scopes { get; set; }
    }
}
