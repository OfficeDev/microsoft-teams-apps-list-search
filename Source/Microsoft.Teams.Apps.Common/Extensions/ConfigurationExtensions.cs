// <copyright file="ConfigurationExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Common.Extensions
{
    using System;
    using Microsoft.Teams.Apps.Common.Configuration;
    using Microsoft.Teams.Apps.Common.Exceptions;
    using Microsoft.Teams.Apps.Common.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// The configuration extension class.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Gets a json encoded setting and converts it to the type requested.
        /// Throws a SettingNotFoundException if the requested setting cannot be found
        /// Throws a SettingDeserializationException if there is an issue deserializing the setting value
        /// </summary>
        /// <typeparam name="TKey">The type of the config.</typeparam>
        /// <typeparam name="T">The expected data type of the value.</typeparam>
        /// <param name="configProvider">The configuration provider.</param>
        /// <param name="key">The setting to retrieve</param>
        /// <returns>The type casted setting value</returns>
        /// <exception cref="SettingDeserializationException">Error deserializing the requested configuration setting.</exception>
        public static T GetSetting<TKey, T>(this IConfigProvider configProvider, TKey key)
            where TKey : struct, IConvertible
        {
            var settingString = configProvider.GetSetting(key);
            try
            {
                return JsonConvert.DeserializeObject<T>(settingString);
            }
            catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
            {
                throw new SettingDeserializationException($"Error deserializing the setting {key} to type {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Tries to get setting.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="configProvider">The configuration provider.</param>
        /// <param name="key">The config key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="logger">[optional] The logger object.</param>
        /// <returns>The setting value.</returns>
        public static int TryGetSetting<TKey>(this IConfigProvider configProvider, TKey key, int defaultValue, ILogProvider logger = null)
            where TKey : struct, IConvertible
        {
            var text = TryGetSetting(configProvider, key, null);
            if (int.TryParse(text, out int output))
            {
                return output;
            }

            logger?.LogWarning($"Failed to get int setting {key} using default value {defaultValue}");
            return defaultValue;
        }

        /// <summary>
        /// Tries to get setting.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="configProvider">The configuration provider.</param>
        /// <param name="key">The config key.</param>
        /// <param name="defaultValue">Default value as true or false.</param>
        /// <param name="logger">[optional] The logger object.</param>
        /// <returns>True if the setting has been found; otherwise, false.</returns>
        public static bool TryGetSetting<TKey>(this IConfigProvider configProvider, TKey key, bool defaultValue, ILogProvider logger = null)
            where TKey : struct, IConvertible
        {
            var text = TryGetSetting(configProvider, key, null);
            if (bool.TryParse(text, out bool output))
            {
                return output;
            }

            logger?.LogWarning($"Failed to get bool setting {key} using default value {defaultValue}");
            return defaultValue;
        }

        /// <summary>
        /// Tries to get setting.
        /// </summary>
        /// <typeparam name="TKey">The type of key.</typeparam>
        /// <param name="configProvider">The configuration provider.</param>
        /// <param name="key">The config key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="logger">[optional] The logger object.</param>
        /// <returns>The setting value.</returns>
        public static string TryGetSetting<TKey>(this IConfigProvider configProvider, TKey key, string defaultValue, ILogProvider logger = null)
            where TKey : struct, IConvertible
        {
            try
            {
                return configProvider.GetSetting(key);
            }
            catch
            {
                logger?.LogWarning($"Failed to get setting {key} using default value {defaultValue}");
                return defaultValue;
            }
        }
    }
}
