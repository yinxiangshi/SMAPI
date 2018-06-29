using System;

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

        /// <summary>The main version.</summary>
        public ModEntryVersionModel Main { get; set; }

        /// <summary>The latest optional version, if newer than <see cref="Main"/>.</summary>
        public ModEntryVersionModel Optional { get; set; }

        /// <summary>The errors that occurred while fetching update data.</summary>
        public string[] Errors { get; set; } = new string[0];

        /****
        ** Backwards-compatible fields
        ****/
        /// <summary>The mod's latest version number.</summary>
        [Obsolete("Use " + nameof(ModEntryModel.Main))]
        internal string Version { get; private set; }

        /// <summary>The mod's web URL.</summary>
        [Obsolete("Use " + nameof(ModEntryModel.Main))]
        internal string Url { get; private set; }

        /// <summary>The mod's latest optional release, if newer than <see cref="Version"/>.</summary>
        [Obsolete("Use " + nameof(ModEntryModel.Optional))]
        internal string PreviewVersion { get; private set; }

        /// <summary>The web URL to the mod's latest optional release, if newer than <see cref="Version"/>.</summary>
        [Obsolete("Use " + nameof(ModEntryModel.Optional))]
        internal string PreviewUrl { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Set backwards-compatible fields.</summary>
        /// <param name="version">The requested API version.</param>
        public void SetBackwardsCompatibility(ISemanticVersion version)
        {
            if (version.IsOlderThan("2.6-beta.19"))
            {
                this.Version = this.Main?.Version?.ToString();
                this.Url = this.Main?.Url;

                this.PreviewVersion = this.Optional?.Version?.ToString();
                this.PreviewUrl = this.Optional?.Url;
            }
        }
    }
}
