// <copyright file="SearchController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Xml;
    using Lib.Helpers;
    using Lib.Models;
    using ListSearch.Filters;
    using ListSearch.Helpers;
    using ListSearch.Models;
    using ListSearch.Resources;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Search controller that handles search flow.
    /// </summary>
    public class SearchController : Controller
    {
        private readonly System.Net.Http.HttpClient httpClient;
        private readonly int topResultsToBeFetched = 5;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchController"/> class.
        /// </summary>
        /// <param name="httpClient">Http client to be used.</param>
        public SearchController(System.Net.Http.HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Search View
        /// </summary>
        /// <param name="token">jwt auth token.</param>
        /// <returns><see cref="ActionResult"/> representing Search view.</returns>
        [HandleError]
        [JWTExceptionFilter]
        public async Task<ActionResult> Search(string token)
        {
            JWTHelper.ValidateJWT(token);

            this.ViewData["token"] = token;
            KBInfoHelper kBInfoHelper = new KBInfoHelper();
            List<KBInfo> kbList = await kBInfoHelper.GetAllKBs(new string[] { nameof(KBInfo.KBName) });
            return this.View(kbList);
        }

        /// <summary>
        /// Search List View
        /// </summary>
        /// <param name="kbId">Id of the kb to be queried.</param>
        /// <param name="kbName">Name of the kb to be queried.</param>
        /// /// <param name="token">jwt auth token.</param>
        /// <returns><see cref="ActionResult"/> representing Search List view.</returns>
        [HandleError]
        [JWTExceptionFilter]
        public ActionResult SearchList(string kbId, string kbName, string token)
        {
            JWTHelper.ValidateJWT(token);

            this.ViewData["token"] = token;
            this.ViewData["searchedKb"] = kbId;
            this.Session["kbId"] = kbId;
            this.Session["kbName"] = kbName;
            return this.View();
        }

        /// <summary>
        /// Search Result View
        /// </summary>
        /// <param name="searchedKeyword">Keyword searched by the user.</param>
        /// <param name="searchedKb">kb from which the keyword is to be searched.</param>
        /// <returns>Task that resolves to <see cref="ActionResult"/> representing Search Results view.</returns>
        [HandleError]
        [JWTExceptionFilter]
        public async Task<PartialViewResult> SearchResults(string searchedKeyword, string searchedKb)
        {
            var token = string.Empty;
            var authHeader = this.Request.Headers["Authorization"];
            if (authHeader?.StartsWith("bearer") ?? false)
            {
                token = authHeader.Split(' ')[1];
            }

            JWTHelper.ValidateJWT(token);

            this.ViewData["token"] = token;
            this.ViewData["searchKeyword"] = searchedKeyword;

            string kbId = this.Session["kbId"]?.ToString() ?? Strings.KBIdNotFound;

            KBInfoHelper kBInfoHelper = new KBInfoHelper();
            KBInfo kbInfo = await kBInfoHelper.GetKBInfo(kbId);

            string hostUrl = ConfigurationManager.AppSettings["HostUrl"];
            string endpointKey = ConfigurationManager.AppSettings["EndpointKey"];
            QnAMakerService qnAMakerHelper = new QnAMakerService(hostUrl, kbId, endpointKey, this.httpClient);

            int top = this.topResultsToBeFetched;
            GenerateAnswerRequest generateAnswerRequest = new GenerateAnswerRequest(searchedKeyword, top);
            GenerateAnswerResponse result = await qnAMakerHelper.GenerateAnswerAsync(generateAnswerRequest);

            return this.PartialView(result);
        }

        /// <summary>
        /// Result Card View
        /// </summary>
        /// /// <param name="token">jwt auth token.</param>
        /// <returns>Task that resolves to <see cref="ActionResult"/> representing Result Card view.</returns>
        [HandleError]
        [JWTExceptionFilter]
        public async Task<ActionResult> ResultCard(string token)
        {
            JWTHelper.ValidateJWT(token);

            string selectedAnswer = Convert.ToString(this.Session["selectdAnswer"]);
            string selectedQuestion = Convert.ToString(this.Session["selectedQuestion"]);
            KBInfoHelper kBInfoHelper = new KBInfoHelper();

            string kbName = this.Session["kbName"]?.ToString() ?? Strings.KBNameNotFound;
            string kbId = this.Session["kbId"]?.ToString() ?? Strings.KBIdNotFound;

            List<string> answerFields = await kBInfoHelper.GetAnswerFields(kbId);
            JObject answerObj = JsonConvert.DeserializeObject<JObject>(selectedAnswer);

            List<DeserializedAnswer> answers = this.DeserializeAnswers(JObject.Parse(answerObj.ToString()), answerFields);
            SelectedSearchResult selectedSearchResult = new SelectedSearchResult()
            {
                KBName = kbName,
                KBId = kbId,
                Question = selectedQuestion,
                Answers = answers,
            };
            return this.View(selectedSearchResult);
        }

        /// <summary>
        /// Error view
        /// </summary>
        /// <returns><see cref="ActionResult"/> for Error view.</returns>
        public ActionResult Error()
        {
            return this.View();
        }

        /// <summary>
        /// Error view for expired tokens.
        /// </summary>
        /// <returns><see cref="ActionResult"/> for Token expired error view.</returns>
        public ActionResult TokenExpiredError()
        {
            return this.View();
        }

        /// <summary>
        /// Error view for unauthorizedAccess
        /// </summary>
        /// <returns><see cref="ActionResult"/> for Unauthorized access error view.</returns>
        public ActionResult UnauthorizedAccess()
        {
            return this.View();
        }

        /// <summary>
        /// Sets selected answer and question
        /// </summary>
        /// <param name="answer">answer string</param>
        /// <param name="question">question string</param>
        /// <param name="token">jwt auth token.</param>
        /// <returns><see cref="JsonResult"/> denoting success</returns>
        [HttpPut]
        public JsonResult SetClickedItem(string answer, string question, string token)
        {
            ValidateToken(token, out _);

            this.Session["selectdAnswer"] = answer;
            this.Session["selectedQuestion"] = question;
            return this.Json("success");
        }

        /// <summary>
        /// Validates JWT
        /// </summary>
        /// <param name="token">JWT to be validated</param>
        /// <param name="tokenExpired">boolean value to check token has expired.</param>
        private static void ValidateToken(string token, out bool tokenExpired)
        {
            try
            {
                tokenExpired = false;
                JWTHelper.ValidateJWT(token);
            }
            catch (Exception ex)
            {
                // TODO: log ex
                if (ex.Message.Contains(JWTExceptions.LifetimeValidationFailedExceptionCode))
                {
                    tokenExpired = true;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Deserializes Answers to <see cref="OrderedDictionary"/>
        /// </summary>
        /// <param name="qnaAnswer">qna Answer</param>
        /// <param name="answerFields">answer fields</param>
        /// <returns><see cref="System.Collections.Generic.List{String}"/> objects representing questions and answers.</returns>
        private List<DeserializedAnswer> DeserializeAnswers(JObject qnaAnswer, List<string> answerFields)
        {
            List<DeserializedAnswer> deserializedAnswers = new List<DeserializedAnswer>();

            foreach (string field in answerFields)
            {
                deserializedAnswers.Add(new DeserializedAnswer()
                {
                    Question = XmlConvert.DecodeName(field),
                    Answer = Convert.ToString(qnaAnswer[field]),
                });
            }

            return deserializedAnswers;
        }
    }
}