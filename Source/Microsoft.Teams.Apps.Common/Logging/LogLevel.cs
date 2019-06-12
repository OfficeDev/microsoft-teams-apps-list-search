// <copyright file="LogLevel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Common.Logging
{
    /// <summary>
    /// Types of log level.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Represents debug.
        /// </summary>
        Debug,

        /// <summary>
        /// Represents information.
        /// </summary>
        Info,

        /// <summary>
        /// Represents warning.
        /// </summary>
        Warning,

        /// <summary>
        /// Represents metric.
        /// </summary>
        Metric,

        /// <summary>
        /// Represents an event.
        /// </summary>
        Event,

        /// <summary>
        /// Represents an error.
        /// </summary>
        Error,
    }
}
