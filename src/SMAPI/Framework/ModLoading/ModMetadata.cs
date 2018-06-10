using System;
using System.Linq;
using StardewModdingAPI.Framework.ModData;

namespace StardewModdingAPI.Framework.ModLoading
{
    /// <summary>Metadata for a mod.</summary>
    internal class ModMetadata : IModMetadata
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

        /// <summary>Metadata about the mod from SMAPI's internal data (if any).</summary>
        public ParsedModDataRecord DataRecord { get; }

        /// <summary>The metadata resolution status.</summary>
        public ModMetadataStatus Status { get; private set; }

        /// <summary>Indicates non-error issues with the mod.</summary>
        public ModWarning Warnings { get; private set; }

        /// <summary>The reason the metadata is invalid, if any.</summary>
        public string Error { get; private set; }

        /// <summary>The mod instance (if loaded and <see cref="IsContentPack"/> is false).</summary>
        public IMod Mod { get; private set; }

        /// <summary>The content pack instance (if loaded and <see cref="IsContentPack"/> is true).</summary>
        public IContentPack ContentPack { get; private set; }

        /// <summary>Writes messages to the console and log file as this mod.</summary>
        public IMonitor Monitor { get; private set; }

        /// <summary>The mod-provided API (if any).</summary>
        public object Api { get; private set; }

        /// <summary>The latest version of the mod.</summary>
        public ISemanticVersion LatestVersion { get; private set; }

        /// <summary>The latest preview version of the mod, if any.</summary>
        public ISemanticVersion LatestPreviewVersion { get; private set; }

        /// <summary>The error checking for updates for this mod, if any.</summary>
        public string UpdateCheckError { get; private set; }

        /// <summary>Whether the mod is a content pack.</summary>
        public bool IsContentPack => this.Manifest?.ContentPackFor != null;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="displayName">The mod's display name.</param>
        /// <param name="directoryPath">The mod's full directory path.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="dataRecord">Metadata about the mod from SMAPI's internal data (if any).</param>
        public ModMetadata(string displayName, string directoryPath, IManifest manifest, ParsedModDataRecord dataRecord)
        {
            this.DisplayName = displayName;
            this.DirectoryPath = directoryPath;
            this.Manifest = manifest;
            this.DataRecord = dataRecord;
        }

        /// <summary>Set the mod status.</summary>
        /// <param name="status">The metadata resolution status.</param>
        /// <param name="error">The reason the metadata is invalid, if any.</param>
        /// <returns>Return the instance for chaining.</returns>
        public IModMetadata SetStatus(ModMetadataStatus status, string error = null)
        {
            this.Status = status;
            this.Error = error;
            return this;
        }

        /// <summary>Set a warning flag for the mod.</summary>
        /// <param name="warning">The warning to set.</param>
        public IModMetadata SetWarning(ModWarning warning)
        {
            this.Warnings |= warning;
            return this;
        }

        /// <summary>Set the mod instance.</summary>
        /// <param name="mod">The mod instance to set.</param>
        public IModMetadata SetMod(IMod mod)
        {
            if (this.ContentPack != null)
                throw new InvalidOperationException("A mod can't be both an assembly mod and content pack.");

            this.Mod = mod;
            this.Monitor = mod.Monitor;
            return this;
        }

        /// <summary>Set the mod instance.</summary>
        /// <param name="contentPack">The contentPack instance to set.</param>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        public IModMetadata SetMod(IContentPack contentPack, IMonitor monitor)
        {
            if (this.Mod != null)
                throw new InvalidOperationException("A mod can't be both an assembly mod and content pack.");

            this.ContentPack = contentPack;
            this.Monitor = monitor;
            return this;
        }

        /// <summary>Set the mod-provided API instance.</summary>
        /// <param name="api">The mod-provided API.</param>
        public IModMetadata SetApi(object api)
        {
            this.Api = api;
            return this;
        }

        /// <summary>Set the update version.</summary>
        /// <param name="latestVersion">The latest version.</param>
        public IModMetadata SetUpdateVersion(ISemanticVersion latestVersion)
        {
            this.LatestVersion = latestVersion;
            return this;
        }

        /// <summary>Set the preview update version.</summary>
        /// <param name="latestPreviewVersion">The latest preview version.</param>
        public IModMetadata SetPreviewUpdateVersion(ISemanticVersion latestPreviewVersion)
        {
            this.LatestPreviewVersion = latestPreviewVersion;
            return this;
        }

        /// <summary>Set the error that occured while checking for updates.</summary>
        /// <param name="updateCheckError">The error checking for updates.</param>
        public IModMetadata SetUpdateError(string updateCheckError)
        {
            this.UpdateCheckError = updateCheckError;
            return this;
        }

        /// <summary>Whether the mod manifest was loaded (regardless of whether the mod itself was loaded).</summary>
        public bool HasManifest()
        {
            return this.Manifest != null;
        }

        /// <summary>Whether the mod has at least one update key set.</summary>
        public bool HasUpdateKeys()
        {
            return
                this.HasManifest()
                && this.Manifest.UpdateKeys != null
                && this.Manifest.UpdateKeys.Any(key => !string.IsNullOrWhiteSpace(key));
        }
    }
}
