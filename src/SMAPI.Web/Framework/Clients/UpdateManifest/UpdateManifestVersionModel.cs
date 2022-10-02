// Copyright 2022 Jamie Taylor
using System;
namespace StardewModdingAPI.Web.Framework.Clients.UpdateManifest {
    /// <summary>Data model for a Version in an update manifest.</summary>
    internal class UpdateManifestVersionModel {
        /// <summary>The semantic version string.</summary>
        public string Version { get; }

        /// <summary>The URL for this version's download page (if any).</summary>
        public string? DownloadPageUrl { get; }

        /// <summary>The URL for this version's direct file download (if any).</summary>
        public string? DownloadFileUrl { get; }

        /// <summary>Construct an instance.</summary>
        /// <param name="version">The semantic version string.</param>
        /// <param name="downloadPageUrl">This version's download page URL (if any).</param>
        /// <param name="downloadFileUrl">This version's direct file download URL (if any).</param>
        public UpdateManifestVersionModel(string version, string? downloadPageUrl, string? downloadFileUrl) {
            this.Version = version;
            this.DownloadPageUrl = downloadPageUrl;
            this.DownloadFileUrl = downloadFileUrl;
        }
    }
}

