// <copyright file="QnAMakerResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Get operation details response
    /// </summary>
    public class QnAMakerResponse
    {
        /// <summary>
        /// Gets or sets operation state
        /// </summary>
        [JsonProperty("operationState")]
        public string OperationState { get; set; }

        /// <summary>
        /// Gets or sets created time stamp
        /// </summary>
        [JsonProperty("createdTimestamp")]
        public string CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets last action timestamp
        /// </summary>
        [JsonProperty("lastActionTimestamp")]
        public string LastActionTimestamp { get; set; }

        /// <summary>
        /// Gets or sets resource location
        /// </summary>
        [JsonProperty("resourceLocation")]
        public string ResourceLocation { get; set; }

        /// <summary>
        /// Gets or sets user id
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets operation id
        /// </summary>
        [JsonProperty("operationId")]
        public string OperationId { get; set; }

        /// <summary>
        /// Gets or sets error response
        /// </summary>
        [JsonProperty("errorResponse")]
        public ErrorResponse ErrorResponse { get; set; }
    }
}
