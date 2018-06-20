namespace StardewModdingAPI.Web.Framework.ModRepositories
{
    /// <summary>Generic metadata about a mod.</summary>
    public class ModInfoModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod name.</summary>
        public string Name { get; set; }

        /// <summary>The mod's latest release number.</summary>
        public string Version { get; set; }

        /// <summary>The mod's latest optional release, if newer than <see cref="Version"/>.</summary>
        public string PreviewVersion { get; set; }

        /// <summary>The mod's web URL.</summary>
        public string Url { get; set; }

        /// <summary>The error message indicating why the mod is invalid (if applicable).</summary>
        public string Error { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an empty instance.</summary>
        public ModInfoModel()
        {
            // needed for JSON deserialising
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="name">The mod name.</param>
        /// <param name="version">The semantic version for the mod's latest release.</param>
        /// <param name="previewVersion">The semantic version for the mod's latest preview release, if available and different from <see cref="Version"/>.</param>
        /// <param name="url">The mod's web URL.</param>
        /// <param name="error">The error message indicating why the mod is invalid (if applicable).</param>
        public ModInfoModel(string name, string version, string url, string previewVersion = null, string error = null)
        {
            this.Name = name;
            this.Version = version;
            this.PreviewVersion = previewVersion;
            this.Url = url;
            this.Error = error;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="error">The error message indicating why the mod is invalid.</param>
        public ModInfoModel(string error)
        {
            this.Error = error;
        }
    }
}
