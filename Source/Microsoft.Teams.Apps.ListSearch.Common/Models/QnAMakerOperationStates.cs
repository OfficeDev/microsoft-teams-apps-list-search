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
        public static readonly string NotStarted = "NotStarted";

        /// <summary>
        /// Running
        /// </summary>
        public static readonly string Running = "Running";

        /// <summary>
        /// Succeeded
        /// </summary>
        public static readonly string Succeeded = "Succeeded";

        /// <summary>
        /// Failed
        /// </summary>
        public static readonly string Failed = "Failed";

        /// <summary>
        /// Bad Argument
        /// </summary>
        public static readonly string BadArgument = "BadArgument";

        /// <summary>
        /// Unauthorized
        /// </summary>
        public static readonly string Unauthorized = "Unauthorized";

        /// <summary>
        /// Forbidden
        /// </summary>
        public static readonly string Forbidden = "Forbidden";

        /// <summary>
        /// NotFound
        /// </summary>
        public static readonly string NotFound = "NotFound";

        /// <summary>
        /// Unspecified
        /// </summary>
        public static readonly string Unspecified = "Unspecified";
    }
}
