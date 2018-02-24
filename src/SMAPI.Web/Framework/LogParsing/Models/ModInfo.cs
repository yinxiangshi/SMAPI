namespace StardewModdingAPI.Web.Framework.LogParsing.Models
{
    /// <summary>Metadata about a mod or content pack in the log.</summary>
    public class LogModInfo
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod name.</summary>
        public string Name { get; set; }

        /// <summary>The mod author.</summary>
        public string Author { get; set; }

        /// <summary>The mod version.</summary>
        public string Version { get; set; }

        /// <summary>The mod description.</summary>
        public string Description { get; set; }

        /// <summary>The number of errors logged by this mod.</summary>
        public int Errors { get; set; }
    }
}
