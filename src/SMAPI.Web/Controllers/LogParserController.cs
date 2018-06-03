using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StardewModdingAPI.Web.Framework;
using StardewModdingAPI.Web.Framework.Clients.Pastebin;
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
        ** Properties
        *********/
        /// <summary>The log parser config settings.</summary>
        private readonly ContextConfig Config;

        /// <summary>The underlying Pastebin client.</summary>
        private readonly IPastebinClient Pastebin;

        /// <summary>The first bytes in a valid zip file.</summary>
        /// <remarks>See <a href="https://en.wikipedia.org/wiki/Zip_(file_format)#File_headers"/>.</remarks>
        private const uint GzipLeadBytes = 0x8b1f;


        /*********
        ** Public methods
        *********/
        /***
        ** Constructor
        ***/
        /// <summary>Construct an instance.</summary>
        /// <param name="contextProvider">The context config settings.</param>
        /// <param name="pastebin">The Pastebin API client.</param>
        public LogParserController(IOptions<ContextConfig> contextProvider, IPastebinClient pastebin)
        {
            this.Config = contextProvider.Value;
            this.Pastebin = pastebin;
        }

        /***
        ** Web UI
        ***/
        /// <summary>Render the log parser UI.</summary>
        /// <param name="id">The paste ID.</param>
        [HttpGet]
        [Route("log")]
        [Route("log/{id}")]
        public async Task<ViewResult> Index(string id = null)
        {
            // fresh page
            if (string.IsNullOrWhiteSpace(id))
                return this.View("Index", new LogParserModel(this.Config.LogParserUrl, id, null));

            // log page
            PasteInfo paste = await this.GetAsync(id);
            ParsedLog log = paste.Success
                ? new LogParser().Parse(paste.Content)
                : new ParsedLog { IsValid = false, Error = "Pastebin error: " + paste.Error };
            return this.View("Index", new LogParserModel(this.Config.LogParserUrl, id, log));
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
                return this.View("Index", new LogParserModel(this.Config.LogParserUrl, null, null) { UploadError = "The log file seems to be empty." });

            // upload log
            input = this.CompressString(input);
            SavePasteResult result = await this.Pastebin.PostAsync(input);

            // handle errors
            if (!result.Success)
                return this.View("Index", new LogParserModel(this.Config.LogParserUrl, result.ID, null) { UploadError = $"Pastebin error: {result.Error ?? "unknown error"}" });

            // redirect to view
            UriBuilder uri = new UriBuilder(new Uri(this.Config.LogParserUrl));
            uri.Path = uri.Path.TrimEnd('/') + '/' + result.ID;
            return this.Redirect(uri.Uri.ToString());
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Fetch raw text from Pastebin.</summary>
        /// <param name="id">The Pastebin paste ID.</param>
        private async Task<PasteInfo> GetAsync(string id)
        {
            PasteInfo response = await this.Pastebin.GetAsync(id);
            response.Content = this.DecompressString(response.Content);
            return response;
        }

        /// <summary>Compress a string.</summary>
        /// <param name="text">The text to compress.</param>
        /// <remarks>Derived from <a href="https://stackoverflow.com/a/17993002/262123"/>.</remarks>
        private string CompressString(string text)
        {
            // get raw bytes
            byte[] buffer = Encoding.UTF8.GetBytes(text);

            // compressed
            byte[] compressedData;
            using (MemoryStream stream = new MemoryStream())
            {
                using (GZipStream zipStream = new GZipStream(stream, CompressionLevel.Optimal, leaveOpen: true))
                    zipStream.Write(buffer, 0, buffer.Length);

                stream.Position = 0;
                compressedData = new byte[stream.Length];
                stream.Read(compressedData, 0, compressedData.Length);
            }

            // prefix length
            byte[] zipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, zipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, zipBuffer, 0, 4);

            // return string representation
            return Convert.ToBase64String(zipBuffer);
        }

        /// <summary>Decompress a string.</summary>
        /// <param name="rawText">The compressed text.</param>
        /// <remarks>Derived from <a href="https://stackoverflow.com/a/17993002/262123"/>.</remarks>
        private string DecompressString(string rawText)
        {
            // get raw bytes
            byte[] zipBuffer;
            try
            {
                zipBuffer = Convert.FromBase64String(rawText);
            }
            catch
            {
                return rawText; // not valid base64, wasn't compressed by the log parser
            }

            // skip if not gzip
            if (BitConverter.ToUInt16(zipBuffer, 4) != LogParserController.GzipLeadBytes)
                return rawText;

            // decompress
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // read length prefix
                int dataLength = BitConverter.ToInt32(zipBuffer, 0);
                memoryStream.Write(zipBuffer, 4, zipBuffer.Length - 4);

                // read data
                byte[] buffer = new byte[dataLength];
                memoryStream.Position = 0;
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    gZipStream.Read(buffer, 0, buffer.Length);

                // return original string
                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}
