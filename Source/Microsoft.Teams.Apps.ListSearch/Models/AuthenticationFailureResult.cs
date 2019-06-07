// <copyright file="AuthenticationFailureResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Models
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    /// <summary>
    /// Authentication Failure Result
    /// </summary>
    public class AuthenticationFailureResult : IHttpActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationFailureResult"/> class.
        /// </summary>
        /// <param name="reasonPhrase">Reason phrase for failure</param>
        /// <param name="request">Request</param>
        public AuthenticationFailureResult(string reasonPhrase, HttpRequestMessage request)
        {
            this.ReasonPhrase = reasonPhrase;
            this.Request = request;
        }

        /// <summary>
        /// Gets Reason Phrase
        /// </summary>
        public string ReasonPhrase { get; private set; }

        /// <summary>
        /// Gets Request
        /// </summary>
        public HttpRequestMessage Request { get; private set; }

        /// <summary>
        /// Executes Task
        /// </summary>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns><see cref="Task"/> that resolves to <see cref="HttpResponseMessage"/></returns>
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(this.Execute());
        }

        /// <summary>
        /// Execute
        /// </summary>
        /// <returns><see cref="HttpResponseMessage"/> containing request message and reason phrase.</returns>
        private HttpResponseMessage Execute()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                RequestMessage = this.Request,
                ReasonPhrase = this.ReasonPhrase,
            };
            return response;
        }
    }
}