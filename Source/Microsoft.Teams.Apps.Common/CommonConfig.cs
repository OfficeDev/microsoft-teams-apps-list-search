// <copyright file="CommonConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Common
{
    /// <summary>
    /// Configurations that are used across multiple projects.
    /// </summary>
    public enum CommonConfig
    {
        /// <summary>
        /// Represents the current deployment environment.
        /// </summary>
        Environment,

        /// <summary>
        /// Represents an active directory audience.
        /// </summary>
        ActiveDirectoryAudience,

        /// <summary>
        /// Represents an active directory authority.
        /// </summary>
        ActiveDirectoryAuthority,

        /// <summary>
        /// Represents an active directory certificate location.
        /// </summary>
        ActiveDirectoryCertificateLocation,

        /// <summary>
        /// Represents an active directory certificate name.
        /// </summary>
        ActiveDirectoryCertificateName,

        /// <summary>
        /// Represents an active directory client identifier.
        /// </summary>
        ActiveDirectoryClientId,

        /// <summary>
        /// Represents an active directory tenant.
        /// </summary>
        ActiveDirectoryTenant,

        /// <summary>
        /// Represents an application insights instrumentation key.
        /// </summary>
        ApplicationInsightsInstrumentationKey,

        /// <summary>
        /// Represents an application insights log level.
        /// </summary>
        ApplicationInsightsLogLevel,

        /// <summary>
        /// Determines whether to use the key vault provider.
        /// </summary>
        UseKeyVault,

        /// <summary>
        /// Represents a key vault uri.
        /// </summary>
        KeyVaultUri,
    }
}
