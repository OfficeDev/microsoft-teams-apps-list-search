// <copyright file="KBInfoHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.ListSearch.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Helper class for accessing KB Info
    /// </summary>
    public class KBInfoHelper
    {
        private const int InsertSuccessResponseCode = 204;

        private readonly CloudTable cloudTable;
        private readonly Lazy<Task> initializeTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="KBInfoHelper"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage.</param>
        public KBInfoHelper(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            this.cloudTable = tableClient.GetTableReference(StorageInfo.KBInfoTableName);

            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync());
        }

        /// <summary>
        /// Get KB Info item from storage.
        /// </summary>
        /// <param name="kbId">Kb Id</param>
        /// <returns>Task that resolves to <see cref="KBInfo"/> object for the searched kbId.</returns>
        public async Task<KBInfo> GetKBInfo(string kbId)
        {
            await this.initializeTask.Value;

            TableOperation searchOperation = TableOperation.Retrieve<KBInfo>(StorageInfo.KBInfoTablePartitionKey, kbId);
            TableResult searchResult = await this.cloudTable.ExecuteAsync(searchOperation);

            return (KBInfo)searchResult.Result;
        }

        /// <summary>
        /// Returns all specified fields for entries from the table.
        /// </summary>
        /// <param name="fields">Fields to be retrieved.</param>
        /// <returns><see cref="Task"/> that resolves to <see cref="List{KBInfo}"/>.</returns>
        public async Task<List<KBInfo>> GetAllKBs(string[] fields)
        {
            await this.initializeTask.Value;

            List<KBInfo> kbList = new List<KBInfo>();
            TableQuery<KBInfo> projectionQuery = new TableQuery<KBInfo>().Select(fields);
            TableContinuationToken token = null;

            do
            {
                TableQuerySegment<KBInfo> seg = await this.cloudTable.ExecuteQuerySegmentedAsync(projectionQuery, token);
                token = seg.ContinuationToken;
                kbList.AddRange(seg.Results);
            }
            while (token != null);

            return kbList;
        }

        /// <summary>
        /// Insert or merge KBInfo entity.
        /// </summary>
        /// <param name="kBInfo">Kb Info entity.</param>
        /// <returns><see cref="Task"/> that represents Insert or Merge function.</returns>
        public async Task InsertOrMergeKBInfo(KBInfo kBInfo)
        {
            await this.initializeTask.Value;

            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(kBInfo);
            TableResult insertOrMergeResult = await this.cloudTable.ExecuteAsync(insertOrMergeOperation);
            if (insertOrMergeResult.HttpStatusCode != InsertSuccessResponseCode)
            {
                throw new Exception($"HTTP Error code - {insertOrMergeResult.HttpStatusCode}");
            }
        }

        /// <summary>
        /// Deletes KB from KBInfo Storage table
        /// </summary>
        /// <param name="kbId">Kb id</param>
        /// <returns> representing the asynchronous operation</returns>
        public async Task DeleteKB(string kbId)
        {
            await this.initializeTask.Value;

            var entity = new DynamicTableEntity(StorageInfo.KBInfoTablePartitionKey, kbId);
            entity.ETag = "*";

            await this.cloudTable.ExecuteAsync(TableOperation.Delete(entity));
        }

        private async Task InitializeAsync()
        {
            await this.cloudTable.CreateIfNotExistsAsync();
        }
    }
}
