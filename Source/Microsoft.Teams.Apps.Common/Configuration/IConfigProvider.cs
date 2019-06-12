// <copyright file="IConfigProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Common.Configuration
{
    using System;

    /// <summary>
    /// Interface for implementing get setting values.
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// Gets the setting value.
        /// </summary>
        /// <typeparam name="TConfigKeys">The type of the configuration keys.</typeparam>
        /// <param name="key">The config key.</param>
        /// <returns>The config value.</returns>
        string GetSetting<TConfigKeys>(TConfigKeys key)
            where TConfigKeys : struct, IConvertible;
    }
}
