using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
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

        /// <summary>Save a text file to Pastebin or Amazon S3, if available.</summary>
        /// <param name="title">The display title, if applicable.</param>
        /// <param name="content">The content to upload.</param>
        /// <param name="compress">Whether to gzip the text.</param>
        /// <returns>Returns metadata about the save attempt.</returns>
        public async Task<UploadResult> SaveAsync(string title, string content, bool compress = true)
        {
            // save to PasteBin
            string uploadError;
            {
                SavePasteResult result = await this.Pastebin.PostAsync(title, content);
                if (result.Success)
                    return new UploadResult(true, result.ID, null);

                uploadError = $"Pastebin error: {result.Error ?? "unknown error"}";
            }

            // fallback to S3
            try
            {
                var credentials = new BasicAWSCredentials(accessKey: this.ClientsConfig.AmazonAccessKey, secretKey: this.ClientsConfig.AmazonSecretKey);
                using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                using IAmazonS3 s3 = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(this.ClientsConfig.AmazonRegion));
                using TransferUtility uploader = new TransferUtility(s3);

                string id = Guid.NewGuid().ToString("N");

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = this.ClientsConfig.AmazonTempBucket,
                    Key = $"uploads/{id}",
                    InputStream = stream,
                    Metadata =
                    {
                        // note: AWS will lowercase keys and prefix 'x-amz-meta-'
                        ["smapi-uploaded"] = DateTime.UtcNow.ToString("O"),
                        ["pastebin-error"] = uploadError
                    }
                };

                await uploader.UploadAsync(uploadRequest);

                return new UploadResult(true, id, uploadError);
            }
            catch (Exception ex)
            {
                return new UploadResult(false, null, $"{uploadError}\n{ex.Message}");
            }
        }

        /// <summary>Fetch raw text from storage.</summary>
        /// <param name="id">The storage ID returned by <see cref="SaveAsync"/>.</param>
        public async Task<StoredFileInfo> GetAsync(string id)
        {
            // get from Amazon S3
            if (Guid.TryParseExact(id, "N", out Guid _))
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
    }
}
