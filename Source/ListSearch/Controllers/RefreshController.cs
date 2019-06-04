// <copyright file="RefreshController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Lib.Helpers;
    using Lib.Models;
    using ListSearch.Filters;
    using ListSearch.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Controller to refresh the KB.
    /// </summary>
    public class RefreshController : ApiController
    {
        private const string JsonFileExtension = ".json";
        private readonly HttpClient httpClient;
        private readonly string subscriptionKey;
        private readonly string connectionString;
        private readonly BlobHelper blobHelper;
        private readonly KBInfoHelper kbInfoHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshController"/> class.
        /// </summary>
        /// <param name="httpClient">Http client to be used.</param>
        public RefreshController(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new System.ArgumentNullException(nameof(httpClient));
            this.subscriptionKey = ConfigurationManager.AppSettings["Ocp-Apim-Subscription-Key"];

            this.connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            this.blobHelper = new BlobHelper(this.connectionString);
            this.kbInfoHelper = new KBInfoHelper(this.connectionString);
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
        [HttpPost]
        [HttpPatch]
        [RefreshAuthFilter]
        public async Task RefreshAllKBs()
        {
            List<KBInfo> kbList = await this.kbInfoHelper.GetAllKBs(
                fields: new string[]
                {
                    nameof(KBInfo.LastRefreshDateTime),
                    nameof(KBInfo.RefreshFrequencyInHours),
                    nameof(KBInfo.SharePointListId),
                    nameof(KBInfo.QuestionField),
                    nameof(KBInfo.AnswerFields),
                    nameof(KBInfo.SharePointSiteId),
                });

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
                        await this.RefreshKnowledgeBaseAsync(kb);
                    }
                    catch
                    {
                        // TODO: log ex
                        continue;
                    }
                }
            }
        }

        // Refresh the data in the given knowledge base
        private async Task RefreshKnowledgeBaseAsync(KBInfo kb)
        {
            Dictionary<string, Uri> blobInfoTemp = new Dictionary<string, Uri>();
            GetListContentsResponse listContents = new GetListContentsResponse();

            do
            {
                ColumnInfo questionColumn = JsonConvert.DeserializeObject<ColumnInfo>(kb.QuestionField);
                string blobName = Guid.NewGuid().ToString() + JsonFileExtension;
                listContents = await this.GetListContents(kb.SharePointListId, kb.AnswerFields, questionColumn.Name, kb.SharePointSiteId, this.connectionString, listContents.ODataNextLink ?? null);
                string blobUrl = await this.blobHelper.UploadBlobAsync(JsonConvert.SerializeObject(listContents), blobName);
                blobInfoTemp.Add(blobName, new Uri(blobUrl));
            }
            while (!string.IsNullOrEmpty(listContents.ODataNextLink));

            await this.RefreshKB(kb.KBId, blobInfoTemp, JsonConvert.DeserializeObject<ColumnInfo>(kb.QuestionField).Name, this.blobHelper);

            // Delete all existing blobs for this KB
            foreach (string blobName in blobInfoTemp.Keys)
            {
                await this.blobHelper.DeleteBlobAsync(blobName);
            }

            kb.LastRefreshDateTime = DateTime.UtcNow;
            await this.kbInfoHelper.InsertOrMergeKBInfo(kb);
        }

        /// <summary>
        /// Refreshes KB - Updates and Publishes KB.
        /// </summary>
        /// <param name="kbId">Id of KB to be refreshed</param>
        /// <param name="blobInfo">Details of source blob files</param>
        /// <param name="questionField">question field</param>
        /// <param name="blobHelper">Blob helper object</param>
        /// <returns>Task that represents refresh operation.</returns>
        private async Task RefreshKB(string kbId, Dictionary<string, Uri> blobInfo, string questionField, BlobHelper blobHelper)
        {
            if (string.IsNullOrWhiteSpace(kbId))
            {
                throw new ArgumentException($"{nameof(kbId)} must not be null or whitespace");
            }

            QnAMakerService qnAMakerService = new QnAMakerService(kbId, this.subscriptionKey, this.httpClient);
            bool deleteSourcesResult = await this.DeleteExistingSources(qnAMakerService);
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
                await qnAMakerService.PublishKB();
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

        /// <summary>
        /// Get the contents of the list.
        /// </summary>
        /// <param name="listId">Id of the list to be fetched.</param>
        /// <param name="answerFields">Answer fields to be used for KB.</param>
        /// <param name="questionField">question field.</param>
        /// <param name="sharePointSiteId">site id of sharepoint site.</param>
        /// <param name="connectionString">connection string of storage.</param>
        /// <param name="odataNextUrl">odata next url</param>
        /// <returns><see cref="Task"/> that resolves to <see cref="GetListContentsResponse"/> which represents the list response.</returns>
        private async Task<GetListContentsResponse> GetListContents(string listId, string answerFields, string questionField, string sharePointSiteId, string connectionString, string odataNextUrl)
        {
            string tenantId = ConfigurationManager.AppSettings["TenantId"];
            string appId = ConfigurationManager.AppSettings["LoginAppClientId"];
            string appSecret = ConfigurationManager.AppSettings["LoginAppClientSecret"];

            TokenHelper tokenHelper = new TokenHelper(this.httpClient, connectionString, tenantId, appId, appSecret, tokenKey: appSecret);
            TokenEntity refreshTokenEntity = await tokenHelper.GetTokenEntity(TokenTypes.GraphTokenType);
            GraphHelper graphHelper = new GraphHelper(appId, appSecret);

            var fieldsToFetch = string.Join(
                ",",
                JsonConvert.DeserializeObject<List<ColumnInfo>>(answerFields)
                    .Select(field => field.Name)
                    .Concat(new string[] { questionField, "id" }));

            string responseBody = await graphHelper.GetListContents(
                httpClient: this.httpClient,
                refreshToken: refreshTokenEntity.RefreshToken,
                listId: listId,
                fieldsToFetch: fieldsToFetch,
                sharePointSiteId: sharePointSiteId,
                connectionString: connectionString,
                tenantId: tenantId,
                encryptionDecryptionKey: appSecret,
                odataNextUrl: odataNextUrl);
            return JsonConvert.DeserializeObject<GetListContentsResponse>(responseBody);
        }
    }
}
