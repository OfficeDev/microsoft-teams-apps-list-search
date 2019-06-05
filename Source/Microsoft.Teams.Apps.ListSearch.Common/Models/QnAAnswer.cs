// <copyright file="QnAAnswer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Structure of QnA Answer.
    /// </summary>
    public class QnAAnswer
    {
        /// <summary>
        /// Gets or sets Questions
        /// </summary>
        [JsonProperty("questions")]
        public List<string> Questions { get; set; }

        /// <summary>
        /// Gets or sets Answer
        /// </summary>
        [JsonProperty("answer")]
        public string Answer { get; set; }

        /// <summary>
        /// Gets or sets Score
        /// </summary>
        [JsonProperty("score")]
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets Id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets Source
        /// </summary>
        [JsonProperty("source")]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets Top
        /// </summary>
        [JsonProperty("top")]
        public string Top { get; set; }
    }
}
