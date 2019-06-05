// <copyright file="HomeViewModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Configuration.Models
{
    using System.Collections.Generic;
    using Common.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Home page view model
    /// </summary>
    public class HomeViewModel
    {
        /// <summary>
        /// Gets or sets SharePointUserUpn
        /// </summary>
        [JsonProperty("SharePointUserUpn")]
        public string SharePointUserUpn { get; set; }

        /// <summary>
        /// Gets or sets Knowledge base List
        /// </summary>
        [JsonProperty("KBList")]
        public List<KBInfo> KBList { get; set; }
    }
}
