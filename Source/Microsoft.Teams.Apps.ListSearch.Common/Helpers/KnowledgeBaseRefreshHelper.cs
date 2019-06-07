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

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeBaseRefreshHelper"/> class.
        /// </summary>
        /// <param name="httpClient">Http client to be used.</param>
        /// <param name="blobHelper">Blob helper</param>
        /// <param name="kbInfoHelper">KB info helper</param>
        /// <param name="graphHelper">Graph helper</param>
        /// <param name="qnaMakerSubscriptionKey">QnAMaker subscription key</param>
        public KnowledgeBaseRefreshHelper(HttpClient httpClient, BlobHelper blobHelper, KBInfoHelper kbInfoHelper, GraphHelper graphHelper, string qnaMakerSubscriptionKey)
        {
            this.httpClient = httpClient ?? throw new System.ArgumentNullException(nameof(httpClient));
            this.blobHelper = blobHelper;
            this.kbInfoHelper = kbInfoHelper;
            this.graphHelper = graphHelper;
            this.qnaMakerSubcriptionKey = qnaMakerSubscriptionKey;
        }

        /// <summary>
        /// Refresh the data in the given knowledge base.
        /// </summary>
        /// <param name="kb">Knowledge base info</param>
        /// <returns>Tracking task</returns>
        public async Task RefreshKnowledgeBaseAsync(KBInfo kb)
        {
            kb.LastRefreshAttemptDateTime = DateTime.UtcNow;

            try
            {
                ColumnInfo questionColumn = JsonConvert.DeserializeObject<ColumnInfo>(kb.QuestionField);
                Dictionary<string, Uri> blobInfoTemp = new Dictionary<string, Uri>();
                GetListContentsResponse listContents = null;

                do
                {
                    string blobName = Guid.NewGuid().ToString() + JsonFileExtension;

                    listContents = await this.GetListContents(kb.SharePointListId, kb.AnswerFields, questionColumn.Name, kb.SharePointSiteId, listContents?.ODataNextLink ?? null);

                    string blobUrl = await this.blobHelper.UploadBlobAsync(JsonConvert.SerializeObject(listContents), blobName);
                    blobInfoTemp.Add(blobName, new Uri(blobUrl));
                }
                while (!string.IsNullOrEmpty(listContents.ODataNextLink));

                await this.UpdateKnowledgeBaseAsync(kb.KBId, blobInfoTemp, JsonConvert.DeserializeObject<ColumnInfo>(kb.QuestionField).Name, this.blobHelper);

                // Delete all existing blobs for this KB
                foreach (string blobName in blobInfoTemp.Keys)
                {
                    await this.blobHelper.DeleteBlobAsync(blobName);
                }

                kb.LastRefreshDateTime = DateTime.UtcNow;
                kb.LastRefreshAttemptError = null;
                await this.kbInfoHelper.InsertOrMergeKBInfo(kb);
            }
            catch (Exception ex)
            {
                kb.LastRefreshAttemptError = ex.ToString();
                await this.kbInfoHelper.InsertOrMergeKBInfo(kb);
            }
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
            if (string.IsNullOrWhiteSpace(kbId))
            {
                throw new ArgumentException($"{nameof(kbId)} must not be null or whitespace");
            }

            QnAMakerService qnAMakerService = new QnAMakerService(this.httpClient, this.qnaMakerSubcriptionKey);
            bool deleteSourcesResult = await this.DeleteExistingSources(qnAMakerService, kbId);
            bool addSourcesResult = true;

            // Less than 10 files
            if (blobInfo.Count < 10)
            {
                addSourcesResult = await this.AddNewSources(kbId, blobInfo, questionField, qnAMakerService);
            }

            // More than 10 files
            else
            {
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
                    addSourcesResult = addSourcesResult && await this.AddNewSources(kbId, blobInfoBatch, questionField, qnAMakerService);
                }
            }

            // if delete or any of the updates fails, KB is not published. Retry on next refresh.
            if (addSourcesResult && deleteSourcesResult)
            {
                await qnAMakerService.PublishKB(kbId);
            }
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
