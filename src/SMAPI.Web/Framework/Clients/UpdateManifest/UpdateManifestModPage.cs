// Copyright 2022 Jamie Taylor
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Toolkit.Framework.UpdateData;

namespace StardewModdingAPI.Web.Framework.Clients.UpdateManifest
{
    /// <summary>Metadata about an update manifest "page".</summary>
    internal class UpdateManifestModPage : GenericModPage
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mods from the update manifest.</summary>
        private readonly IDictionary<string, UpdateManifestModModel> Mods;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="url">The URL of the update manifest file.</param>
        /// <param name="manifest">The parsed update manifest.</param>
        public UpdateManifestModPage(string url, UpdateManifestModel manifest)
            : base(ModSiteKey.UpdateManifest, url)
        {
            this.RequireSubkey = true;
            this.Mods = manifest.Mods ?? new Dictionary<string, UpdateManifestModModel>();
            this.SetInfo(name: url, url: url, version: null, downloads: this.ParseDownloads(manifest.Mods).ToArray());
        }

        /// <summary>Return the mod name for the given subkey, if it exists in this update manifest.</summary>
        /// <param name="subkey">The subkey.</param>
        /// <returns>The mod name for the given subkey, or <see langword="null"/> if this manifest does not contain the given subkey.</returns>
        public override string? GetName(string? subkey)
        {
            return subkey is not null && this.Mods.TryGetValue(subkey.TrimStart('@'), out UpdateManifestModModel? modModel)
                ? modModel.Name
                : null;
        }

        /// <summary>Return the mod URL for the given subkey, if it exists in this update manifest.</summary>
        /// <param name="subkey">The subkey.</param>
        /// <returns>The mod URL for the given subkey, or <see langword="null"/> if this manifest does not contain the given subkey.</returns>
        public override string? GetUrl(string? subkey)
        {
            return subkey is not null && this.Mods.TryGetValue(subkey.TrimStart('@'), out UpdateManifestModModel? modModel)
                ? modModel.Url
                : null;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Convert the raw download info from an update manifest to <see cref="IModDownload"/>.</summary>
        /// <param name="mods">The mods from the update manifest.</param>
        private IEnumerable<IModDownload> ParseDownloads(IDictionary<string, UpdateManifestModModel>? mods)
        {
            if (mods is null)
                yield break;

            foreach ((string modKey, UpdateManifestModModel mod) in mods)
            {
                if (mod.Versions is null)
                    continue;

                foreach (UpdateManifestVersionModel version in mod.Versions)
                    yield return new UpdateManifestModDownload(modKey, mod.Name ?? modKey, version.Version, version.DownloadFileUrl ?? version.DownloadPageUrl);
            }
        }

    }
}
