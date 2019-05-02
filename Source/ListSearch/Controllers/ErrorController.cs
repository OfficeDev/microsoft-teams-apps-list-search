// <copyright file="ErrorController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Controllers
{
    using System.Web.Mvc;

    /// <summary>
    /// Error Controller
    /// </summary>
    public class ErrorController : Controller
    {
        /// <summary>
        /// Error view
        /// </summary>
        /// <returns>Task that resolves to <see cref="ActionResult"/> representing Error view.</returns>
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
    }
}