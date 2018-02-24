using StardewModdingAPI.Web.Framework.LogParsing.Models;

namespace StardewModdingAPI.Web.ViewModels
{
    /// <summary>The view model for the log parser page.</summary>
    public class LogParserModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The root URL for the log parser controller.</summary>
        public string SectionUrl { get; set; }

        /// <summary>The paste ID.</summary>
        public string PasteID { get; set; }

        /// <summary>The parsed log info.</summary>
        public ParsedLog ParsedLog { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public LogParserModel() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="sectionUrl">The root URL for the log parser controller.</param>
        /// <param name="pasteID">The paste ID.</param>
        /// <param name="parsedLog">The parsed log info.</param>
        public LogParserModel(string sectionUrl, string pasteID, ParsedLog parsedLog)
        {
            this.SectionUrl = sectionUrl;
            this.PasteID = pasteID;
            this.ParsedLog = parsedLog;
        }
    }
}
