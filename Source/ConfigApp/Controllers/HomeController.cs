// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace ConfigApp.Controllers
{
    using System.Net.Http;
    using System.Web.Mvc;
    using Lib.Helpers;

    /// <summary>
    /// Home Controller
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly HttpClient httpClient;
        private TokenHelper tokenHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="httpClient">Http client to be used.</param>
        /// <param name="tokenHelper">Token Helper.</param>
        public HomeController(HttpClient httpClient, TokenHelper tokenHelper)
        {
            this.httpClient = httpClient;
            this.tokenHelper = tokenHelper;
        }

        /// <summary>
        /// GET: Home
        /// </summary>
        /// <returns>Task Action Result</returns>
        public ActionResult Index()
        {
            return this.View();
        }
    }
}