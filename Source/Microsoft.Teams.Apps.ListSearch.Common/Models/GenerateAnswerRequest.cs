// <copyright file="GenerateAnswerRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Structure of Request for Generate Answer API.
    /// </summary>
    public class GenerateAnswerRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateAnswerRequest"/> class.
        /// </summary>
        /// <param name="question">Question asked by the user</param>
        /// <param name="top">Number of results to be fetched</param>
        /// <param name="score">Score to filter the search result</param>
        public GenerateAnswerRequest(string question = null, int? top = null, int? score = null)
        {
            this.RankerType = "AutoSuggestQuestion";
            this.Question = question;
            this.Top = top;
            this.ScoreThreshold = score;
        }

        /// <summary>
        /// Gets or sets question asked by the user
        /// </summary>
        [JsonProperty("question")]
        public string Question { get; set; }

        /// <summary>
        /// Gets or sets number of results to be fetched
        /// </summary>
        [JsonProperty("top")]
        public int? Top { get; set; }

        /// <summary>
        /// Gets or sets ScoreTherhold
        /// </summary>
        [JsonProperty("scoreThreshold")]
        public int? ScoreThreshold { get; set; }

        /// <summary>
        /// Gets or sets Ranker type
        /// </summary>
        [JsonProperty("RankerType")]
        public string RankerType { get; set; }
    }
}
