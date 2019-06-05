// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace ConfigApp.Controllers
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Lib;
    using Lib.Helpers;
    using Lib.Models;
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
        public HomeController(HttpClient httpClient, TokenHelper tokenHelper)
        {
            var connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            this.httpClient = httpClient;
            this.tokenHelper = tokenHelper;
            this.kbInfoHelper = new KBInfoHelper(connectionString);
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
                    new string[] { OpenIdConnectAuthenticationDefaults.AuthenticationType, Lib.Constants.SharePointAppLoginAuthenticationType });
        }

        /// <summary>
        /// Get the Kb Info And User for home page
        /// </summary>
        /// <returns> kb info</returns>
        private async Task<List<KBInfo>> GetKbInfoAndSharepointUserAsync()
        {
            string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

            TokenEntity tokenEntity = await this.tokenHelper.GetTokenEntity(TokenTypes.GraphTokenType);
            if (tokenEntity != null)
            {
                this.ViewBag.Email = tokenEntity.UserPrincipalName;
                this.ViewBag.IsSharepointUserConfigured = true;
            }
            else
            {
                this.ViewBag.IsSharepointUserConfigured = false;
            }

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
            return kbList;
        }
    }
}