// <copyright file="StorageInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    /// <summary>
    /// References to storage tables.
    /// </summary>
    public class StorageInfo
    {
        /// <summary>
        /// Config Table
        /// </summary>
        public const string ConfigTableName = "Config";

        /// <summary>
        /// KB Info Table
        /// </summary>
        public const string KBInfoTableName = "KBInfo";

        /// <summary>
        /// KB Info Table Partition key
        /// </summary>
        public const string KBInfoTablePartitionKey = "KbInfo";

        /// <summary>
        /// Token Table
        /// </summary>
        public const string TokenTableName = "Token";

        /// <summary>
        /// Blob Container
        /// </summary>
        public const string BlobContainerName = "list-search-blob";
    }
}
