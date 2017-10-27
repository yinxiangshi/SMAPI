using Microsoft.AspNetCore.Mvc;

namespace StardewModdingAPI.Web.Controllers
{
    /// <summary>Provides a web UI and API for parsing SMAPI log files.</summary>
    [Route("log")]
    internal class LogParserController : Controller
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Render the web UI to upload a log file.</summary>
        [HttpGet]
        public ViewResult Index()
        {
            return this.View("Index");
        }
    }
}
