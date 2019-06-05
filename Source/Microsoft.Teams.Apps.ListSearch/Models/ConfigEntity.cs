// <copyright file="ConfigEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Models
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Structure of Config Entity in table storage.
    /// </summary>
    public class ConfigEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets property that represents Data field of the Table Entity.
        /// </summary>
        public string Data { get; set; }
    }
}