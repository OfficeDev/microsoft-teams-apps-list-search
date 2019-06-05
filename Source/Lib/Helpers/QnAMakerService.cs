// <copyright file="QnAMakerService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Lib.Helpers
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Lib.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Helper for accessing QnA Maker APIs
    /// </summary>
    public class QnAMakerService : IQnAMakerService
    {
        /// <summary>
        /// QnA Maker Request url
        /// </summary>
        private const string QnAMakerRequestUrl = "https://westus.api.cognitive.microsoft.com/qnamaker/v4.0";

        private const string MethodKB = "knowledgebases";
        private const string MethodOperation = "operations";

        /// <summary>
        /// Host url of the compute application
        /// </summary>
        private readonly string hostUrl;

        /// <summary>
        /// Id of KB to be queried.
        /// </summary>
        private readonly string kbId;

        /// <summary>
        /// Ocp-Apim-Subscription-Key for the QnA Maker service
        /// </summary>
        private readonly string subscriptionKey;

        /// <summary>
        /// Http client for generating http requests.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// Endpoint key for the published Kb to be searched.
        /// </summary>
        private string endpointKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerService"/> class.
        /// This constructor initializes an instance meant for GenerateAnswerAsync method.
        /// </summary>
        /// <param name="httpClient">HttpClient for generating http requests</param>
        public QnAMakerService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerService"/> class.
        /// This constructor initializes an instance meant for Update, Publish and GetOperation APIs.
        /// </summary>
        /// <param name="kbId">Id of the KB to be queried</param>
        /// <param name="subscriptionKey">Ocp-Apim-Subscription-Key for the QnA Maker service</param>
        /// <param name="httpClient">Http Client to be used.</param>
        public QnAMakerService(string kbId, string subscriptionKey, HttpClient httpClient)
        {
            this.kbId = kbId;
            this.subscriptionKey = subscriptionKey;
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerService"/> class.
        /// This constructor initializes an instance meant for Create API.
        /// </summary>
        /// <param name="subscriptionKey">Ocp-Apim-Subscription-Key for the QnA Maker service</param>
        /// <param name="httpClient">Http Client to be used.</param>
        public QnAMakerService(string subscriptionKey, HttpClient httpClient)
        {
            this.subscriptionKey = subscriptionKey;
            this.httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<GenerateAnswerResponse> GenerateAnswerAsync(GenerateAnswerRequest request, string kbId, string hostUrl)
        {
            string uri = $"{hostUrl}/qnamaker/{MethodKB}/{kbId}/generateAnswer";
            await this.FetchQnAMakerEndpointKey();
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                httpRequest.Headers.Add("Authorization", "EndpointKey " + this.endpointKey);

                var response = await this.httpClient.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<GenerateAnswerResponse>(await response.Content.ReadAsStringAsync());
            }
        }

        /// <inheritdoc/>
        public async Task<QnAMakerResponse> UpdateKB(UpdateKBRequest body)
        {
            string uri = $"{QnAMakerRequestUrl}/{MethodKB}/{this.kbId}";
            using (var httpRequest = new HttpRequestMessage(new HttpMethod("PATCH"), uri))
            {
                httpRequest.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                httpRequest.Headers.Add(Constants.OcpApimSubscriptionKey, this.subscriptionKey);

                var response = await this.httpClient.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<QnAMakerResponse>(await response.Content.ReadAsStringAsync());
            }
        }

        /// <inheritdoc/>
        public async Task<bool> PublishKB(string kbId = null)
        {
            var uri = $"{QnAMakerRequestUrl}/{MethodKB}/{kbId ?? this.kbId}";
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                httpRequest.Headers.Add(Constants.OcpApimSubscriptionKey, this.subscriptionKey);

                var response = await this.httpClient.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                return true;
            }
        }

        /// <inheritdoc/>
        public async Task<QnAMakerResponse> CreateKB(CreateKBRequest body)
        {
            var uri = $"{QnAMakerRequestUrl}/{MethodKB}/create";
            using (HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                httpRequest.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                httpRequest.Headers.Add(Constants.OcpApimSubscriptionKey, this.subscriptionKey);

                var response = await this.httpClient.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<QnAMakerResponse>(await response.Content.ReadAsStringAsync());
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteKB(string kbId)
        {
            var uri = $"{QnAMakerRequestUrl}/{MethodKB}/{kbId}";
            using (HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Delete, uri))
            {
                httpRequest.Headers.Add(Constants.OcpApimSubscriptionKey, this.subscriptionKey);

                var response = await this.httpClient.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                return true;
            }
        }

        /// <inheritdoc/>
        public async Task<QnAMakerResponse> GetOperationDetails(string operationId)
        {
            var uri = $"{QnAMakerRequestUrl}/{MethodOperation}/{operationId}";
            using (HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                httpRequest.Headers.Add(Constants.OcpApimSubscriptionKey, this.subscriptionKey);

                var response = await this.httpClient.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<QnAMakerResponse>(await response.Content.ReadAsStringAsync());
            }
        }

        /// <inheritdoc/>
        public async Task<GetKnowledgeBaseDetailsResponse> GetKnowledgeBaseDetails()
        {
            var uri = $"{QnAMakerRequestUrl}/{MethodKB}/{this.kbId}";
            using (HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                httpRequest.Headers.Add(Constants.OcpApimSubscriptionKey, this.subscriptionKey);

                var response = await this.httpClient.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<GetKnowledgeBaseDetailsResponse>(await response.Content.ReadAsStringAsync());
            }
        }

        /// <inheritdoc/>
        public async Task<string> AwaitOperationCompletionState(QnAMakerResponse response)
        {
            int delay = 1000; // ms
            QnAMakerResponse getOperationDetailsResponse = response;
            while (!this.IsOperationComplete(getOperationDetailsResponse))
            {
                await Task.Delay(delay);
                getOperationDetailsResponse = await this.GetOperationDetails(response.OperationId);
            }

            return getOperationDetailsResponse.OperationState;
        }

        /// <inheritdoc/>
        public async Task<QnAMakerResponse> AwaitOperationCompletionResponse(QnAMakerResponse response)
        {
            int delay = 1000; // ms
            QnAMakerResponse getOperationDetailsResponse = response;
            while (!this.IsOperationComplete(getOperationDetailsResponse))
            {
                await Task.Delay(delay);
                getOperationDetailsResponse = await this.GetOperationDetails(response.OperationId);
            }

            return getOperationDetailsResponse;
        }

        /// <inheritdoc/>
        public bool IsOperationSuccessful(string operationState)
        {
            if (operationState == QnAMakerOperationStates.Succeeded)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if operation is completed.
        /// </summary>
        /// <param name="response">Response to be checked if completed.</param>
        /// <returns><see cref="bool"/> that represents if operation is complete.</returns>
        private bool IsOperationComplete(QnAMakerResponse response)
        {
            if (response?.OperationState == QnAMakerOperationStates.Succeeded)
            {
                return true;
            }
            else if (response?.OperationState == QnAMakerOperationStates.Running || response?.OperationState == QnAMakerOperationStates.NotStarted)
            {
                return false;
            }
            else
            {
                StringBuilder details = new StringBuilder();
                foreach (var detail in response.ErrorResponse.Error.Details)
                {
                    details.AppendLine(detail.Message);
                }

                throw new Exception($"Error Code: {response.ErrorResponse.Error.Code}\nError Message: {response.ErrorResponse.Error.Message}\nError Details: {details.ToString()}");
            }
        }

        /// <summary>
        /// Description : Get and return the QnAMaker end point key
        /// </summary>
        /// <returns> representing the asynchronous operation.</returns>
        private async Task FetchQnAMakerEndpointKey()
        {
            if (string.IsNullOrEmpty(this.endpointKey))
            {
                string endpointKeyUrl = $"{QnAMakerRequestUrl}/endpointkeys";

                using (HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, endpointKeyUrl))
                {
                    httpRequest.Headers.Add(Constants.OcpApimSubscriptionKey, this.subscriptionKey);

                    var response = await this.httpClient.SendAsync(httpRequest);
                    response.EnsureSuccessStatusCode();
                    QnAMakerEndpointResponse qnaMakerEndpointResponse = JsonConvert.DeserializeObject<QnAMakerEndpointResponse>(await response.Content.ReadAsStringAsync());
                    this.endpointKey = qnaMakerEndpointResponse.PrimaryEndpointKey;
                }
            }
        }
    }
}
