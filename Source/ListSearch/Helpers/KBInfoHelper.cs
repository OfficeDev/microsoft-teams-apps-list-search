// <copyright file="KBInfoHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Helpers
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading.Tasks;
    using System.Web.Http;
    using ListSearch.Models;
    using Microsoft.Azure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Helper class for accessing KB Info
    /// </summary>
    public class KBInfoHelper
    {
        private const string PartitionKey = "KbInfo";

        private static readonly string KbInfoConfigTableName = StorageInfo.KBInfoTableName;
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient cloudTableClient;
        private readonly CloudTable cloudTable;
        private readonly int insertSuccessResponseCode = 204;

        /// <summary>
        /// Initializes a new instance of the <see cref="KBInfoHelper"/> class.
        /// </summary>
        public KBInfoHelper()
        {
            this.storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            this.cloudTableClient = this.storageAccount.CreateCloudTableClient();
            this.cloudTable = this.cloudTableClient.GetTableReference(KbInfoConfigTableName);
        }

        /// <summary>
        /// Get KB Info item from stoarge.
        /// </summary>
        /// <param name="kbId">Kb Id</param>
        /// <returns>Task that resolves to <see cref="KBInfo"/> object for the searched kbId.</returns>
        public async Task<KBInfo> GetKBInfo(string kbId)
        {
            TableOperation searchOperation = TableOperation.Retrieve<KBInfo>(PartitionKey, kbId);
            TableResult searchResult = await this.cloudTable.ExecuteAsync(searchOperation);
            return (KBInfo)searchResult.Result;
        }

        /// <summary>
        /// Get answer fields from storage.
        /// </summary>
        /// <param name="kbId">Id of the KB for which Metadata fields are to be fetched.</param>
        /// <returns><see cref="Task"/> that resolves to list of answer fields.</returns>
        public async Task<List<string>> GetAnswerFields(string kbId)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<KBInfo>(PartitionKey, kbId);
            TableResult retrievedResult = await this.cloudTable.ExecuteAsync(retrieveOperation);
            KBInfo result = (KBInfo)retrievedResult.Result;
            List<string> retObj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result.AnswerFields);
            return retObj;
        }

        /// <summary>
        /// Returns all specified fields for entries from the table.
        /// </summary>
        /// <param name="fields">Fields to be retireved.</param>
        /// <returns><see cref="Task"/> that resolves to <see cref="List{KBInfo}"/>.</returns>
        public async Task<List<KBInfo>> GetAllKBs(string[] fields)
        {
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
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(kBInfo);
            TableResult insertOrMergeResult = await this.cloudTable.ExecuteAsync(insertOrMergeOperation);
            if (insertOrMergeResult.HttpStatusCode != this.insertSuccessResponseCode)
            {
                throw new HttpResponseException((System.Net.HttpStatusCode)insertOrMergeResult.HttpStatusCode); // TODO: Handle Exception
            }
        }
    }
}