// <copyright file="TaskContinueResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Task Continue Result
    /// </summary>
    public class TaskContinueResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskContinueResult"/> class.
        /// </summary>
        /// <param name="taskInfo">Task info for task continue result.</param>
        public TaskContinueResult(TaskInfo taskInfo)
        {
            this.TaskInfo = taskInfo;
            this.Type = ComposeExtensionResultType.TaskContinue;
        }

        /// <summary>
        /// Gets or sets <see cref="TaskInfo"/>
        /// </summary>
        [JsonProperty("value")]
        public TaskInfo TaskInfo { get; set; }

        /// <summary>
        /// Gets or sets Type of task
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}