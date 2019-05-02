// <copyright file="TaskSubmitResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents Task Envelope for task module
    /// </summary>
    public class TaskSubmitResponse
    {
        /// <summary>
        /// Gets or sets Task
        /// </summary>
        [JsonProperty("task")]
        public TaskContinueResult Task { get; set; }
    }
}