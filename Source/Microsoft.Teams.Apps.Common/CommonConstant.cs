// <copyright file="CommonConstant.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Common
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Store common constants used in project.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class CommonConstant
    {
        /// <summary>
        /// The correlation identifier.
        /// </summary>
        public const string CorrelationId = "CorrelationId";

        /// <summary>
        /// Azure storage partition key.
        /// </summary>
        public const string PartitionKey = "PartitionKey";

        /// <summary>
        /// Azure storage row key.
        /// </summary>
        public const string RowKey = "RowKey";

        /// <summary>
        /// The epoch value.
        /// </summary>
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
