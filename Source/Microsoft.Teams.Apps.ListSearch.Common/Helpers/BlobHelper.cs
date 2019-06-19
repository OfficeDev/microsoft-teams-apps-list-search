// <copyright file="BlobHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Common.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.ListSearch.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Helper for blob.
    /// </summary>
    public class BlobHelper
    {
        private readonly CloudBlobContainer cloudBlobContainer;
        private readonly Lazy<Task> initializeTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobHelper"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string of storage.</param>
        public BlobHelper(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            this.cloudBlobContainer = cloudBlobClient.GetContainerReference(StorageInfo.BlobContainerName);

            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync());
        }

        /// <summary>
        /// Upload blob
        /// </summary>
        /// <param name="fileContents">Contents of file to be uploaded.</param>
        /// <param name="blobName">Name of the blob</param>
        /// <returns><see cref="Task"/> That represents upload operation.</returns>
        public async Task<string> UploadBlobAsync(string fileContents, string blobName)
        {
            await this.initializeTask.Value;

            CloudBlockBlob cloudBlockBlob = this.cloudBlobContainer.GetBlockBlobReference(blobName);
            await cloudBlockBlob.UploadTextAsync(fileContents);

            return cloudBlockBlob.Uri.ToString();
        }

        /// <summary>
        /// Delete blob.
        /// </summary>
        /// <param name="blobName">Name of the blob to be deleted.</param>
        /// <returns><see cref="Task"/> That represents the delete operation.</returns>
        public async Task DeleteBlobAsync(string blobName)
        {
            await this.initializeTask.Value;

            CloudBlockBlob cloudBlockBlob = this.cloudBlobContainer.GetBlockBlobReference(blobName);
            await cloudBlockBlob.DeleteIfExistsAsync();
        }

        private async Task InitializeAsync()
        {
            if (await this.cloudBlobContainer.CreateIfNotExistsAsync())
            {
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob,
                };
                await this.cloudBlobContainer.SetPermissionsAsync(permissions);
            }
        }
    }
}
