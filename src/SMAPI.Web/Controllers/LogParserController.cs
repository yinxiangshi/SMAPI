using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StardewModdingAPI.Web.Framework;
using StardewModdingAPI.Web.Framework.Clients.Pastebin;
using StardewModdingAPI.Web.Framework.ConfigModels;
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
        private readonly LogParserConfig Config;

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
        /// <param name="configProvider">The log parser config settings.</param>
        /// <param name="pastebin">The Pastebin API client.</param>
        public LogParserController(IOptions<LogParserConfig> configProvider, IPastebinClient pastebin)
        {
            this.Config = configProvider.Value;
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
        public ViewResult Index(string id = null)
        {
            return this.View("Index", new LogParserModel(this.Config.SectionUrl, id));
        }

        /***
        ** JSON
        ***/
        /// <summary>Fetch raw text from Pastebin.</summary>
        /// <param name="id">The Pastebin paste ID.</param>
        [HttpGet, Produces("application/json")]
        [Route("log/fetch/{id}")]
        public async Task<PasteInfo> GetAsync(string id)
        {
            PasteInfo response = await this.Pastebin.GetAsync(id);
            response.Content = this.DecompressString(response.Content);
            return response;
        }

        /// <summary>Save raw log data.</summary>
        /// <param name="content">The log content to save.</param>
        [HttpPost, Produces("application/json"), AllowLargePosts]
        [Route("log/save")]
        public async Task<SavePasteResult> PostAsync([FromBody] string content)
        {
            content = this.CompressString(content);
            return await this.Pastebin.PostAsync(content);
        }


        /*********
        ** Private methods
        *********/
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
            var zipBuffer = new byte[compressedData.Length + 4];
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
                var buffer = new byte[dataLength];
                memoryStream.Position = 0;
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    gZipStream.Read(buffer, 0, buffer.Length);

                // return original string
                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}
