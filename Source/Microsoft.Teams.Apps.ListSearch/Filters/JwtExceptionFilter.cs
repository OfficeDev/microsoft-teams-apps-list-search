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
    /// Custom exception handler for JWT validation errors
    /// </summary>
    public class JwtExceptionFilter : FilterAttribute, IExceptionFilter
    {
        private const string ErrorController = "Error";
        private const string LifetimeValidationFailedExceptionCode = "IDX10223";
        private static readonly string[] ExpectedJWTExceptionSources = new string[] { "System.IdentityModel.Tokens.Jwt", "Microsoft.IdentityModel.Tokens" };

        /// <summary>
        /// Gets or sets a value indicating whether the filter should redirect to a partial view on error
        /// </summary>
        public bool IsPartialView
        {
            get; set;
        }

        /// <inheritdoc/>
        public void OnException(ExceptionContext filterContext)
        {
            var logProvider = DependencyResolver.Current.GetService<ILogProvider>();

            string errorCode = string.Empty;

            var ex = filterContext.Exception;
            if (ex.GetType() == typeof(SecurityTokenException) || ExpectedJWTExceptionSources.Contains(ex.Source))
            {
                if (ex.Message.Contains(LifetimeValidationFailedExceptionCode))
                {
                    logProvider.LogWarning("Access denied: Expired JWT", exception: ex);
                    errorCode = "SessionExpired";
                }
                else
                {
                    logProvider.LogWarning("Access denied: Invalid JWT", exception: ex);
                    errorCode = "Unauthorized";
                }
            }
            else
            {
                logProvider.LogError("Error while processing request", exception: ex);
            }

            filterContext.Result = new RedirectResult($"/{ErrorController}?code={errorCode}&isPartialView={this.IsPartialView}");
            filterContext.ExceptionHandled = true;
        }
    }
}