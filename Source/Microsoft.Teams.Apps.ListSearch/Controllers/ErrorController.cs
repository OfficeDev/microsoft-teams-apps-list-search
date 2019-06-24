// <copyright file="ErrorController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Controllers
{
    using System.Web.Mvc;
    using Microsoft.Teams.Apps.ListSearch.Resources;

    /// <summary>
    /// Error Controller
    /// </summary>
    public class ErrorController : Controller
    {
        /// <summary>
        /// Error view
        /// </summary>
        /// <param name="code">The error to show</param>
        /// <param name="isPartialView">True if this should be partial view</param>
        /// <returns>Task that resolves to <see cref="ActionResult"/> representing Error view.</returns>
        public ActionResult Index(string code = null, bool isPartialView = false)
        {
            switch (code)
            {
                case "Unauthorized":
                    this.ViewBag.Title = Strings.ErrorUnauthorizedTitle;
                    this.ViewBag.Message = Strings.ErrorUnauthorizedMessage;
                    break;

                case "SessionExpired":
                    this.ViewBag.Title = Strings.ErrorSessionExpiredTitle;
                    this.ViewBag.Message = Strings.ErrorSessionExpiredMessage;
                    break;

                default:
                    this.ViewBag.Title = Strings.ErrorGenericTitle;
                    this.ViewBag.Message = Strings.ErrorGenericMessage;
                    break;
            }

            return isPartialView ? (ActionResult)this.PartialView("ErrorPartial") : this.View();
        }
    }
}