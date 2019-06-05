// <copyright file="GetKnowledgeBaseResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Response of GetKnowledgeBase API
    /// </summary>
    public class GetKnowledgeBaseResponse
    {
        /// <summary>
        /// Gets or sets the list of Knowledge bases
        /// </summary>
        [JsonProperty("knowledgebases")]
        public List<KnowledgeBase> KnowledgeBases { get; set; }
    }
}
