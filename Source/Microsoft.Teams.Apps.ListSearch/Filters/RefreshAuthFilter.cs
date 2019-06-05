// <copyright file="RefreshAuthFilter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Filters
{
    using System;
    using System.Configuration;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Filters;
    using ListSearch.Models;

    /// <summary>
    /// Authentication filter for the refresh endpoint
    /// </summary>
    public class RefreshAuthFilter : Attribute, IAuthenticationFilter
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
            else if (authorization.Scheme != "Basic")
            {
                context.ErrorResult = new AuthenticationFailureResult($"Invalid authorization scheme {authorization.Scheme}", request);
                return Task.CompletedTask;
            }
            else if (string.IsNullOrEmpty(authorization.Parameter))
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                return Task.CompletedTask;
            }

            Tuple<string, string> userNameAndPassword = this.ExtractUserNameAndPassword(authorization.Parameter);
            if (userNameAndPassword == null)
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request);
                return Task.CompletedTask;
            }

            string userName = userNameAndPassword.Item1;
            string password = userNameAndPassword.Item2;

            if (!this.ValidateCredentials(userName, password))
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid username or password", request);
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

        /// <summary>
        /// Validates credentials
        /// </summary>
        /// <param name="userName">username</param>
        /// <param name="password">password</param>
        /// <returns><see cref="bool"/> representing success or failure of validation.</returns>
        private bool ValidateCredentials(string userName, string password)
        {
            if (userName == ConfigurationManager.AppSettings["LogicAppUserName"] && password == ConfigurationManager.AppSettings["LogicAppPassword"])
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Extracts username and password from encoded credentials.
        /// </summary>
        /// <param name="encodedCredentials">Encoded credentials.</param>
        /// <returns><see cref="Tuple"/> of username and password.</returns>
        private Tuple<string, string> ExtractUserNameAndPassword(string encodedCredentials)
        {
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            string userNameAndPassword = encoding.GetString(Convert.FromBase64String(encodedCredentials));

            int separator = userNameAndPassword.IndexOf(':');
            string name = userNameAndPassword.Substring(0, separator);
            string password = userNameAndPassword.Substring(separator + 1);
            return new Tuple<string, string>(name, password);
        }
    }
}