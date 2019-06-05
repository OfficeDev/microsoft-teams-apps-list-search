// <copyright file="GenerateAnswerResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Structure of Response from Generate Answer API.
    /// </summary>
    public class GenerateAnswerResponse
    {
        /// <summary>
        /// Gets or sets list of answers.
        /// </summary>
        [JsonProperty("answers")]
        public List<QnAAnswer> Answers { get; set; }
    }
}
