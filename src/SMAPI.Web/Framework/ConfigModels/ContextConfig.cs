namespace StardewModdingAPI.Web.Framework.ConfigModels
{
    /// <summary>The config settings for the app context.</summary>
    public class ContextConfig // must be public to pass into views
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The root URL for the app.</summary>
        public string RootUrl { get; set; }

        /// <summary>The root URL for the log parser.</summary>
        public string LogParserUrl { get; set; }
    }
}
