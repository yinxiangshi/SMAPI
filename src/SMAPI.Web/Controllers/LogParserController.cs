using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StardewModdingAPI.Web.Framework;
using StardewModdingAPI.Web.Framework.ConfigModels;
using StardewModdingAPI.Web.Framework.LogParser;
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
        private readonly PastebinClient PastebinClient;


        /*********
        ** Public methods
        *********/
        /***
        ** Constructor
        ***/
        /// <summary>Construct an instance.</summary>
        /// <param name="configProvider">The log parser config settings.</param>
        public LogParserController(IOptions<LogParserConfig> configProvider)
        {
            // init Pastebin client
            this.Config = configProvider.Value;
            string version = this.GetType().Assembly.GetName().Version.ToString(3);
            string userAgent = string.Format(this.Config.PastebinUserAgent, version);
            this.PastebinClient = new PastebinClient(this.Config.PastebinBaseUrl, userAgent, this.Config.PastebinUserKey, this.Config.PastebinDevKey);
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
        public async Task<GetPasteResponse> GetAsync(string id)
        {
            return await this.PastebinClient.GetAsync(id);
        }

        /// <summary>Save raw log data.</summary>
        /// <param name="content">The log content to save.</param>
        [HttpPost, Produces("application/json"), AllowLargePosts]
        [Route("log/save")]
        public async Task<SavePasteResponse> PostAsync([FromBody] string content)
        {
            return await this.PastebinClient.PostAsync(content);
        }
    }
}
