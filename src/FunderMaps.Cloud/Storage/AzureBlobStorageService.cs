﻿using FunderMaps.Core.Helpers;
using FunderMaps.Core.Interfaces;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FunderMaps.Cloud.Storage
{
    // FUTURE: Use Azure SDK Storage v12

    /// <summary>
    /// Azure storage implementing file storage service.
    /// </summary>
    internal class AzureBlobStorageService : IFileStorageService
    {
        private readonly FileStorageOptions _options;
        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudBlobClient _blobClient;

        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="options">File service options.</param>
        /// <param name="config">Application configuration.</param>
        public AzureBlobStorageService(IOptions<FileStorageOptions> options, IConfiguration config)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _storageAccount = CloudStorageAccount.Parse(config.GetConnectionString("AzureStorageConnectionString"));
            _blobClient = _storageAccount.CreateCloudBlobClient();
        }

        /// <summary>
        /// Set container name.
        /// </summary>
        /// <param name="name">Name of container.</param>
        /// <returns>Blob container.</returns>
        protected CloudBlobContainer SetContainer(string name)
            => _blobClient.GetContainerReference(_options.StorageContainers.TryGetValue(name, out string store) ? store : name);

        /// <summary>
        /// Prepare blob for storage.
        /// </summary>
        /// <param name="container">Where to store the file.</param>
        /// <param name="filename">The actual file on disk.</param>
        /// <param name="properties">Blob properties.</param>
        /// <returns>The blob block.</returns>
        protected CloudBlockBlob PrepareBlob(string container, string filename, BlobProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            var cloudBlockBlob = SetContainer(container).GetBlockBlobReference(filename);

            cloudBlockBlob.Properties.CacheControl = properties.CacheControl;
            cloudBlockBlob.Properties.ContentMD5 = properties.ContentMD5;
            cloudBlockBlob.Properties.ContentType = properties.ContentType;
            cloudBlockBlob.Properties.ContentEncoding = properties.ContentEncoding;
            cloudBlockBlob.Properties.ContentLanguage = properties.ContentLanguage;
            cloudBlockBlob.Properties.ContentDisposition = properties.ContentDisposition;

            return cloudBlockBlob;
        }

        /// <summary>
        /// Check if a file exist in storage.
        /// </summary>
        /// <param name="store">Storage container.</param>
        /// <param name="name">File name.</param>
        /// <returns>True if file exist, false otherwise.</returns>
        public async ValueTask<bool> FileExists(string store, string name)
        {
            return await SetContainer(store)
                .GetBlockBlobReference(name)
                .ExistsAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve file access link.
        /// </summary>
        /// <param name="store">Storage container.</param>
        /// <param name="name">File name.</param>
        /// <param name="hoursValid">How long the link is valid in hours.</param>
        /// <returns>The generated link.</returns>
        public Uri GetAccessLink(string store, string name, double hoursValid = 1)
        {
            var cloudBlockBlob = SetContainer(store).GetBlockBlobReference(name);

            // Generate the shared access signature on the blob, setting the constraints directly on the signature.
            var sasBlobToken = cloudBlockBlob.GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(hoursValid),
                Permissions = SharedAccessBlobPermissions.Read,
            });

            return new Uri(cloudBlockBlob.Uri, sasBlobToken);
        }

        /// <summary>
        /// Retrieve account name for the storage service.
        /// </summary>
        /// <returns>Account name.</returns>
        public async ValueTask<string> GetStorageNameAsync()
        {
            var properties = await _blobClient.GetAccountPropertiesAsync().ConfigureAwait(false);
            return properties.AccountKind;
        }

        /// <summary>
        /// Store the file in the data store.
        /// </summary>
        /// <param name="store">Storage container.</param>
        /// <param name="name">File name.</param>
        /// <param name="content">Content array.</param>
        public async ValueTask StoreFileAsync(string store, string name, byte[] content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            await PrepareBlob(store, name, new BlobProperties())
                .UploadFromByteArrayAsync(content, 0, content.Length)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Store the file in the data store.
        /// </summary>
        /// /// <param name="store">Storage container.</param>
        /// <param name="file">Application file.</param>
        /// <param name="content">Content array.</param>
        public async ValueTask StoreFileAsync(string store, FileWrapper file, byte[] content)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            await PrepareBlob(store, file.FileName, new BlobProperties
            {
                ContentType = file.ContentType
            })
                .UploadFromByteArrayAsync(content, 0, content.Length)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Store the file in the data store.
        /// </summary>
        /// /// <param name="store">Storage container.</param>
        /// <param name="name">File name.</param>
        /// <param name="stream">Content stream.</param>
        public async ValueTask StoreFileAsync(string store, string name, Stream stream)
        {
            await PrepareBlob(store, name, new BlobProperties())
                .UploadFromStreamAsync(stream)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Store the file in the data store.
        /// </summary>
        /// /// <param name="store">Storage container.</param>
        /// <param name="file">Application file.</param>
        /// <param name="stream">Content stream.</param>
        public async ValueTask StoreFileAsync(string store, FileWrapper file, Stream stream)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            await PrepareBlob(store, file.FileName, new BlobProperties
            {
                ContentType = file.ContentType
            })
                .UploadFromStreamAsync(stream)
                .ConfigureAwait(false);
        }
    }
}