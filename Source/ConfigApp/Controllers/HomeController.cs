// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace ConfigApp.Controllers
{
    using System.Web.Mvc;

    /// <summary>
    /// Home Controller
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
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