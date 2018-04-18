using StardewModdingAPI.Framework.ModData;
using StardewModdingAPI.Framework.ModLoading;

namespace StardewModdingAPI.Framework
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

        /// <summary>Metadata about the mod from SMAPI's internal data (if any).</summary>
        ParsedModDataRecord DataRecord { get; }

        /// <summary>The metadata resolution status.</summary>
        ModMetadataStatus Status { get; }

        /// <summary>The reason the metadata is invalid, if any.</summary>
        string Error { get; }

        /// <summary>The mod instance (if loaded and <see cref="IsContentPack"/> is false).</summary>
        IMod Mod { get; }

        /// <summary>The content pack instance (if loaded and <see cref="IsContentPack"/> is true).</summary>
        IContentPack ContentPack { get; }

        /// <summary>Writes messages to the console and log file as this mod.</summary>
        IMonitor Monitor { get; }

        /// <summary>The mod-provided API (if any).</summary>
        object Api { get; }

        /// <summary>Whether the mod is a content pack.</summary>
        bool IsContentPack { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Set the mod status.</summary>
        /// <param name="status">The metadata resolution status.</param>
        /// <param name="error">The reason the metadata is invalid, if any.</param>
        /// <returns>Return the instance for chaining.</returns>
        IModMetadata SetStatus(ModMetadataStatus status, string error = null);

        /// <summary>Set the mod instance.</summary>
        /// <param name="mod">The mod instance to set.</param>
        IModMetadata SetMod(IMod mod);

        /// <summary>Set the mod instance.</summary>
        /// <param name="contentPack">The contentPack instance to set.</param>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        IModMetadata SetMod(IContentPack contentPack, IMonitor monitor);

        /// <summary>Set the mod-provided API instance.</summary>
        /// <param name="api">The mod-provided API.</param>
        IModMetadata SetApi(object api);

        /// <summary>Whether the mod has at least one update key set.</summary>
        bool HasUpdateKeys();
    }
}
