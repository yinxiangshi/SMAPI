namespace StardewModdingAPI.Web.ViewModels
{
    /// <summary>The view model for the index page.</summary>
    public class IndexModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The latest SMAPI version.</summary>
        public string LatestVersion { get; set; }

        /// <summary>The Markdown description for the release.</summary>
        public string Description { get; set; }

        /// <summary>The main download URL.</summary>
        public string DownloadUrl { get; set; }

        /// <summary>The for-developers download URL.</summary>
        public string DevDownloadUrl { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public IndexModel() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="latestVersion">The latest SMAPI version.</param>
        /// <param name="description">The Markdown description for the release.</param>
        /// <param name="downloadUrl">The main download URL.</param>
        /// <param name="devDownloadUrl">The for-developers download URL.</param>
        internal IndexModel(string latestVersion, string description, string downloadUrl, string devDownloadUrl)
        {
            this.LatestVersion = latestVersion;
            this.Description = description;
            this.DownloadUrl = downloadUrl;
            this.DevDownloadUrl = devDownloadUrl;
        }
    }
}
