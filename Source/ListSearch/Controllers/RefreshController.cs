// <copyright file="RefreshController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Lib.Helpers;
    using Lib.Models;
    using ListSearch.Filters;
    using ListSearch.Helpers;
    using ListSearch.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Controller to refresh the KB.
    /// </summary>
    public class RefreshController : ApiController
    {
        private const string JSONFileExtension = ".json";
        private readonly HttpClient httpClient;
        private readonly string subscriptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshController"/> class.
        /// </summary>
        /// <param name="httpClient">Http client to be used.</param>
        public RefreshController(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new System.ArgumentNullException(nameof(httpClient));
            this.subscriptionKey = ConfigurationManager.AppSettings["Ocp-Apim-Subscription-Key"];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshController"/> class.
        /// </summary>
        public RefreshController()
        {
        }

        /// <summary>
        /// Refreshes all KBs due for a refresh
        /// </summary>
        /// <returns><see cref="Task"/> to refresh KBs.</returns>
        [HttpPatch]
        [RefreshAuthFilter]
        public async Task RefreshAllKBs()
        {
            KBInfoHelper kBInfoHelper = new KBInfoHelper();
            List<KBInfo> kbList = await kBInfoHelper.GetAllKBs(
                fields: new string[]
                {
                    nameof(KBInfo.LastRefreshDateTime),
                    nameof(KBInfo.RefreshFrequencyInHours),
                    nameof(KBInfo.SharePointListId),
                    nameof(KBInfo.QuestionField),
                    nameof(KBInfo.AnswerFields),
                    nameof(KBInfo.SharePointSiteId),
                });

            BlobHelper blobHelper = new BlobHelper();

            foreach (var kb in kbList)
            {
                DateTime lastRefreshed = kb.LastRefreshDateTime;
                int frequencyInHours = kb.RefreshFrequencyInHours;
                if (lastRefreshed == DateTime.MinValue || frequencyInHours == 0)
                {
                    continue;
                }

                if (lastRefreshed.AddHours(frequencyInHours) < DateTime.UtcNow)
                {
                    try
                    {
                        Dictionary<string, Uri> blobInfoTemp = new Dictionary<string, Uri>();
                        GetListContentsResponse listContents = new GetListContentsResponse();

                        do
                        {
                            string blobName = Guid.NewGuid().ToString() + JSONFileExtension;
                            listContents = await this.GetListContents(kb.SharePointListId, kb.AnswerFields, kb.QuestionField, kb.SharePointSiteId, listContents.ODataNextLink ?? null);
                            string blobUrl = await blobHelper.UploadBlobAsync(JsonConvert.SerializeObject(listContents), blobName);
                            blobInfoTemp.Add(blobName, new Uri(blobUrl));
                        }
                        while (!string.IsNullOrEmpty(listContents.ODataNextLink));

                        await this.RefreshKB(kb.KBId, blobInfoTemp, kb.QuestionField, blobHelper);

                        // Delete all existing blobs for this KB
                        foreach (string blobName in blobInfoTemp.Keys)
                        {
                            await blobHelper.DeleteBlobAsync(blobName);
                        }

                        kb.LastRefreshDateTime = DateTime.UtcNow;
                        await kBInfoHelper.InsertOrMergeKBInfo(kb);
                    }
                    catch
                    {
                        // TODO: log ex
                        continue;
                    }
                }
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
        public async Task RefreshKB(string kbId, Dictionary<string, Uri> blobInfo, string questionField, BlobHelper blobHelper)
        {
            if (string.IsNullOrWhiteSpace(kbId))
            {
                throw new ArgumentException($"{nameof(kbId)} must not be null or whitespace");
            }

            int filesExtracted = 0;
            int counter = 0;
            bool deletesourcesResult = false;
            bool addSourcesResult = false;

            QnAMakerService qnAMakerService = new QnAMakerService(kbId, this.subscriptionKey, this.httpClient);

            deletesourcesResult = await this.DeleteExistingSources(qnAMakerService);

            // Less than 10 files
            if (blobInfo.Count < 10)
            {
                addSourcesResult = await this.AddNewSources(kbId, blobInfo, questionField, qnAMakerService);
            }

            // More than 10 files
            else
            {
                Dictionary<string, Uri> blobInfoBatch = new Dictionary<string, Uri>();
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
                    addSourcesResult = await this.AddNewSources(kbId, blobInfoBatch, questionField, qnAMakerService);

                    if (filesExtracted < blobInfo.Count)
                    {
                        counter = 1;
                        filesExtracted++;
                        blobInfoBatch.Clear();
                        blobInfoBatch.Add(entry.Key, entry.Value);
                    }
                }
            }

            // if delete or any of the updates fails, KB is not published. Retry on next refresh.
            if (addSourcesResult && deletesourcesResult)
            {
                await this.PublishKB(qnAMakerService);
            }
        }

        /// <summary>
        /// Deletes sources from KB
        /// </summary>
        /// <param name="qnAMakerService">instance qna maker service</param>
        /// <returns><see cref="Task"/> that resolves to a <see cref="bool"/> which represents success or failure of operation.</returns>
        private async Task<bool> DeleteExistingSources(QnAMakerService qnAMakerService)
        {
            GetKnowledgeBaseDetailsResponse kbDetails = await qnAMakerService.GetKnowledgeBaseDetails();

            UpdateKBRequest deleteSourcesRequest = new UpdateKBRequest()
            {
                Delete = new Delete()
                {
                    Sources = kbDetails.Sources
                }
            };
            QnAMakerResponse deleteSourcesResult = await qnAMakerService.UpdateKB(deleteSourcesRequest);
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
                                Question = questionField
                            }
                        }
                    });
            }

            UpdateKBRequest addSourcesRequest = new UpdateKBRequest()
            {
                Add = new Add()
                {
                    Files = files,
                }
            };

            QnAMakerResponse addSourcesResult = await qnAMakerService.UpdateKB(addSourcesRequest);
            string addSourcesResultState = await qnAMakerService.AwaitOperationCompletionState(addSourcesResult);
            return qnAMakerService.IsOperationSuccessful(addSourcesResultState);
        }

        private async Task<bool> PublishKB(QnAMakerService qnAMakerService)
        {
            QnAMakerResponse publishResponse = new QnAMakerResponse();

            string publishResultState = await qnAMakerService.AwaitOperationCompletionState(publishResponse);

            return qnAMakerService.IsOperationSuccessful(publishResultState);
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
            TokenHelper tokenHelper = new TokenHelper();
            TokenEntity refreshTokenEntity = await tokenHelper.GetTokenEntity(TokenTypes.GraphTokenType);
            GraphHelper graphHelper = new GraphHelper();

            System.Text.StringBuilder fieldsToFetch = new System.Text.StringBuilder();
            fieldsToFetch.Append(questionField + ",");
            foreach (string answerField in JsonConvert.DeserializeObject<List<string>>(answerFields))
            {
                fieldsToFetch.Append(answerField + ",");
            }

            fieldsToFetch.Remove(fieldsToFetch.Length - 1, 1);

            string responseBody = await graphHelper.GetListContents(this.httpClient, refreshTokenEntity.RefreshToken, listId, fieldsToFetch.ToString(), sharePointSiteId, odataNextUrl);
            return JsonConvert.DeserializeObject<GetListContentsResponse>(responseBody);
        }
    }
}
