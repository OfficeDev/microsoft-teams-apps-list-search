// <copyright file="KBInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Models
{
    using System;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents KB Info in storage.
    /// </summary>
    public class KBInfo : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KBInfo"/> class.
        /// </summary>
        public KBInfo()
        {
        }

        /// <summary>
        /// Gets the kb id
        /// </summary>
        public string KBId
        {
            get
            {
                return this.RowKey;
            }
        }

        /// <summary>
        /// Gets or sets name of the kb
        /// </summary>
        [JsonProperty("KBName")]
        public string KBName { get; set; }

        /// <summary>
        /// Gets or sets sharepoint url of the list
        /// </summary>
        [JsonProperty("SharePointUrl")]
        public string SharePointUrl { get; set; }

        /// <summary>
        /// Gets or sets sharepoint site id.
        /// </summary>
        [JsonProperty("SharePointSiteId")]
        public string SharePointSiteId { get; set; }

        /// <summary>
        /// Gets or sets question field
        /// </summary>
        [JsonProperty("QuestionField")]
        public string QuestionField { get; set; }

        /// <summary>
        /// Gets or sets answer field
        /// </summary>
        [JsonProperty("AnswerField")]
        public string AnswerFields { get; set; }

        /// <summary>
        /// Gets or sets last refresh date time
        /// </summary>
        [JsonProperty("LastRefreshDateTime")]
        public DateTime LastRefreshDateTime { get; set; }

        /// <summary>
        /// Gets or sets refresh frequency in hours
        /// </summary>
        [JsonProperty("RefreshFrequencyInHours")]
        public int RefreshFrequencyInHours { get; set; }

        /// <summary>
        /// Gets or sets SharePoint list id
        /// </summary>
        [JsonProperty("SharePointListId")]
        public string SharePointListId { get; set; }
    }
}