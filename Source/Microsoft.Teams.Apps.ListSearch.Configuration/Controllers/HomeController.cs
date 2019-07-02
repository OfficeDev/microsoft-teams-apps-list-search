// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Configuration.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.OpenIdConnect;
    using Microsoft.Teams.Apps.ListSearch.Common;
    using Microsoft.Teams.Apps.ListSearch.Common.Helpers;
    using Microsoft.Teams.Apps.ListSearch.Common.Models;
    using Microsoft.Teams.Apps.ListSearch.Configuration.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Home Controller
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly HttpClient httpClient;
        private readonly TokenHelper tokenHelper;
        private readonly KBInfoHelper kbInfoHelper;
        private readonly GraphHelper graphHelper;
        private readonly QnAMakerService qnaMakerService;
        private readonly KnowledgeBaseRefreshHelper knowledgeBaseRefreshHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="httpClient">Http client to be used.</param>
        /// <param name="tokenHelper">Token Helper.</param>
        /// <param name="kbInfoHelper">Knowledge base helper</param>
        /// <param name="graphHelper">Graph api helper</param>
        /// <param name="qnaMakerService">QnAMaker service</param>
        /// <param name="knowledgeBaseRefreshHelper"> Knowledge Base Refresh Helper </param>
        public HomeController(HttpClient httpClient, TokenHelper tokenHelper, KBInfoHelper kbInfoHelper, GraphHelper graphHelper, QnAMakerService qnaMakerService, KnowledgeBaseRefreshHelper knowledgeBaseRefreshHelper)
        {
            this.httpClient = httpClient;
            this.tokenHelper = tokenHelper;
            this.kbInfoHelper = kbInfoHelper;
            this.graphHelper = graphHelper;
            this.qnaMakerService = qnaMakerService;
            this.knowledgeBaseRefreshHelper = knowledgeBaseRefreshHelper;
        }

        /// <summary>
        /// GET: Home
        /// </summary>
        /// <returns>Task Action Result</returns>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var listKbInfo = await this.GetKbInfoAndSharepointUserAsync();
            return this.View(listKbInfo);
        }

        /// <summary>
        /// Login As SharePoint User
        /// </summary>
        [HttpGet]
        public void LoginAsSharePointUser()
        {
            this.HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" },
                    new string[] { OpenIdConnectAuthenticationDefaults.AuthenticationType, Constants.SharePointAppLoginAuthenticationType });
        }

        /// <summary>
        /// Configuration view
        /// </summary>
        /// <param name="id">Kb Id</param>
        /// <returns>Action Result</returns>
        [HttpGet]
        public async Task<ActionResult> ConfigureList(string id)
        {
            KBInfo kbInfo;
            if (string.IsNullOrEmpty(id))
            {
                kbInfo = new KBInfo();
            }
            else
            {
                kbInfo = await this.kbInfoHelper.GetKBInfo(id);
            }

            return this.View(kbInfo);
        }

        /// <summary>
        /// SharePoint list
        /// </summary>
        /// <param name="sharePointSiteUrl">SharePoint site url</param>
        /// <returns>SharePoint list result</returns>
        [HttpGet]
        public async Task<JsonResult> GetSharePointListColumns(string sharePointSiteUrl)
        {
            var response = await this.graphHelper.GetListInfoAsync(sharePointSiteUrl);

            return this.Json(JsonConvert.DeserializeObject<GetListContentsColumnsResponse>(response), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To save Knowledge base
        /// </summary>
        /// <param name="kbInfo"> Knowledge base</param>
        /// <returns>Action Result</returns>
        [HttpPost]
        public async Task<ActionResult> SaveKB(KBInfo kbInfo)
        {
            if (string.IsNullOrWhiteSpace(kbInfo.KBId))
            {
                string kbId = await this.CreateEmptyKB(kbInfo.KBName, this.qnaMakerService);
                if (!string.IsNullOrEmpty(kbId))
                {
                    kbInfo.RowKey = kbId;
                    kbInfo.PartitionKey = StorageInfo.KBInfoTablePartitionKey;
                    kbInfo.RankerType = RankerTypes.AutoSuggestQuestion;
                    kbInfo.LastRefreshDateTime = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    kbInfo.LastRefreshAttemptDateTime = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    await this.kbInfoHelper.InsertOrMergeKBInfo(kbInfo);
                }
                else
                {
                    return this.Redirect("/Home");
                }
            }

            if (!string.IsNullOrEmpty(kbInfo.KBId))
            {
                await this.knowledgeBaseRefreshHelper.RefreshKnowledgeBaseAsync(kbInfo);
            }

            return this.Redirect("/Home");
        }

        /// <summary>
        /// Action to delete KB
        /// </summary>
        /// <param name="id">Knowledge Base id</param>
        /// <returns>Action Result</returns>
        public async Task<ActionResult> DeleteKB(string id)
        {
            bool isDeleted = await this.qnaMakerService.DeleteKB(id);
            if (isDeleted)
            {
                await this.kbInfoHelper.DeleteKB(id);
            }

            return this.Redirect("/Home");
        }

        /// <summary>
        /// Get the Kb Info And User for home page
        /// </summary>
        /// <returns> Kb info</returns>
        private async Task<HomeViewModel> GetKbInfoAndSharepointUserAsync()
        {
            TokenEntity tokenEntity = await this.tokenHelper.GetTokenEntityAsync(TokenTypes.GraphTokenType);

            List<KBInfo> kbList = await this.kbInfoHelper.GetAllKBs(
               fields: new string[]
               {
                    nameof(KBInfo.KBName),
                    nameof(KBInfo.LastRefreshDateTime),
                    nameof(KBInfo.RefreshFrequencyInHours),
                    nameof(KBInfo.SharePointListId),
                    nameof(KBInfo.QuestionField),
                    nameof(KBInfo.AnswerFields),
                    nameof(KBInfo.SharePointSiteId),
                    nameof(KBInfo.SharePointUrl),
                    nameof(KBInfo.LastRefreshAttemptError),
               });

            return new HomeViewModel()
            {
                KBList = kbList,
                SharePointUserUpn = tokenEntity?.UserPrincipalName,
            };
        }

        /// <summary>
        /// Creates Knowledge base
        /// </summary>
        /// <param name="kbName">Knowledge base name</param>
        /// <param name="qnAMakerService">QnAMaker service</param>
        /// <returns>Returns kbId</returns>
        private async Task<string> CreateEmptyKB(string kbName, QnAMakerService qnAMakerService)
        {
            string kbId = string.Empty;
            CreateKBRequest createKBRequest = new CreateKBRequest()
            {
                Name = kbName,
            };

            var qnaMakerResponse = await qnAMakerService.CreateKB(createKBRequest);
            var operationResponse = await qnAMakerService.AwaitOperationCompletionResponse(qnaMakerResponse);

            if (qnAMakerService.IsOperationSuccessful(operationResponse.OperationState))
            {
                kbId = operationResponse.ResourceLocation.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            return kbId;
        }
    }
}