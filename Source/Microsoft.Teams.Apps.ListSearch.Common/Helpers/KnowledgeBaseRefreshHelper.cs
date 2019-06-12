// <copyright file="KnowledgeBaseRefreshHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Common.Extensions;
    using Microsoft.Teams.Apps.Common.Logging;
    using Microsoft.Teams.Apps.ListSearch.Common.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Helper with logic to refresh the KB.
    /// </summary>
    public class KnowledgeBaseRefreshHelper
    {
        private const string JsonFileExtension = ".json";

        private readonly HttpClient httpClient;
        private readonly string qnaMakerSubcriptionKey;
        private readonly BlobHelper blobHelper;
        private readonly KBInfoHelper kbInfoHelper;
        private readonly GraphHelper graphHelper;
        private readonly ILogProvider logProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeBaseRefreshHelper"/> class.
        /// </summary>
        /// <param name="httpClient">Http client to be used.</param>
        /// <param name="blobHelper">Blob helper</param>
        /// <param name="kbInfoHelper">KB info helper</param>
        /// <param name="graphHelper">Graph helper</param>
        /// <param name="qnaMakerSubscriptionKey">QnAMaker subscription key</param>
        /// <param name="logProvider">Log provider to use</param>
        public KnowledgeBaseRefreshHelper(HttpClient httpClient, BlobHelper blobHelper, KBInfoHelper kbInfoHelper, GraphHelper graphHelper, string qnaMakerSubscriptionKey, ILogProvider logProvider)
        {
            this.httpClient = httpClient ?? throw new System.ArgumentNullException(nameof(httpClient));
            this.blobHelper = blobHelper;
            this.kbInfoHelper = kbInfoHelper;
            this.graphHelper = graphHelper;
            this.qnaMakerSubcriptionKey = qnaMakerSubscriptionKey;
            this.logProvider = logProvider;
        }

        /// <summary>
        /// Refresh the data in the given knowledge base.
        /// </summary>
        /// <param name="kb">Knowledge base info</param>
        /// <returns>Tracking task</returns>
        public async Task RefreshKnowledgeBaseAsync(KBInfo kb)
        {
            this.logProvider.LogInfo($"Refreshing knowledge base {kb.KBId}");
            this.logProvider.LogDebug($"Last successful refresh was on {kb.LastRefreshDateTime}");
            this.logProvider.LogDebug($"Last refresh attempt was on {kb.LastRefreshAttemptDateTime}, with status {kb.LastRefreshAttemptError ?? "success"}");

            kb.LastRefreshAttemptDateTime = DateTime.UtcNow;

            try
            {
                ColumnInfo questionColumn = JsonConvert.DeserializeObject<ColumnInfo>(kb.QuestionField);
                Dictionary<string, Uri> blobInfoTemp = new Dictionary<string, Uri>();
                GetListContentsResponse listContents = null;

                do
                {
                    listContents = await this.GetListContents(kb.SharePointListId, kb.AnswerFields, questionColumn.Name, kb.SharePointSiteId, listContents?.ODataNextLink ?? null);

                    string blobName = Guid.NewGuid().ToString() + JsonFileExtension;
                    string blobUrl = await this.blobHelper.UploadBlobAsync(JsonConvert.SerializeObject(listContents), blobName);
                    blobInfoTemp.Add(blobName, new Uri(blobUrl));

                    this.logProvider.LogDebug($"Fetched page of list contents, stored as {blobName}");
                }
                while (!string.IsNullOrEmpty(listContents.ODataNextLink));

                await this.UpdateKnowledgeBaseAsync(kb.KBId, blobInfoTemp, JsonConvert.DeserializeObject<ColumnInfo>(kb.QuestionField).Name, this.blobHelper);

                // Delete all existing blobs for this KB
                foreach (string blobName in blobInfoTemp.Keys)
                {
                    await this.blobHelper.DeleteBlobAsync(blobName);
                    this.logProvider.LogDebug($"Deleted temporary blob {blobName}");
                }

                this.logProvider.LogDebug($"Refresh of KB succeeded");

                kb.LastRefreshDateTime = DateTime.UtcNow;
                kb.LastRefreshAttemptError = null;
                await this.kbInfoHelper.InsertOrMergeKBInfo(kb);
            }
            catch (Exception ex)
            {
                this.logProvider.LogError($"Refresh of KB failed: {ex.Message}", ex);

                kb.LastRefreshAttemptError = ex.ToString();
                await this.kbInfoHelper.InsertOrMergeKBInfo(kb);
            }

            this.logProvider.LogInfo($"Finished refreshing KB {kb.KBId}, with status {kb.LastRefreshAttemptError ?? "success"}");
        }

        /// <summary>
        /// Refreshes KB - Updates and Publishes KB.
        /// </summary>
        /// <param name="kbId">Id of KB to be refreshed</param>
        /// <param name="blobInfo">Details of source blob files</param>
        /// <param name="questionField">question field</param>
        /// <param name="blobHelper">Blob helper object</param>
        /// <returns>Task that represents refresh operation.</returns>
        private async Task UpdateKnowledgeBaseAsync(string kbId, Dictionary<string, Uri> blobInfo, string questionField, BlobHelper blobHelper)
        {
            QnAMakerService qnAMakerService = new QnAMakerService(this.httpClient, this.qnaMakerSubcriptionKey);

            this.logProvider.LogDebug($"Deleting existing KB sources");
            bool deleteSourcesResult = await this.DeleteExistingSources(qnAMakerService, kbId);

            this.logProvider.LogDebug($"Adding new KB sources ({blobInfo.Count} files)");
            bool addSourcesResult = true;
            if (blobInfo.Count < 10)
            {
                // Fewer than 10 files
                addSourcesResult = await this.AddNewSources(kbId, blobInfo, questionField, qnAMakerService);
            }
            else
            {
                // More than 10 files, have to add them in batches
                Dictionary<string, Uri> blobInfoBatch = new Dictionary<string, Uri>();
                int filesExtracted = 0;
                int counter = 0;

                foreach (var entry in blobInfo)
                {
                    // files still in blob info to be included in batch
                    if (counter < 10 && filesExtracted < blobInfo.Count)
                    {
                        counter++;
                        filesExtracted++;
                        blobInfoBatch.Add(entry.Key, entry.Value);
                        continue;
                    }

                    // no more file left to include in batch
                    this.logProvider.LogDebug($"Adding next batch of sources");
                    addSourcesResult = addSourcesResult && await this.AddNewSources(kbId, blobInfoBatch, questionField, qnAMakerService);
                    blobInfoBatch.Clear();

                    if (filesExtracted < blobInfo.Count)
                    {
                        counter = 1;
                        filesExtracted++;
                        blobInfoBatch.Add(entry.Key, entry.Value);
                    }
                }

                if (blobInfoBatch.Count > 0)
                {
                    this.logProvider.LogDebug($"Adding final batch of sources");
                    addSourcesResult = addSourcesResult && await this.AddNewSources(kbId, blobInfoBatch, questionField, qnAMakerService);
                }
            }

            // if delete or any of the updates fails, KB is not published. Retry on next refresh.
            if (addSourcesResult && deleteSourcesResult)
            {
                this.logProvider.LogDebug($"Publishing updated knowledge base");
                await qnAMakerService.PublishKB(kbId);
            }

            this.logProvider.LogInfo($"Updated knowledge base {kbId}");
        }

        /// <summary>
        /// Deletes sources from KB
        /// </summary>
        /// <param name="qnAMakerService">instance qna maker service</param>
        /// <param name="kbId">Knowledge base ID</param>
        /// <returns><see cref="Task"/> that resolves to a <see cref="bool"/> which represents success or failure of operation.</returns>
        private async Task<bool> DeleteExistingSources(QnAMakerService qnAMakerService, string kbId)
        {
            GetKnowledgeBaseDetailsResponse kbDetails = await qnAMakerService.GetKnowledgeBaseDetails(kbId);
            UpdateKBRequest deleteSourcesRequest = new UpdateKBRequest()
            {
                Delete = new Delete()
                {
                    Sources = kbDetails.Sources,
                },
            };
            QnAMakerResponse deleteSourcesResult = await qnAMakerService.UpdateKB(kbId, deleteSourcesRequest);
            string deleteSourcesResultState = await qnAMakerService.AwaitOperationCompletionState(deleteSourcesResult);

            this.logProvider.LogDebug($"Add operation completed with status {deleteSourcesResultState}");
            return qnAMakerService.IsOperationSuccessful(deleteSourcesResultState);
        }

        /// <summary>
        /// Adds new sources to the kb.
        /// </summary>
        /// <param name="kbId">kb id</param>
        /// <param name="blobInfo"><see cref="Dictionary{TKey, TValue}"/> with keys as blob names and values as Uris of the corresponding blobs.</param>
        /// <param name="questionField">question field</param>
        /// <param name="qnAMakerService">instance of qna maker servcice</param>
        /// <returns><see cref="Task"/> that resolves to a <see cref="bool"/> which represents success or failure of operation.</returns>
        private async Task<bool> AddNewSources(string kbId, Dictionary<string, Uri> blobInfo, string questionField, QnAMakerService qnAMakerService)
        {
            List<File> files = new List<File>();
            foreach (var blobData in blobInfo)
            {
                files.Add(
                    new File()
                    {
                        FileName = blobData.Key,
                        FileUri = blobData.Value.ToString(),
                        ExtractionOptions = new ExtractionOptions()
                        {
                            ColumnMapping = new ColumnMapping()
                            {
                                Question = questionField,
                            },
                        },
                    });
            }

            UpdateKBRequest addSourcesRequest = new UpdateKBRequest()
            {
                Add = new Add()
                {
                    Files = files,
                },
            };

            QnAMakerResponse addSourcesResult = await qnAMakerService.UpdateKB(kbId, addSourcesRequest);
            string addSourcesResultState = await qnAMakerService.AwaitOperationCompletionState(addSourcesResult);

            this.logProvider.LogDebug($"Add operation completed with status {addSourcesResultState}");
            return qnAMakerService.IsOperationSuccessful(addSourcesResultState);
        }

        /// <summary>
        /// Get the contents of the list.
        /// </summary>
        /// <param name="listId">Id of the list to be fetched.</param>
        /// <param name="answerFields">Answer fields to be used for KB.</param>
        /// <param name="questionField">question field.</param>
        /// <param name="sharePointSiteId">site id of sharepoint site.</param>
        /// <param name="odataNextUrl">odata next url</param>
        /// <returns><see cref="Task"/> that resolves to <see cref="GetListContentsResponse"/> which represents the list response.</returns>
        private async Task<GetListContentsResponse> GetListContents(string listId, string answerFields, string questionField, string sharePointSiteId, string odataNextUrl)
        {
            var fieldsToFetch = JsonConvert.DeserializeObject<List<ColumnInfo>>(answerFields)
                .Select(field => field.Name)
                .Concat(new string[] { questionField, "id" });

            string responseBody = await this.graphHelper.GetListContentsAsync(
                listId: listId,
                fieldsToFetch: fieldsToFetch,
                sharePointSiteId: sharePointSiteId,
                odataNextUrl: odataNextUrl);
            return JsonConvert.DeserializeObject<GetListContentsResponse>(responseBody);
        }
    }
}
