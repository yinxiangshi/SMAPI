namespace StardewModdingAPI.Toolkit.Framework.Clients.WebApi
{
    /// <summary>Metadata about a mod.</summary>
    public class ModEntryModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod's unique ID (if known).</summary>
        public string ID { get; set; }

        /// <summary>The mod name.</summary>
        public string Name { get; set; }

        /// <summary>The mod's latest release number.</summary>
        public string Version { get; set; }

        /// <summary>The mod's web URL.</summary>
        public string Url { get; set; }

        /// <summary>The mod's latest optional release, if newer than <see cref="Version"/>.</summary>
        public string PreviewVersion { get; set; }

        /// <summary>The web URL to the mod's latest optional release, if newer than <see cref="Version"/>.</summary>
        public string PreviewUrl { get; set; }

        /// <summary>The errors that occurred while fetching update data.</summary>
        public string[] Errors { get; set; } = new string[0];
    }
}
