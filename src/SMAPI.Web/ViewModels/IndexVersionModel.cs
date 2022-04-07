#nullable disable

namespace StardewModdingAPI.Web.ViewModels
{
    /// <summary>The fields for a SMAPI version.</summary>
    public class IndexVersionModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The release version.</summary>
        public string Version { get; set; }

        /// <summary>The Markdown description for the release.</summary>
        public string Description { get; set; }

        /// <summary>The main download URL.</summary>
        public string DownloadUrl { get; set; }

        /// <summary>The for-developers download URL (not applicable for prerelease versions).</summary>
        public string DevDownloadUrl { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public IndexVersionModel() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="version">The release number.</param>
        /// <param name="description">The Markdown description for the release.</param>
        /// <param name="downloadUrl">The main download URL.</param>
        /// <param name="devDownloadUrl">The for-developers download URL (not applicable for prerelease versions).</param>
        internal IndexVersionModel(string version, string description, string downloadUrl, string devDownloadUrl)
        {
            this.Version = version;
            this.Description = description;
            this.DownloadUrl = downloadUrl;
            this.DevDownloadUrl = devDownloadUrl;
        }
    }
}
