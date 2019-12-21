using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using StardewModdingAPI.Web.Framework.Clients.Pastebin;
using StardewModdingAPI.Web.Framework.Compression;
using StardewModdingAPI.Web.Framework.ConfigModels;

namespace StardewModdingAPI.Web.Framework.Storage
{
    /// <summary>Provides access to raw data storage.</summary>
    internal class StorageProvider : IStorageProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The API client settings.</summary>
        private readonly ApiClientsConfig ClientsConfig;

        /// <summary>The underlying Pastebin client.</summary>
        private readonly IPastebinClient Pastebin;

        /// <summary>The underlying text compression helper.</summary>
        private readonly IGzipHelper GzipHelper;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="clientsConfig">The API client settings.</param>
        /// <param name="pastebin">The underlying Pastebin client.</param>
        /// <param name="gzipHelper">The underlying text compression helper.</param>
        public StorageProvider(IOptions<ApiClientsConfig> clientsConfig, IPastebinClient pastebin, IGzipHelper gzipHelper)
        {
            this.ClientsConfig = clientsConfig.Value;
            this.Pastebin = pastebin;
            this.GzipHelper = gzipHelper;
        }

        /// <summary>Save a text file to storage.</summary>
        /// <param name="title">The display title, if applicable.</param>
        /// <param name="content">The content to upload.</param>
        /// <param name="compress">Whether to gzip the text.</param>
        /// <returns>Returns metadata about the save attempt.</returns>
        public async Task<UploadResult> SaveAsync(string title, string content, bool compress = true)
        {
            try
            {
                using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                string id = Guid.NewGuid().ToString("N");

                BlobClient blob = this.GetAzureBlobClient(id);
                await blob.UploadAsync(stream);

                return new UploadResult(true, id, null);
            }
            catch (Exception ex)
            {
                return new UploadResult(false, null, ex.Message);
            }
        }

        /// <summary>Fetch raw text from storage.</summary>
        /// <param name="id">The storage ID returned by <see cref="SaveAsync"/>.</param>
        public async Task<StoredFileInfo> GetAsync(string id)
        {
            // fetch from Azure/Amazon
            if (Guid.TryParseExact(id, "N", out Guid _))
            {
                // try Azure
                try
                {
                    BlobClient blob = this.GetAzureBlobClient(id);
                    Response<BlobDownloadInfo> response = await blob.DownloadAsync();
                    using BlobDownloadInfo result = response.Value;

                    using StreamReader reader = new StreamReader(result.Content);
                    DateTimeOffset expiry = result.Details.LastModified + TimeSpan.FromDays(this.ClientsConfig.AzureBlobTempExpiryDays);
                    string content = this.GzipHelper.DecompressString(reader.ReadToEnd());

                    return new StoredFileInfo
                    {
                        Success = true,
                        Content = content,
                        Expiry = expiry.UtcDateTime
                    };
                }
                catch (RequestFailedException ex)
                {
                    if (ex.ErrorCode != "BlobNotFound")
                        return new StoredFileInfo { Error = $"Could not fetch that file from storage ({ex.ErrorCode}: {ex.Message})." };
                }

                // try legacy Amazon S3
                {
                    var credentials = new BasicAWSCredentials(accessKey: this.ClientsConfig.AmazonAccessKey, secretKey: this.ClientsConfig.AmazonSecretKey);
                    using IAmazonS3 s3 = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(this.ClientsConfig.AmazonRegion));

                    try
                    {
                        using GetObjectResponse response = await s3.GetObjectAsync(this.ClientsConfig.AmazonTempBucket, $"uploads/{id}");
                        using Stream responseStream = response.ResponseStream;
                        using StreamReader reader = new StreamReader(responseStream);

                        DateTime expiry = response.Expiration.ExpiryDateUtc;
                        string pastebinError = response.Metadata["x-amz-meta-pastebin-error"];
                        string content = this.GzipHelper.DecompressString(reader.ReadToEnd());

                        return new StoredFileInfo
                        {
                            Success = true,
                            Content = content,
                            Expiry = expiry,
                            Warning = pastebinError
                        };
                    }
                    catch (AmazonServiceException ex)
                    {
                        return ex.ErrorCode == "NoSuchKey"
                            ? new StoredFileInfo { Error = "There's no file with that ID." }
                            : new StoredFileInfo { Error = $"Could not fetch that file from AWS S3 ({ex.ErrorCode}: {ex.Message})." };
                    }
                }
            }

            // get from PasteBin
            else
            {
                PasteInfo response = await this.Pastebin.GetAsync(id);
                response.Content = this.GzipHelper.DecompressString(response.Content);
                return new StoredFileInfo
                {
                    Success = response.Success,
                    Content = response.Content,
                    Error = response.Error
                };
            }
        }

        /// <summary>Get a client for reading and writing to Azure Blob storage.</summary>
        /// <param name="id">The file ID to fetch.</param>
        private BlobClient GetAzureBlobClient(string id)
        {
            var azure = new BlobServiceClient(this.ClientsConfig.AzureBlobConnectionString);
            var container = azure.GetBlobContainerClient(this.ClientsConfig.AzureBlobTempContainer);
            return container.GetBlobClient($"uploads/{id}");
        }
    }
}
