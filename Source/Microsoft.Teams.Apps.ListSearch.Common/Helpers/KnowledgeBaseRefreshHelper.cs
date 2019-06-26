// <copyright file="KnowledgeBaseRefreshHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        private readonly BlobHelper blobHelper;
        private readonly KBInfoHelper kbInfoHelper;
        private readonly GraphHelper graphHelper;
        private readonly QnAMakerService qnaMakerService;
        private readonly ILogProvider logProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeBaseRefreshHelper"/> class.
        /// </summary>
        /// <param name="blobHelper">Blob helper</param>
        /// <param name="kbInfoHelper">KB info helper</param>
        /// <param name="graphHelper">Graph helper</param>
        /// <param name="qnaMakerService">QnAMaker service</param>
        /// <param name="logProvider">Log provider to use</param>
        public KnowledgeBaseRefreshHelper(BlobHelper blobHelper, KBInfoHelper kbInfoHelper, GraphHelper graphHelper, QnAMakerService qnaMakerService, ILogProvider logProvider)
        {
            this.blobHelper = blobHelper;
            this.kbInfoHelper = kbInfoHelper;
            this.graphHelper = graphHelper;
            this.qnaMakerService = qnaMakerService;
            this.logProvider = logProvider;
        }

        /// <summary>
        /// Refresh the data in the given knowledge base.
        /// </summary>
        /// <param name="kb">Knowledge base info</param>
        /// <param name="correlationId">Correlation id to use for logging</param>
        /// <returns>Tracking task</returns>
        public async Task RefreshKnowledgeBaseAsync(KBInfo kb, Guid? correlationId = null)
        {
            correlationId = correlationId ?? Guid.NewGuid();

            this.logProvider.LogInfo($"Starting refresh of knowledge base {kb.KBId}", correlationId: correlationId);
            this.logProvider.LogDebug($"Last successful refresh was on {kb.LastRefreshDateTime}", correlationId: correlationId);
            this.logProvider.LogDebug($"Last refresh attempt was on {kb.LastRefreshAttemptDateTime}, with status {kb.LastRefreshAttemptError ?? "success"}", correlationId: correlationId);

            Dictionary<string, Uri> blobInfoTemp = new Dictionary<string, Uri>();

            kb.LastRefreshAttemptDateTime = DateTime.UtcNow;

            try
            {
                // Update the KB with the current list contents
                ColumnInfo questionColumn = JsonConvert.DeserializeObject<ColumnInfo>(kb.QuestionField);
                await this.PopulateTemporaryBlobsWithListContentsAsync(kb, questionColumn, blobInfoTemp, correlationId.Value);
                await this.UpdateKnowledgeBaseAsync(kb.KBId, blobInfoTemp, JsonConvert.DeserializeObject<ColumnInfo>(kb.QuestionField).Name, correlationId.Value);

                this.logProvider.LogDebug($"Refresh of KB succeeded", correlationId: correlationId);

                // Record the successful update
                kb.LastRefreshDateTime = DateTime.UtcNow;
                kb.LastRefreshAttemptError = string.Empty;
                await this.kbInfoHelper.InsertOrMergeKBInfo(kb);
            }
            catch (Exception ex)
            {
                this.logProvider.LogError($"Refresh of KB failed: {ex.Message}", ex, correlationId: correlationId);

                // Log refresh attempt
                kb.LastRefreshAttemptError = ex.ToString();
                await this.kbInfoHelper.InsertOrMergeKBInfo(kb);
            }
            finally
            {
                try
                {
                    // Delete the temporary blobs that were created
                    await this.DeleteTemporaryBlobsAsync(blobInfoTemp, correlationId.Value);
                }
                catch (Exception ex)
                {
                    this.logProvider.LogError($"Failed to delete temporary blobs: {ex.Message}", ex, correlationId: correlationId);
                }
            }

            this.logProvider.LogInfo($"Finished refreshing KB {kb.KBId}, with status {kb.LastRefreshAttemptError ?? "success"}", correlationId: correlationId);
        }

        // Populate temporary blob files with the SharePoint list contents
        private async Task PopulateTemporaryBlobsWithListContentsAsync(KBInfo kb, ColumnInfo questionColumn, Dictionary<string, Uri> blobInfoTemp, Guid correlationId)
        {
            GetListContentsResponse listContents = null;

            do
            {
                listContents = await this.GetListContentsPageAsync(kb.SharePointListId, kb.AnswerFields, questionColumn.Name, kb.SharePointSiteId, listContents?.ODataNextLink ?? null);

                string blobName = $"{Guid.NewGuid()}.json";
                string blobUrl = await this.blobHelper.UploadBlobAsync(JsonConvert.SerializeObject(listContents), blobName);
                blobInfoTemp.Add(blobName, new Uri(blobUrl));

                this.logProvider.LogDebug($"Fetched page of list contents, stored as {blobName}", correlationId: correlationId);
            }
            while (!string.IsNullOrEmpty(listContents.ODataNextLink));
        }

        // Delete the temporary blobs that were created
        private async Task DeleteTemporaryBlobsAsync(Dictionary<string, Uri> blobInfoTemp, Guid correlationId)
        {
            foreach (string blobName in blobInfoTemp.Keys)
            {
                await this.blobHelper.DeleteBlobAsync(blobName);
                this.logProvider.LogDebug($"Deleted temporary blob {blobName}", correlationId: correlationId);
            }
        }

        // Update and publish the knowledge base
        private async Task UpdateKnowledgeBaseAsync(string kbId, Dictionary<string, Uri> blobInfo, string questionField, Guid correlationId)
        {
            this.logProvider.LogDebug($"Deleting existing KB sources", correlationId: correlationId);
            bool deleteSourcesResult = await this.DeleteExistingKnowledgeBaseSourcesAsync(kbId, correlationId);

            this.logProvider.LogDebug($"Adding new KB sources ({blobInfo.Count} files)", correlationId: correlationId);
            bool addSourcesResult = true;
            if (blobInfo.Count < 10)
            {
                // Fewer than 10 files
                addSourcesResult = await this.AddNewKnowledgeBaseSourcesAsync(kbId, blobInfo, questionField, correlationId);
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
                    this.logProvider.LogDebug($"Adding next batch of sources", correlationId: correlationId);
                    addSourcesResult = addSourcesResult && await this.AddNewKnowledgeBaseSourcesAsync(kbId, blobInfoBatch, questionField, correlationId);
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
                    this.logProvider.LogDebug($"Adding final batch of sources", correlationId: correlationId);
                    addSourcesResult = addSourcesResult && await this.AddNewKnowledgeBaseSourcesAsync(kbId, blobInfoBatch, questionField, correlationId);
                }
            }

            // if delete or any of the updates fails, KB is not published. Retry on next refresh.
            if (addSourcesResult && deleteSourcesResult)
            {
                this.logProvider.LogDebug($"Publishing updated knowledge base", correlationId: correlationId);
                await this.qnaMakerService.PublishKB(kbId);
            }

            this.logProvider.LogInfo($"Updated knowledge base {kbId}", correlationId: correlationId);
        }

        // Delete existing sources from the knowledge base
        private async Task<bool> DeleteExistingKnowledgeBaseSourcesAsync(string kbId, Guid correlationId)
        {
            GetKnowledgeBaseDetailsResponse kbDetails = await this.qnaMakerService.GetKnowledgeBaseDetails(kbId);
            UpdateKBRequest deleteSourcesRequest = new UpdateKBRequest()
            {
                Delete = new Delete()
                {
                    Sources = kbDetails.Sources,
                },
            };
            QnAMakerResponse deleteSourcesResult = await this.qnaMakerService.UpdateKB(kbId, deleteSourcesRequest);
            string deleteSourcesResultState = await this.qnaMakerService.AwaitOperationCompletionState(deleteSourcesResult);

            this.logProvider.LogDebug($"Delete operation completed with status {deleteSourcesResultState} ({kbDetails.Sources?.Count} sources)", correlationId: correlationId);

            return this.qnaMakerService.IsOperationSuccessful(deleteSourcesResultState);
        }

        // Add the specified sources to the knowledge base
        private async Task<bool> AddNewKnowledgeBaseSourcesAsync(string kbId, Dictionary<string, Uri> blobInfo, string questionField, Guid correlationId)
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

            var addSourcesResult = await this.qnaMakerService.UpdateKB(kbId, addSourcesRequest);
            string addSourcesResultState = await this.qnaMakerService.AwaitOperationCompletionState(addSourcesResult);

            this.logProvider.LogDebug($"Add operation completed with status {addSourcesResultState} ({files.Count} sources)", correlationId: correlationId);

            return this.qnaMakerService.IsOperationSuccessful(addSourcesResultState);
        }

        /// <summary>
        /// Get a single page of contents in the SharePoint list.
        /// </summary>
        /// <param name="listId">Id of the list to be fetched.</param>
        /// <param name="answerFields">Answer fields to be used for KB.</param>
        /// <param name="questionField">Question field.</param>
        /// <param name="sharePointSiteId">Site id of SharePoint site.</param>
        /// <param name="odataNextUrl">Link to the next set of items</param>
        /// <returns><see cref="Task"/> That resolves to <see cref="GetListContentsResponse"/> which represents the list response.</returns>
        private async Task<GetListContentsResponse> GetListContentsPageAsync(string listId, string answerFields, string questionField, string sharePointSiteId, string odataNextUrl)
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
