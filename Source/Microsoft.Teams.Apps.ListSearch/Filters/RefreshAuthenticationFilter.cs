// <copyright file="RefreshAuthenticationFilter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Filters
{
    using System;
    using System.Configuration;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Filters;
    using Microsoft.Teams.Apps.ListSearch.Models;

    /// <summary>
    /// Authentication filter for the refresh endpoint
    /// </summary>
    public class RefreshAuthenticationFilter : Attribute, IAuthenticationFilter
    {
        /// <summary>
        /// Gets a value indicating whether multiple instances are allowed
        /// </summary>
        public bool AllowMultiple
        {
            get { return true; }
        }

        /// <summary>
        /// Authenticate the incoming request
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns><see cref="Task"/> representing authenticate method.</returns>
        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = context.Request;
            var authorization = request.Headers.Authorization;
            if (authorization == null)
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing Authorization header", request);
                return Task.CompletedTask;
            }
            else if (authorization.Scheme != "Bearer")
            {
                context.ErrorResult = new AuthenticationFailureResult($"Invalid authorization scheme {authorization.Scheme}", request);
                return Task.CompletedTask;
            }
            else if (authorization.Parameter != ConfigurationManager.AppSettings["RefreshEndpointKey"])
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Asynchronous challenge method.
        /// </summary>
        /// <param name="context">Http Authentication Challenge Context</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns><see cref="Task"/> that resolves to Challenge Async method</returns>
        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}