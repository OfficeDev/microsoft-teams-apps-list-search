// <copyright file="ConfigHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Helpers
{
    using System.Configuration;
    using System.Threading.Tasks;
    using ListSearch.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Helper for configuration data.
    /// </summary>
    public class ConfigHelper
    {
        private static readonly string ConfigTableName = StorageInfo.ConfigTableName;
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient cloudTableClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigHelper"/> class.
        /// </summary>
        public ConfigHelper()
        {
            this.storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            this.cloudTableClient = this.storageAccount.CreateCloudTableClient();
        }

        /// <summary>
        /// Get configuration value from storage.
        /// </summary>
        /// <param name="configKey">Key of item to search in storage.</param>
        /// <returns>Task that resolves to the config value from storage.</returns>
        public async Task<string> GetConfigValue(string configKey)
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(ConfigTableName);
            TableOperation searchOperation = TableOperation.Retrieve<ConfigEntity>(configKey, configKey);
            TableResult searchResult = await cloudTable.ExecuteAsync(searchOperation);
            return ((ConfigEntity)searchResult.Result).Data;
        }
    }
}