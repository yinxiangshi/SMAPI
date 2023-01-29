// Copyright 2022 Jamie Taylor
using System.Collections.Generic;

namespace StardewModdingAPI.Web.Framework.Clients.UpdateManifest
{
    /// <summary>The data model for an update manifest file.</summary>
    internal class UpdateManifestModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The manifest format version.</summary>
        public string? ManifestVersion { get; }

        /// <summary>The mod info in this update manifest.</summary>
        public IDictionary<string, UpdateManifestModModel>? Mods { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="manifestVersion">The manifest format version.</param>
        /// <param name="mods">The mod info in this update manifest.</param>
        public UpdateManifestModel(string manifestVersion, IDictionary<string, UpdateManifestModModel> mods)
        {
            this.ManifestVersion = manifestVersion;
            this.Mods = mods;
        }
    }
}
