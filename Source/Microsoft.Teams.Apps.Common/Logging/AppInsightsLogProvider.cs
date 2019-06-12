// <copyright file="AppInsightsLogProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

#pragma warning disable SA1513 // Closing brace should be followed by blank line

namespace Microsoft.Teams.Apps.Common.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Teams.Apps.Common.Configuration;

    /// <summary>
    /// App insights log provider class.
    /// </summary>
    /// <seealso cref="ILogProvider" />
    public class AppInsightsLogProvider : ILogProvider
    {
        private const string Message = "Message";
        private const string Source = "Source";
        private readonly Lazy<TelemetryClient> telemetryClient;
        private LogLevel logLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppInsightsLogProvider"/> class.
        /// </summary>
        /// <param name="configProvider">The configuration provider.</param>
        public AppInsightsLogProvider(IConfigProvider configProvider)
        {
            this.telemetryClient = new Lazy<TelemetryClient>(() => this.InitializeClient(new TelemetryClient(), configProvider), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Gets the telemetry client.
        /// </summary>
        private TelemetryClient TelemetryClient => this.telemetryClient.Value;

        /// <inheritdoc />
        public void Log(LogLevel logType, string message, Exception exception, Dictionary<string, string> properties, Guid correlationId, Func<string> source)
        {
            this.Log(logType, LogLevelToSeverityLevel(logType), message, exception, properties, correlationId, source);
        }

        /// <inheritdoc />
        public void LogDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success)
        {
            this.TelemetryClient.TrackDependency(dependencyTypeName, dependencyName, data, startTime, duration, success);
        }

        /// <inheritdoc />
        public void LogEvent(string eventName, Dictionary<string, string> properties, Dictionary<string, double> metrics, Guid correlationId, Func<string> source)
        {
            if (this.logLevel <= LogLevel.Event)
            {
                this.TelemetryClient.TrackEvent(eventName, AddEnvironmentData(properties, source(), correlationId), metrics);
            }
        }

        /// <inheritdoc />
        public void LogMetric(string name, double value, Dictionary<string, string> properties = null, LogLevel loggingLevel = LogLevel.Metric)
        {
            if (this.logLevel <= loggingLevel)
            {
                this.TelemetryClient.TrackMetric(name, value, AddEnvironmentData(properties));
            }
        }

        /// <summary>
        /// This is used by ExceptionTelemetry and TraceTelemetry to identify severity level of log.
        /// </summary>
        /// <param name="logType">Type of the log.</param>
        /// <returns>Severity level of log.</returns>
        /// <exception cref="NotImplementedException">Direct logging with specified log type is not implemented.</exception>
        private static SeverityLevel LogLevelToSeverityLevel(LogLevel logType)
        {
            switch (logType)
            {
                case LogLevel.Debug:
                    return SeverityLevel.Verbose;
                case LogLevel.Error:
                    return SeverityLevel.Error;
                case LogLevel.Warning:
                    return SeverityLevel.Warning;
                case LogLevel.Info:
                    return SeverityLevel.Information;
                default:
                    throw new NotImplementedException($"Direct logging with LogLevel {logType} not implemented.");
            }
        }

        /// <summary>
        /// Adds the environment data.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="source">The source.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <returns>A key value property.</returns>
        private static Dictionary<string, string> AddEnvironmentData(Dictionary<string, string> properties, string source = "", Guid? correlationId = null)
        {
            if (properties == null)
            {
                properties = new Dictionary<string, string>();
            }
            if (correlationId != null && !properties.ContainsKey(CommonConstant.CorrelationId))
            {
                properties.Add(CommonConstant.CorrelationId, correlationId.ToString());
            }
            if (!string.IsNullOrEmpty(source) && !properties.ContainsKey(Source))
            {
                properties.Add(Source, source);
            }
            return properties;
        }

        /// <summary>
        /// Initializes the client.
        /// </summary>
        /// <param name="telemetryClient">The telemetry client.</param>
        /// <param name="configProvider">The configuration provider.</param>
        /// <returns>Telemetry client object.</returns>
        private TelemetryClient InitializeClient(TelemetryClient telemetryClient, IConfigProvider configProvider)
        {
            var telemetryKey = configProvider.GetSetting(CommonConfig.ApplicationInsightsInstrumentationKey);
            var correlationId = Guid.NewGuid();
            string Source() => $"{nameof(AppInsightsLogProvider)}.cs {nameof(AppInsightsLogProvider)}";
            if (string.IsNullOrEmpty(telemetryKey))
            {
                telemetryClient.TrackTrace("Error getting application insights instrumentation key", SeverityLevel.Error, AddEnvironmentData(null, Source(), correlationId));
            }
            else
            {
                TelemetryConfiguration.Active.InstrumentationKey = telemetryKey;
                telemetryClient.InstrumentationKey = telemetryKey;
            }

            var logLevelText = configProvider.GetSetting(CommonConfig.ApplicationInsightsLogLevel);
            this.logLevel = LogLevel.Debug;
            if (!Enum.TryParse(logLevelText, out this.logLevel))
            {
                telemetryClient.TrackTrace("Error parsing log level", SeverityLevel.Error, AddEnvironmentData(new Dictionary<string, string> { { "Actual Value", logLevelText } }, Source(), correlationId));
            }
            if (this.logLevel > LogLevel.Debug)
            {
                telemetryClient.TrackTrace("Error parsing log level", SeverityLevel.Verbose, AddEnvironmentData(new Dictionary<string, string> { { "Actual Value", logLevelText } }, Source(), correlationId));
                this.Log(LogLevel.Debug, "Finished booting Appinsights logging", null, null, correlationId, Source);
            }
            return telemetryClient;
        }

        /// <summary>
        /// Logs the specified log type.
        /// </summary>
        /// <param name="logType">The log type.</param>
        /// <param name="severityLevel">The severity level.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception object.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <param name="getSource">The get source function.</param>
        private void Log(LogLevel logType, SeverityLevel severityLevel, string message, Exception exception, Dictionary<string, string> properties, Guid correlationId, Func<string> getSource)
        {
            if (this.logLevel > logType)
            {
                return;
            }

            var source = getSource();
            properties = AddEnvironmentData(properties, source, correlationId);
            if (exception != null)
            {
                properties[Message] = message;
                this.TelemetryClient.TrackException(exception, properties);
            }
            this.TelemetryClient.TrackTrace(message, severityLevel, properties);
        }
    }
}
