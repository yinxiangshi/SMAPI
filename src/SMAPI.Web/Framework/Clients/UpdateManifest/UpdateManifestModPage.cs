// Copyright 2022 Jamie Taylor
using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Toolkit.Framework.UpdateData;

namespace StardewModdingAPI.Web.Framework.Clients.UpdateManifest {
    /// <summary>Metadata about an update manifest "page".</summary>
    internal class UpdateManifestModPage : GenericModPage {
        /// <summary>The update manifest model.</summary>
        private UpdateManifestModel manifest;

        /// <summary>Constuct an instance.</summary>
        /// <param name="id">The "id" (i.e., URL) of this update manifest.</param>
        /// <param name="manifest">The manifest object model.</param>
        public UpdateManifestModPage(string id, UpdateManifestModel manifest) : base(ModSiteKey.UpdateManifest, id) {
            this.IsSubkeyStrict = true;
            this.manifest = manifest;
            this.SetInfo(name: id, url: id, version: null, downloads: TranslateDownloads(manifest).ToArray());
        }

        /// <summary>Return the mod name for the given subkey, if it exists in this update manifest.</summary>
        /// <param name="subkey">The subkey.</param>
        /// <returns>The mod name for the given subkey, or <see langword="null"/> if this manifest does not contain the given subkey.</returns>
        public override string? GetName(string? subkey) {
            if (subkey is null)
                return null;
            this.manifest.Subkeys.TryGetValue(subkey, out UpdateManifestModModel? modModel);
            return modModel?.Name;
        }

        /// <summary>Return the mod URL for the given subkey, if it exists in this update manifest.</summary>
        /// <param name="subkey">The subkey.</param>
        /// <returns>The mod URL for the given subkey, or <see langword="null"/> if this manifest does not contain the given subkey.</returns>
        public override string? GetUrl(string? subkey) {
            if (subkey is null)
                return null;
            this.manifest.Subkeys.TryGetValue(subkey, out UpdateManifestModModel? modModel);
            return modModel?.Url;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Translate the downloads from the manifest's object model into <see cref="IModDownload"/> objects.</summary>
        /// <param name="manifest">The manifest object model.</param>
        /// <returns>An <see cref="IModDownload"/> for each <see cref="UpdateManifestVersionModel"/> in the manifest.</returns>
        private static IEnumerable<IModDownload> TranslateDownloads(UpdateManifestModel manifest) {
            foreach (var entry in manifest.Subkeys) {
                foreach (var version in entry.Value.Versions) {
                    yield return new UpdateManifestModDownload(entry.Key, entry.Value.Name, version.Version, version.DownloadFileUrl ?? version.DownloadPageUrl);
                }
            }
        }

    }
}