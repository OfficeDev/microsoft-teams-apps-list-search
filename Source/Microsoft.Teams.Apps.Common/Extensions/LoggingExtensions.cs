// <copyright file="LoggingExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Common;
    using Microsoft.Teams.Apps.Common.Logging;

    /// <summary>
    /// The logging extension class.
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Extracts the correlation identifier.
        /// </summary>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <param name="exception">The exception type.</param>
        /// <returns>Correlation identifier.</returns>
        public static Guid ExtractCorrelationId(Guid? correlationId, Exception exception)
        {
            if (correlationId.HasValue)
            {
                return correlationId.Value;
            }

            if (exception != null && exception.Data.Contains(CommonConstant.CorrelationId))
            {
                var value = exception.Data[CommonConstant.CorrelationId];
                if (value != null && Guid.TryParse(value.ToString(), out Guid tmp))
                {
                    return tmp;
                }
            }

            return Guid.NewGuid();
        }

        /// <summary>
        /// Log a debugging message.
        /// A debug message is intended to help debug an issue and won't be logged in production
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="message">The debug message</param>
        /// <param name="properties">[Optional] A collection of properties that should be associated to the telemetry entry</param>
        /// <param name="exception">[Optional] An exception to capture and correlate to the debug event</param>
        /// <param name="correlationId">[Optional] This is used for tracing a series of events. If this is left null then a new ID will be created and returned by this method. Otherwise pass in an ID from a previous logging event</param>
        /// <param name="callerName">[Optional] source name; set to caller's name if not specified</param>
        /// <param name="callerPath">[Optional] source path; set to caller's path if not specified</param>
        /// <param name="callerLineNumber">[Optional] source line number; set to caller's source line number if not specified</param>
        /// <returns>The correlation Id for this logging event if a value was provided in the method call this will return the value provided otherwise a new Guid will be generated</returns>
        public static Guid LogDebug(this ILogProvider logger, string message, Dictionary<string, string> properties = null, Exception exception = null, Guid? correlationId = null, [CallerMemberName] string callerName = null, [CallerFilePath] string callerPath = null, [CallerLineNumber] int callerLineNumber = 0)
        {
            var id = ExtractCorrelationId(correlationId, exception);
            logger.Log(LogLevel.Debug, message, exception, properties, correlationId: id, source: () => CallerInfoToSource(callerName, callerPath, callerLineNumber));
            return id;
        }

        /// <summary>
        ///  Log an error message. In appinsights this will be filed with a severity level of error.
        ///  An error message should be logged when there is a critical or system stopping error.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="message">The error message</param>
        /// <param name="exception">[Optional] The exception that caused this error</param>
        /// <param name="properties">[Optional] A collection of properties that should be associated to the telemetry entry</param>
        /// <param name="correlationId">[Optional] This is used for tracing a series of events. If this is left null then a new ID will be created and returned by this method. Otherwise pass in an ID from a previous logging event</param>
        /// <param name="callerName">[Optional] source name; set to caller's name if not specified</param>
        /// <param name="callerPath">[Optional] source path; set to caller's path if not specified</param>
        /// <param name="callerLineNumber">[Optional] source line number; set to caller's source line number if not specified</param>
        /// <returns>The correlation Id for this logging event if a value was provided in the method call this will return the value provided otherwise a new Guid will be generated</returns>
        public static Guid LogError(this ILogProvider logger, string message, Exception exception = null, Dictionary<string, string> properties = null, Guid? correlationId = null, [CallerMemberName] string callerName = null, [CallerFilePath] string callerPath = null, [CallerLineNumber] int callerLineNumber = 0)
        {
            var id = ExtractCorrelationId(correlationId, exception);
            logger.Log(LogLevel.Error, message, exception, properties, id, source: () => CallerInfoToSource(callerName, callerPath, callerLineNumber));
            return id;
        }

        /// <summary>
        /// Log a custom event.
        /// A custom event log provides insight to system administrators by logging information that may not be captured by other log levels.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="eventName">The information message</param>
        /// <param name="properties">[Optional] A collection of properties that should be associated to the telemetry entry</param>
        /// <param name="metrics">[Optional] The metrics</param>
        /// <param name="correlationId">[Optional] This is used for tracing a series of events. If this is left null then a new ID will be created and returned by this method. Otherwise pass in an ID from a previous logging event</param>
        /// <param name="callerName">[Optional] source name; set to caller's name if not specified</param>
        /// <param name="callerPath">[Optional] source path; set to caller's path if not specified</param>
        /// <param name="callerLineNumber">[Optional] source line number; set to caller's source line number if not specified</param>
        /// <returns>The correlation Id for this logging event if a value was provided in the method call this will return the value provided otherwise a new Guid will be generated</returns>
        public static Guid LogEvent(this ILogProvider logger, string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null, Guid? correlationId = null, [CallerMemberName] string callerName = null, [CallerFilePath] string callerPath = null, [CallerLineNumber] int callerLineNumber = 0)
        {
            var id = ExtractCorrelationId(correlationId, null);
            logger.LogEvent(eventName, properties, metrics, id, source: () => CallerInfoToSource(callerName, callerPath, callerLineNumber));
            return id;
        }

        /// <summary>
        /// Log a custom event.
        /// A custom event log provides insight to system administrators by logging information that may not be captured by other log levels.
        /// </summary>
        /// <typeparam name="TEvent">Type of event name</typeparam>
        /// <typeparam name="TKey">Type of dictionary key</typeparam>
        /// <param name="logger">The logger</param>
        /// <param name="event">The information message</param>
        /// <param name="properties">[Optional] A collection of properties that should be associated to the telemetry entry</param>
        /// <param name="metrics">[Optional] The metrics</param>
        /// <param name="correlationId">[Optional] This is used for tracing a series of events. If this is left null then a new ID will be created and returned by this method. Otherwise pass in an ID from a previous logging event</param>
        /// <param name="callerName">[Optional] source name; set to caller's name if not specified</param>
        /// <param name="callerPath">[Optional] source path; set to caller's path if not specified</param>
        /// <param name="callerLineNumber">[Optional] source line number; set to caller's source line number if not specified</param>
        /// <returns>The correlation Id for this logging event if a value was provided in the method call this will return the value provided otherwise a new Guid will be generated</returns>
        public static Guid LogEvent<TEvent, TKey>(this ILogProvider logger, TEvent @event, Dictionary<TKey, string> properties = null, Dictionary<TKey, double> metrics = null, Guid? correlationId = null, [CallerMemberName] string callerName = null, [CallerFilePath] string callerPath = null, [CallerLineNumber] int callerLineNumber = 0)
            where TEvent : struct
        {
            return logger.LogEvent(
                @event.ToString(),
                properties?.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
                metrics?.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
                correlationId,
                callerName,
                callerPath,
                callerLineNumber);
        }

        /// <summary>
        /// Log a information message.
        /// An information log is an event that should be logged during normal operation to provide insight to system administrators
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="message">The information message</param>
        /// <param name="properties">[Optional] A collection of properties that should be associated to the telemetry entry</param>
        /// <param name="correlationId">[Optional] This is used for tracing a series of events. If this is left null then a new ID will be created and returned by this method. Otherwise pass in an ID from a previous logging event</param>
        /// <param name="callerName">[Optional] source name; set to caller's name if not specified</param>
        /// <param name="callerPath">[Optional] source path; set to caller's path if not specified</param>
        /// <param name="callerLineNumber">[Optional] source line number; set to caller's source line number if not specified</param>
        /// <returns>The correlation Id for this logging event if a value was provided in the method call this will return the value provided otherwise a new Guid will be generated</returns>
        public static Guid LogInfo(this ILogProvider logger, string message, Dictionary<string, string> properties = null, Guid? correlationId = null, [CallerMemberName] string callerName = null, [CallerFilePath] string callerPath = null, [CallerLineNumber] int callerLineNumber = 0)
        {
            var id = ExtractCorrelationId(correlationId, null);
            logger.Log(LogLevel.Info, message, exception: null, properties: properties, correlationId: id, source: () => CallerInfoToSource(callerName, callerPath, callerLineNumber));
            return id;
        }

        /// <summary>
        /// Log a warning message. In appinsights this will be filed with a severity level of warning.
        /// A warning message should be logged when there is a non critical recoverable or transient error that is good to note but does not mean that the system is failing
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="message">The warning message</param>
        /// <param name="properties">[Optional] A collection of properties that should be associated to the telemetry entry</param>
        /// <param name="exception">[Optional] An exception to capture and correlate to the warning event</param>
        /// <param name="correlationId">[Optional] This is used for tracing a series of events. If this is left null then a new ID will be created and returned by this method. Otherwise pass in an ID from a previous logging event</param>
        /// <param name="callerName">[Optional] source name; set to caller's name if not specified</param>
        /// <param name="callerPath">[Optional] source path; set to caller's path if not specified</param>
        /// <param name="callerLineNumber">[Optional] source line number; set to caller's source line number if not specified</param>
        /// <returns>The correlation Id for this logging event if a value was provided in the method call this will return the value provided otherwise a new Guid will be generated</returns>
        public static Guid LogWarning(this ILogProvider logger, string message, Dictionary<string, string> properties = null, Exception exception = null, Guid? correlationId = null, [CallerMemberName] string callerName = null, [CallerFilePath] string callerPath = null, [CallerLineNumber] int callerLineNumber = 0)
        {
            var id = ExtractCorrelationId(correlationId, exception);
            logger.Log(LogLevel.Warning, message, exception, properties, id, source: () => CallerInfoToSource(callerName, callerPath, callerLineNumber));
            return id;
        }

        /// <summary>
        /// Log callers information.
        /// </summary>
        /// <param name="callerName">Name of the caller.</param>
        /// <param name="callerPath">The caller path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        /// <returns>The caller info.</returns>
        private static string CallerInfoToSource(string callerName, string callerPath, int callerLineNumber)
        {
            return $"{Path.GetFileName(callerPath)}:{callerLineNumber} {callerName}";
        }
    }
}
