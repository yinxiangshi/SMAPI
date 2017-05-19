using StardewModdingAPI.Framework.Models;

namespace StardewModdingAPI.Framework.ModLoading
{
    /// <summary>Metadata for a mod.</summary>
    internal interface IModMetadata
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod's display name.</summary>
        string DisplayName { get; }

        /// <summary>The mod's full directory path.</summary>
        string DirectoryPath { get; }

        /// <summary>The mod manifest.</summary>
        IManifest Manifest { get; }

        /// <summary>Optional metadata about a mod version that SMAPI should assume is compatible or broken, regardless of whether it detects incompatible code.</summary>
        ModCompatibility Compatibility { get; }

        /// <summary>The metadata resolution status.</summary>
        ModMetadataStatus Status { get; }

        /// <summary>The reason the metadata is invalid, if any.</summary>
        string Error { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Set the mod status.</summary>
        /// <param name="status">The metadata resolution status.</param>
        /// <param name="error">The reason the metadata is invalid, if any.</param>
        /// <returns>Return the instance for chaining.</returns>
        IModMetadata SetStatus(ModMetadataStatus status, string error = null);
    }
}
