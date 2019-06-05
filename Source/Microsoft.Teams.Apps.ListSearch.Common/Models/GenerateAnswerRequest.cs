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
        public GenerateAnswerRequest(string question, int top)
        {
            this.Question = question;
            this.Top = top;
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
        public int Top { get; set; }
    }
}
