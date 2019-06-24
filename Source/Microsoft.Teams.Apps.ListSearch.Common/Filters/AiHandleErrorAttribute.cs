// <copyright file="AiHandleErrorAttribute.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Filters
{
    using System;
    using System.Web.Mvc;
    using Microsoft.Teams.Apps.Common.Extensions;
    using Microsoft.Teams.Apps.Common.Logging;

    /// <summary>
    /// Application Insights error filter
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AiHandleErrorAttribute : HandleErrorAttribute
    {
        /// <inheritdoc/>
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext != null && filterContext.HttpContext != null && filterContext.Exception != null)
            {
                // If customError is Off, then AI HTTPModule will report the exception
                if (filterContext.HttpContext.IsCustomErrorEnabled)
                {
                    var logProvider = DependencyResolver.Current.GetService<ILogProvider>();
                    logProvider.LogWarning("Unhandled exception", exception: filterContext.Exception);
                }
            }

            base.OnException(filterContext);
        }
    }
}