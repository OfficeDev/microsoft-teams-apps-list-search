// <copyright file="JwtExceptionFilter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Filters
{
    using System.Linq;
    using System.Web.Mvc;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.Teams.Apps.Common.Extensions;
    using Microsoft.Teams.Apps.Common.Logging;
    using Microsoft.Teams.Apps.ListSearch.Common.Models;

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
            var logProvider = DependencyResolver.Current.GetService<ILogProvider>();

            var ex = filterContext.Exception;
            if (ex.GetType() == typeof(SecurityTokenException) || JWTExceptions.ExpectedJWTExceptionSources.Contains(ex.Source))
            {
                if (ex.Message.Contains(JWTExceptions.LifetimeValidationFailedExceptionCode))
                {
                    logProvider.LogWarning("Access denied: Expired JWT", exception: ex);
                    filterContext.Result = new RedirectResult($"/{ErrorController}/{TokenExpiredView}");
                    filterContext.ExceptionHandled = true;
                }
                else
                {
                    logProvider.LogWarning("Access denied: Invalid JWT", exception: ex);
                    filterContext.Result = new RedirectResult($"/{ErrorController}/{UnauthorizedAccessView}");
                    filterContext.ExceptionHandled = true;
                }
            }
            else
            {
                logProvider.LogError("Error while processing request", exception: ex);
                filterContext.Result = new RedirectResult($"/{ErrorController}/{GenericErrorView}");
                filterContext.ExceptionHandled = true;
            }
        }
    }
}