// <copyright file="TaskInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Models
{
    using Microsoft.Bot.Connector;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents Task Info for task module
    /// </summary>
    public class TaskInfo
    {
        /// <summary>
        /// Gets or sets the url
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the card
        /// </summary>
        [JsonProperty("card")]
        public Attachment Card { get; set; }

        /// <summary>
        /// Gets or sets the title
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the height
        /// </summary>
        [JsonProperty("height")]
        public object Height { get; set; }

        /// <summary>
        /// Gets or sets the width
        /// </summary>
        [JsonProperty("width")]
        public object Width { get; set; }

        /// <summary>
        /// Gets or sets the fallback url
        /// </summary>
        [JsonProperty("fallbackUrl")]
        public string FallbackUrl { get; set; }

        /// <summary>
        /// Gets or sets the completion bot id
        /// </summary>
        [JsonProperty("completionBotId")]
        public string CompletionBotId { get; set; }

        /// <summary>
        /// Get Json Serialized object.
        /// </summary>
        /// <returns>Serialized Json string</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}