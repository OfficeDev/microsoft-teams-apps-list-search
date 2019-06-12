// <copyright file="ILogProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Common.Logging
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface to implement different log information.
    /// </summary>
    public interface ILogProvider
    {
        /// <summary>
        /// Log a message of the given type.
        /// </summary>
        /// <param name="logType">Type of log</param>
        /// <param name="message">The debug message</param>
        /// <param name="exception">An exception to capture and correlate to the debug event</param>
        /// <param name="properties">A collection of properties that should be associated to the telemetry entry</param>
        /// <param name="correlationId">This is used for tracing a series of events.</param>
        /// <param name="source">The source</param>
        void Log(LogLevel logType, string message, Exception exception, Dictionary<string, string> properties, Guid correlationId, Func<string> source);

        /// <summary>
        /// Log a custom event.
        /// A custom event log provides insight to system administrators by logging information that may not be captured by other log levels.
        /// </summary>
        /// <param name="eventName">The information message</param>
        /// <param name="properties">A collection of properties that should be associated to the telemetry entry</param>
        /// <param name="metrics">The metrics</param>
        /// <param name="correlationId">This is used for tracing a series of events.</param>
        /// <param name="source">The source</param>
        void LogEvent(string eventName, Dictionary<string, string> properties, Dictionary<string, double> metrics, Guid correlationId, Func<string> source);

        /// <summary>
        /// Log a metric. Metrics are aggregated double values that can be processed for the system as a whole
        /// </summary>
        /// <param name="name">Metric Name</param>
        /// <param name="value">The value that you are logging</param>
        /// <param name="properties">[Optional] A collection of properties that should be associated to the metric entry</param>
        /// <param name="logLevel">[Optional] The loglevel that this metric should be logged at</param>
        void LogMetric(string name, double value, Dictionary<string, string> properties = null, LogLevel logLevel = LogLevel.Metric);

        /// <summary>
        /// Log a dependency.
        /// </summary>
        /// <param name="dependencyTypeName">External dependency type. Very low cardinality value for logical grouping and interpretation of fields. Examples are SQL, Azure table, and HTTP.</param>
        /// <param name="dependencyName">Name of the command initiated with this dependency call. Low cardinality value. Examples are stored procedure name and URL path template.</param>
        /// <param name="data">Command initiated by this dependency call. Examples are SQL statement and HTTP URL's with all query parameters.</param>
        /// <param name="startTime">The time when the dependency was called.</param>
        /// <param name="duration">The time taken by the external dependency to handle the call.</param>
        /// <param name="success">True if the dependency call was handled successfully.</param>
        void LogDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success);
    }
}