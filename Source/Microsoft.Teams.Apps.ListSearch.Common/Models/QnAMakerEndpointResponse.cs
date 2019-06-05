// <copyright file="QnAMakerEndpointResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// QNAMaker EndPoint Keys Response
    /// </summary>
    public class QnAMakerEndpointResponse
    {
        /// <summary>
        /// Gets or sets primaryEndpointKey
        /// </summary>
        [JsonProperty("primaryEndpointKey")]
        public string PrimaryEndpointKey { get; set; }

        /// <summary>
        /// Gets or sets secondaryEndpointKey
        /// </summary>
        [JsonProperty("secondaryEndpointKey")]
        public string SecondaryEndpointKey { get; set; }

        /// <summary>
        /// Gets or sets installedVersion
        /// </summary>
        [JsonProperty("installedVersion")]
        public string InstalledVersion { get; set; }

        /// <summary>
        /// Gets or sets lastStableVersion
        /// </summary>
        [JsonProperty("lastStableVersion")]
        public string LastStableVersion { get; set; }
    }
}
