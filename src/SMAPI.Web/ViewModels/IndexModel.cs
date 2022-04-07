#nullable disable

namespace StardewModdingAPI.Web.ViewModels
{
    /// <summary>The view model for the index page.</summary>
    public class IndexModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The latest stable SMAPI version.</summary>
        public IndexVersionModel StableVersion { get; set; }

        /// <summary>A message to show below the download button (e.g. for details on downloading a beta version), in Markdown format.</summary>
        public string OtherBlurb { get; set; }

        /// <summary>A list of supports to credit on the main page, in Markdown format.</summary>
        public string SupporterList { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public IndexModel() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="stableVersion">The latest stable SMAPI version.</param>
        /// <param name="otherBlurb">A message to show below the download button (e.g. for details on downloading a beta version), in Markdown format.</param>
        /// <param name="supporterList">A list of supports to credit on the main page, in Markdown format.</param>
        internal IndexModel(IndexVersionModel stableVersion, string otherBlurb, string supporterList)
        {
            this.StableVersion = stableVersion;
            this.OtherBlurb = otherBlurb;
            this.SupporterList = supporterList;
        }
    }
}
