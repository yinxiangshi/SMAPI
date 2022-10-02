// Copyright 2022 Jamie Taylor
using System;
using System.Collections.Generic;

namespace StardewModdingAPI.Web.Framework.Clients.UpdateManifest {
    /// <summary>Data model for an update manifest.</summary>
    internal class UpdateManifestModel {
        /// <summary>The manifest format version.</summary>
        public string ManifestVersion { get; }

        /// <summary>The subkeys in this update manifest.</summary>
        public IDictionary<string, UpdateManifestModModel> Subkeys { get; }

        /// <summary>Construct an instance.</summary>
        /// <param name="manifestVersion">The manifest format version.</param>
        /// <param name="subkeys">The subkeys in this update manifest.</param>
        public UpdateManifestModel(string manifestVersion, IDictionary<string, UpdateManifestModModel> subkeys) {
            this.ManifestVersion = manifestVersion;
            this.Subkeys = subkeys;
        }
    }
}

