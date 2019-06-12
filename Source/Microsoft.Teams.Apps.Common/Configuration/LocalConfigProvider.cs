// <copyright file="LocalConfigProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Common.Configuration
{
    using System;
    using System.Configuration;
    using Microsoft.Teams.Apps.Common.Exceptions;

    /// <summary>
    /// Provides configuration that are stored locally.
    /// </summary>
    /// <seealso cref="IConfigProvider" />
    [Serializable]
    public class LocalConfigProvider : IConfigProvider
    {
        /// <summary>
        /// Gets the setting value.
        /// </summary>
        /// <typeparam name="TConfigKeys">The type of the configuration keys.</typeparam>
        /// <param name="key">The config key.</param>
        /// <returns>The config value.</returns>
        /// <exception cref="SettingNotFoundException">
        /// Specific key configuration not found.
        /// </exception>
        public string GetSetting<TConfigKeys>(TConfigKeys key)
            where TConfigKeys : struct, IConvertible
        {
            string value = ConfigurationManager.AppSettings[key.ToString()];
            if (value != null)
            {
                return value;
            }

            throw new SettingNotFoundException($"{key} configuration not found");
        }
    }
}
