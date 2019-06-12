// <copyright file="SearchController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Xml;
    using Microsoft.Teams.Apps.Common.Logging;
    using Microsoft.Teams.Apps.ListSearch.Common.Helpers;
    using Microsoft.Teams.Apps.ListSearch.Common.Models;
    using Microsoft.Teams.Apps.ListSearch.Filters;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Search controller that handles search flow.
    /// </summary>
    public class SearchController : Controller
    {
        private readonly System.Net.Http.HttpClient httpClient;
        private readonly JwtHelper jwtHelper;
        private readonly int topResultsToBeFetched = 5;
        private readonly string tenantId;
        private readonly string connectionString;
        private readonly ILogProvider logProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchController"/> class.
        /// </summary>
        /// <param name="httpClient">Http client to be used.</param>
        /// <param name="jwtHelper">JWT Helper.</param>
        /// <param name="logProvider">Log provider to use</param>
        public SearchController(System.Net.Http.HttpClient httpClient, JwtHelper jwtHelper, ILogProvider logProvider)
        {
            this.httpClient = httpClient;
            this.jwtHelper = jwtHelper;
            this.tenantId = ConfigurationManager.AppSettings["TenantId"];
            this.connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            this.logProvider = logProvider;
        }

        /// <summary>
        /// Search View
        /// </summary>
        /// <param name="token">jwt auth token.</param>
        /// <returns><see cref="ActionResult"/> representing Search view.</returns>
        [HandleError]
        [JwtExceptionFilter]
        public async Task<ActionResult> Search(string token)
        {
            this.jwtHelper.ValidateJWT(token, this.tenantId);

            this.ViewData["token"] = token;

            KBInfoHelper kBInfoHelper = new KBInfoHelper(this.connectionString);
            List<KBInfo> kbList = await kBInfoHelper.GetAllKBs(new string[] { nameof(KBInfo.KBName), nameof(KBInfo.KBId), nameof(KBInfo.QuestionField), nameof(KBInfo.AnswerFields) });

            return this.View(kbList);
        }

        /// <summary>
        /// Search Result View
        /// </summary>
        /// <param name="searchedKeyword">Keyword searched by the user.</param>
        /// <param name="kbId">kb Id.</param>
        /// <returns>Task that resolves to <see cref="PartialViewResult"/> representing Search Results partial view.</returns>
        [HandleError]
        [JwtExceptionFilter]
        public async Task<PartialViewResult> SearchResults(string searchedKeyword, string kbId)
        {
            this.ValidateAuthorizationHeader();

            this.ViewData["searchKeyword"] = searchedKeyword;

            KBInfoHelper kbInfoHelper = new KBInfoHelper(this.connectionString);
            KBInfo kbInfo = await kbInfoHelper.GetKBInfo(kbId);

            var subscriptionKey = ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"];
            var hostUrl = ConfigurationManager.AppSettings["QnAMakerHostUrl"];
            QnAMakerService qnaMakerHelper = new QnAMakerService(this.httpClient, subscriptionKey, hostUrl);

            int top = this.topResultsToBeFetched;
            GenerateAnswerRequest generateAnswerRequest = new GenerateAnswerRequest(searchedKeyword, top);
            GenerateAnswerResponse result = await qnaMakerHelper.GenerateAnswerAsync(kbId, generateAnswerRequest);

            List<SelectedSearchResult> selectedSearchResults = new List<SelectedSearchResult>();

            this.Session["SharePointUrl"] = kbInfo.SharePointUrl;

            // To check if answers list has some values or not. If it has some values then proceed
            if (result != null)
            {
                // If answers value score is not equal to 0.0 and then no need to proceed
                if (result.Answers.Count > 0 && result.Answers[0].Score != 0.0)
                {
                    foreach (QnAAnswer item in result.Answers)
                    {
                        List<ColumnInfo> answerFields = JsonConvert.DeserializeObject<List<ColumnInfo>>(kbInfo.AnswerFields);
                        JObject answerObj = JsonConvert.DeserializeObject<JObject>(item.Answer);
                        List<DeserializedAnswer> answers = this.DeserializeAnswers(answerObj, answerFields);

                        selectedSearchResults.Add(new SelectedSearchResult()
                        {
                            KBId = kbId,
                            Question = item.Questions[0],
                            Answers = answers,
                            Id = answerObj["id"].ToString(),
                        });
                    }
                }
            }

            return this.PartialView(selectedSearchResults);
        }

        /// <summary>
        /// Result Card Partial View
        /// </summary>
        /// <param name="kbId">kd id</param>
        /// <returns><see cref="PartialViewResult"/> representing Result card partial view.</returns>
        [HandleError]
        [JwtExceptionFilter]
        public PartialViewResult ResultCardPartial(string kbId)
        {
            string selectedAnswer = Convert.ToString(this.Session["selectdAnswer"]);
            string selectedQuestion = Convert.ToString(this.Session["selectedQuestion"]);
            string selectedItemId = Convert.ToString(this.Session["selectedItemId"]);

            List<DeserializedAnswer> answers = JsonConvert.DeserializeObject<List<DeserializedAnswer>>(selectedAnswer);

            SelectedSearchResult selectedSearchResult = new SelectedSearchResult()
            {
                KBId = kbId,
                Question = selectedQuestion,
                Answers = answers,
                Id = selectedItemId,
                SharePointURL = this.Session["SharePointUrl"].ToString(),
            };

            return this.PartialView(selectedSearchResult);
        }

        /// <summary>
        /// Sets selected answer and question
        /// </summary>
        /// <param name="answer">answer string</param>
        /// <param name="question">question string</param>
        /// <param name="id">id of selected item</param>
        /// <returns><see cref="JsonResult"/> denoting success</returns>
        [HttpPost]
        [HandleError]
        [JwtExceptionFilter]
        public JsonResult SetClickedItem(string answer, string question, string id)
        {
            this.ValidateAuthorizationHeader();

            this.Session["selectdAnswer"] = answer;
            this.Session["selectedQuestion"] = question;
            this.Session["selectedItemId"] = id;
            return this.Json("success");
        }

        // Validate the incoming JWT
        private string ValidateAuthorizationHeader()
        {
            var token = string.Empty;
            var authHeader = this.Request.Headers["Authorization"];
            if (authHeader?.StartsWith("bearer") ?? false)
            {
                token = authHeader.Split(' ')[1];
            }

            this.jwtHelper.ValidateJWT(token, this.tenantId);

            return token;
        }

        /// <summary>
        /// Deserializes Answers to <see cref="OrderedDictionary"/>
        /// </summary>
        /// <param name="qnaAnswer">qna Answer</param>
        /// <param name="answerFields">answer fields</param>
        /// <returns><see cref="System.Collections.Generic.List{String}"/> objects representing questions and answers.</returns>
        private List<DeserializedAnswer> DeserializeAnswers(JObject qnaAnswer, List<ColumnInfo> answerFields)
        {
            List<DeserializedAnswer> deserializedAnswers = new List<DeserializedAnswer>();

            foreach (var field in answerFields)
            {
                deserializedAnswers.Add(new DeserializedAnswer()
                {
                    Question = XmlConvert.DecodeName(field.DisplayName),
                    Answer = Convert.ToString(qnaAnswer[field.Name]),
                });
            }

            return deserializedAnswers;
        }
    }
}