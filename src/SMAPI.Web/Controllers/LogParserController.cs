using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StardewModdingAPI.Toolkit.Utilities;
using StardewModdingAPI.Web.Framework;
using StardewModdingAPI.Web.Framework.Clients.Pastebin;
using StardewModdingAPI.Web.Framework.Compression;
using StardewModdingAPI.Web.Framework.ConfigModels;
using StardewModdingAPI.Web.Framework.LogParsing;
using StardewModdingAPI.Web.Framework.LogParsing.Models;
using StardewModdingAPI.Web.ViewModels;

namespace StardewModdingAPI.Web.Controllers
{
    /// <summary>Provides a web UI and API for parsing SMAPI log files.</summary>
    internal class LogParserController : Controller
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
        /***
        ** Constructor
        ***/
        /// <summary>Construct an instance.</summary>
        /// <param name="clientsConfig">The API client settings.</param>
        /// <param name="pastebin">The Pastebin API client.</param>
        /// <param name="gzipHelper">The underlying text compression helper.</param>
        public LogParserController(IOptions<ApiClientsConfig> clientsConfig, IPastebinClient pastebin, IGzipHelper gzipHelper)
        {
            this.ClientsConfig = clientsConfig.Value;
            this.Pastebin = pastebin;
            this.GzipHelper = gzipHelper;
        }

        /***
        ** Web UI
        ***/
        /// <summary>Render the log parser UI.</summary>
        /// <param name="id">The paste ID.</param>
        /// <param name="raw">Whether to display the raw unparsed log.</param>
        [HttpGet]
        [Route("log")]
        [Route("log/{id}")]
        public async Task<ViewResult> Index(string id = null, bool raw = false)
        {
            // fresh page
            if (string.IsNullOrWhiteSpace(id))
                return this.View("Index", this.GetModel(id));

            // log page
            PasteInfo paste = await this.GetAsync(id);
            ParsedLog log = paste.Success
                ? new LogParser().Parse(paste.Content)
                : new ParsedLog { IsValid = false, Error = paste.Error };

            return this.View("Index", this.GetModel(id, uploadWarning: paste.Warning, expiry: paste.Expiry).SetResult(log, raw));
        }

        /***
        ** JSON
        ***/
        /// <summary>Save raw log data.</summary>
        [HttpPost, AllowLargePosts]
        [Route("log")]
        public async Task<ActionResult> PostAsync()
        {
            // get raw log text
            string input = this.Request.Form["input"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(input))
                return this.View("Index", this.GetModel(null, uploadError: "The log file seems to be empty."));

            // upload log
            input = this.GzipHelper.CompressString(input);
            var uploadResult = await this.TrySaveLog(input);
            if (!uploadResult.Succeeded)
                return this.View("Index", this.GetModel(null, uploadError: uploadResult.UploadError));

            // redirect to view
            return this.Redirect(this.Url.Action("Index", "LogParser", new { id = uploadResult.ID }));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Fetch raw text from Pastebin.</summary>
        /// <param name="id">The Pastebin paste ID.</param>
        private async Task<PasteInfo> GetAsync(string id)
        {
            // get from Amazon S3
            if (Guid.TryParseExact(id, "N", out Guid _))
            {
                var credentials = new BasicAWSCredentials(accessKey: this.ClientsConfig.AmazonAccessKey, secretKey: this.ClientsConfig.AmazonSecretKey);

                using (IAmazonS3 s3 = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(this.ClientsConfig.AmazonRegion)))
                {
                    try
                    {
                        using (GetObjectResponse response = await s3.GetObjectAsync(this.ClientsConfig.AmazonLogBucket, $"logs/{id}"))
                        using (Stream responseStream = response.ResponseStream)
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            DateTime expiry = response.Expiration.ExpiryDateUtc;
                            string pastebinError = response.Metadata["x-amz-meta-pastebin-error"];
                            string content = this.GzipHelper.DecompressString(reader.ReadToEnd());

                            return new PasteInfo
                            {
                                Success = true,
                                Content = content,
                                Expiry = expiry,
                                Warning = pastebinError
                            };
                        }
                    }
                    catch (AmazonServiceException ex)
                    {
                        return ex.ErrorCode == "NoSuchKey"
                            ? new PasteInfo { Error = "There's no log with that ID." }
                            : new PasteInfo { Error = $"Could not fetch that log from AWS S3 ({ex.ErrorCode}: {ex.Message})." };
                    }
                }
            }

