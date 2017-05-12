using StardewModdingAPI.Framework.Models;

namespace StardewModdingAPI.Framework
{
    /// <summary>Metadata for a mod.</summary>
    internal class ModMetadata
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod's display name.</summary>
        public string DisplayName { get; }

        /// <summary>The mod's full directory path.</summary>
        public string DirectoryPath { get; }

        /// <summary>The mod manifest.</summary>
        public IManifest Manifest { get; }

        /// <summary>Optional metadata about a mod version that SMAPI should assume is compatible or broken, regardless of whether it detects incompatible code.</summary>
        public ModCompatibility Compatibility { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="displayName">The mod's display name.</param>
        /// <param name="directoryPath">The mod's full directory path.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="compatibility">Optional metadata about a mod version that SMAPI should assume is compatible or broken, regardless of whether it detects incompatible code.</param>
        public ModMetadata(string displayName, string directoryPath, IManifest manifest, ModCompatibility compatibility)
        {
            this.DisplayName = displayName;
            this.DirectoryPath = directoryPath;
            this.Manifest = manifest;
            this.Compatibility = compatibility;
        }
    }
}
