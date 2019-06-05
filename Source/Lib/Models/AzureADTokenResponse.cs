// <copyright file="AzureADTokenResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Lib.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Refresh Token Response.
    /// </summary>
    public class AzureADTokenResponse
    {
        /// <summary>
        /// Gets or sets Token Type.
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets scope.
        /// </summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets Expires In.
        /// </summary>
        [JsonProperty("expires_in")]
        public double ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets Ext Expires In.
        /// </summary>
        [JsonProperty("ext_expires_in")]
        public string ExtExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets Access Token.
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets Refresh Token.
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
