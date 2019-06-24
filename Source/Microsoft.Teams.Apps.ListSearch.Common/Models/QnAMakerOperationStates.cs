// <copyright file="QnAMakerOperationStates.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Models
{
    /// <summary>
    /// Possible operation states of QnA Maker Response
    /// </summary>
    public class QnAMakerOperationStates
    {
        /// <summary>
        /// Not Started
        /// </summary>
        public const string NotStarted = "NotStarted";

        /// <summary>
        /// Running
        /// </summary>
        public const string Running = "Running";

        /// <summary>
        /// Succeeded
        /// </summary>
        public const string Succeeded = "Succeeded";

        /// <summary>
        /// Failed
        /// </summary>
        public const string Failed = "Failed";

        /// <summary>
        /// Bad Argument
        /// </summary>
        public const string BadArgument = "BadArgument";

        /// <summary>
        /// Unauthorized
        /// </summary>
        public const string Unauthorized = "Unauthorized";

        /// <summary>
        /// Forbidden
        /// </summary>
        public const string Forbidden = "Forbidden";

        /// <summary>
        /// NotFound
        /// </summary>
        public const string NotFound = "NotFound";

        /// <summary>
        /// Unspecified
        /// </summary>
        public const string Unspecified = "Unspecified";
    }
}
