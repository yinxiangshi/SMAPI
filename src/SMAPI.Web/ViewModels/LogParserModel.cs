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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public LogParserModel() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="sectionUrl">The root URL for the log parser controller.</param>
        /// <param name="pasteID">The paste ID.</param>
        public LogParserModel(string sectionUrl, string pasteID)
        {
            this.SectionUrl = sectionUrl;
            this.PasteID = pasteID;
        }
    }
}
