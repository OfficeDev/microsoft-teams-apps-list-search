// <copyright file="StorageInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Lib.Models
{
    /// <summary>
    /// References to storage tables.
    /// </summary>
    public class StorageInfo
    {
        /// <summary>
        /// Config Table
        /// </summary>
        public static readonly string ConfigTableName = "Config";

        /// <summary>
        /// KB Info Table
        /// </summary>
        public static readonly string KBInfoTableName = "KBInfo";

        /// <summary>
        /// Token Table
        /// </summary>
        public static readonly string TokenTableName = "Token";

        /// <summary>
        /// Blob Container
        /// </summary>
        public static readonly string BlobContainerName = "list-search-blob";
    }
}
