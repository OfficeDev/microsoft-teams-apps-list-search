// <copyright file="UserTelemetryInitializer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Common.Telemetry
{
    using System;
    using System.Web;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;

    /// <summary>
    /// Initializes user property of telemetry
    /// </summary>
    public class UserTelemetryInitializer : ITelemetryInitializer
    {
        private const string TelemetryUserId = "TelemetryUserId";

        /// <summary>
        /// Set the user id in the platform HTTP context.
        /// </summary>
        /// <param name="platformContext">The HTTP context</param>
        /// <param name="userId">The user id</param>
        public static void SetTelemetryUserId(HttpContext platformContext, string userId)
        {
            platformContext.Items[TelemetryUserId] = userId;
        }

        /// <inheritdoc/>
        public void Initialize(ITelemetry telemetry)
        {
            try
            {
                var platformContext = HttpContext.Current;
                if ((platformContext != null) &&
                    platformContext.Items[TelemetryUserId] is string userId)
                {
                    telemetry.Context.User.Id = userId;
                }
            }
            catch (Exception)
            {
                // Ignore failure to add user id
            }
        }
    }
}