            // get from PasteBin
            else
            {
                PasteInfo response = await this.Pastebin.GetAsync(id);
                response.Content = this.GzipHelper.DecompressString(response.Content);
                return response;
            }
        }

        /// <summary>Save a log to Pastebin or Amazon S3, if available.</summary>
        /// <param name="content">The content to upload.</param>
        /// <returns>Returns metadata about the save attempt.</returns>
        private async Task<UploadResult> TrySaveLog(string content)
        {
            // save to PasteBin
            string uploadError;
            {
                SavePasteResult result = await this.Pastebin.PostAsync($"SMAPI log {DateTime.UtcNow:s}", content);
                if (result.Success)
                    return new UploadResult(true, result.ID, null);

                uploadError = $"Pastebin error: {result.Error ?? "unknown error"}";
            }

            // fallback to S3
            try
            {
                var credentials = new BasicAWSCredentials(accessKey: this.ClientsConfig.AmazonAccessKey, secretKey: this.ClientsConfig.AmazonSecretKey);

                using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                using (IAmazonS3 s3 = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(this.ClientsConfig.AmazonRegion)))
                using (TransferUtility uploader = new TransferUtility(s3))
                {
                    string id = Guid.NewGuid().ToString("N");

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = this.ClientsConfig.AmazonLogBucket,
                        Key = $"logs/{id}",
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
            }
            catch (Exception ex)
            {
                return new UploadResult(false, null, $"{uploadError}\n{ex.Message}");
            }
        }

        /// <summary>Build a log parser model.</summary>
        /// <param name="pasteID">The paste ID.</param>
        /// <param name="expiry">When the uploaded file will no longer be available.</param>
        /// <param name="uploadWarning">A non-blocking warning while uploading the log.</param>
        /// <param name="uploadError">An error which occurred while uploading the log.</param>
        private LogParserModel GetModel(string pasteID, DateTime? expiry = null, string uploadWarning = null, string uploadError = null)
        {
            Platform? platform = this.DetectClientPlatform();

            return new LogParserModel(pasteID, platform)
            {
                UploadWarning = uploadWarning,
                UploadError = uploadError,
                Expiry = expiry
            };
        }

        /// <summary>Detect the viewer's OS.</summary>
        /// <returns>Returns the viewer OS if known, else null.</returns>
        private Platform? DetectClientPlatform()
        {
            string userAgent = this.Request.Headers["User-Agent"];
            switch (userAgent)
            {
                case string ua when ua.Contains("Windows"):
                    return Platform.Windows;

                case string ua when ua.Contains("Android"): // check for Android before Linux because Android user agents also contain Linux
                    return Platform.Android;

                case string ua when ua.Contains("Linux"):
                    return Platform.Linux;

                case string ua when ua.Contains("Mac"):
                    return Platform.Mac;

                default:
                    return null;
            }
        }

        /// <summary>The result of an attempt to upload a file.</summary>
        private class UploadResult
        {
            /*********
            ** Accessors
            *********/
            /// <summary>Whether the file upload succeeded.</summary>
            public bool Succeeded { get; }

            /// <summary>The file ID, if applicable.</summary>
            public string ID { get; }

            /// <summary>The upload error, if any.</summary>
            public string UploadError { get; }


            /*********
            ** Public methods
            *********/
            /// <summary>Construct an instance.</summary>
            /// <param name="succeeded">Whether the file upload succeeded.</param>
            /// <param name="id">The file ID, if applicable.</param>
            /// <param name="uploadError">The upload error, if any.</param>
            public UploadResult(bool succeeded, string id, string uploadError)
            {
                this.Succeeded = succeeded;
                this.ID = id;
                this.UploadError = uploadError;
            }
        }
    }
}
