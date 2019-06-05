// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Configuration.Controllers
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using ListSearch.Common;
    using ListSearch.Common.Helpers;
    using ListSearch.Common.Models;
    using ListSearch.Configuration.Models;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.OpenIdConnect;

    /// <summary>
    /// Home Controller
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly HttpClient httpClient;
        private TokenHelper tokenHelper;
        private KBInfoHelper kbInfoHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="httpClient">Http client to be used.</param>
        /// <param name="tokenHelper">Token Helper.</param>
        /// <param name="kbInfoHelper">Knowledge base helper</param>
        public HomeController(HttpClient httpClient, TokenHelper tokenHelper, KBInfoHelper kbInfoHelper)
        {
            this.httpClient = httpClient;
            this.tokenHelper = tokenHelper;
            this.kbInfoHelper = kbInfoHelper;
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
        /// Login As Share point User
        /// </summary>
        public void LoginAsSharepointUser()
        {
            this.HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" },
                    new string[] { OpenIdConnectAuthenticationDefaults.AuthenticationType, Constants.SharePointAppLoginAuthenticationType });
        }

        /// <summary>
        /// Get the Kb Info And User for home page
        /// </summary>
        /// <returns> kb info</returns>
        private async Task<HomeViewModel> GetKbInfoAndSharepointUserAsync()
        {
            TokenEntity tokenEntity = await this.tokenHelper.GetTokenEntityAsync(TokenTypes.GraphTokenType);

            List<KBInfo> kbList = await this.kbInfoHelper.GetAllKBs(
               fields: new string[]
               {
                    nameof(KBInfo.KBId),
                    nameof(KBInfo.KBName),
                    nameof(KBInfo.LastRefreshDateTime),
                    nameof(KBInfo.RefreshFrequencyInHours),
                    nameof(KBInfo.SharePointListId),
                    nameof(KBInfo.QuestionField),
                    nameof(KBInfo.AnswerFields),
                    nameof(KBInfo.SharePointSiteId),
                    nameof(KBInfo.LastRefreshAttemptError)
               });

            return new HomeViewModel()
            {
                KBList = kbList,
                SharePointUserUpn = tokenEntity.UserPrincipalName,
            };
        }
    }
}