// <copyright file="JwtExceptionFilter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Filters
{
    using System.Linq;
    using System.Web.Mvc;
    using ListSearch.Common.Models;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Custom exception handler.
    /// </summary>
    public class JwtExceptionFilter : FilterAttribute, IExceptionFilter
    {
        private const string ErrorController = "Error";
        private const string GenericErrorView = "Error";
        private const string UnauthorizedAccessView = "UnauthorizedAccess";
        private const string TokenExpiredView = "TokenExpiredError";

        /// <inheritdoc/>
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception.GetType() == typeof(SecurityTokenException) || JWTExceptions.ExpectedJWTExceptionSources.Contains(filterContext.Exception.Source))
            {
                if (filterContext.Exception.Message.Contains(JWTExceptions.LifetimeValidationFailedExceptionCode))
                {
                    filterContext.Result = new RedirectResult($"/{ErrorController}/{TokenExpiredView}");
                    filterContext.ExceptionHandled = true;
                }
                else
                {
                    filterContext.Result = new RedirectResult($"/{ErrorController}/{UnauthorizedAccessView}");
                    filterContext.ExceptionHandled = true;
                }
            }
            else
            {
                filterContext.Result = new RedirectResult($"/{ErrorController}/{GenericErrorView}");
                filterContext.ExceptionHandled = true;
            }
        }
    }
}