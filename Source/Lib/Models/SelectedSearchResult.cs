// <copyright file="SelectedSearchResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Lib.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Structure of response from Task Module for generating the adaptive card.
    /// </summary>
    public class SelectedSearchResult
    {
        /// <summary>
        /// Gets or sets Kb id
        /// </summary>
        [JsonProperty("kbId")]
        public string KBId { get; set; }

        /// <summary>
        /// Gets or sets answers
        /// </summary>
        [JsonProperty("answers")]
        public List<DeserializedAnswer> Answers { get; set; }

        /// <summary>
        /// Gets or sets question
        /// </summary>
        [JsonProperty("question")]
        public string Question { get; set; }

        /// <summary>
        /// Gets or sets sharePoint url
        /// </summary>
        [JsonProperty("sharepointURL")]
        public string SharePointURL { get; set; }

        /// <summary>
        /// Gets or sets Id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
