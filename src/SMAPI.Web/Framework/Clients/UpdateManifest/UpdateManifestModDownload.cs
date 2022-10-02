// Copyright 2022 Jamie Taylor
using System;
namespace StardewModdingAPI.Web.Framework.Clients.UpdateManifest {
    /// <summary>Metadata about a mod download in an update manifest file.</summary>
    internal class UpdateManifestModDownload : GenericModDownload {
        /// <summary>The subkey for this mod download</summary>
        private readonly string subkey;
        /// <summary>Construct an instance.</summary>
        /// <param name="subkey">The subkey for this download.</param>
        /// <param name="name">The mod name for this download.</param>
        /// <param name="version">The download's version.</param>
        /// <param name="url">The download's URL.</param>
        public UpdateManifestModDownload(string subkey, string name, string? version, string? url) : base(name, null, version, url) {
            this.subkey = subkey;
        }

        /// <summary>
        ///   Returns <see langword="true"/> iff the given subkey is the same as the subkey for this download.
        /// </summary>
        /// <param name="subkey">The subkey to match</param>
        /// <returns><see langword="true"/> if <paramref name="subkey"/> is the same as the subkey for this download, <see langword="false"/> otherwise.</returns>
        public override bool MatchesSubkey(string subkey) {
            return this.subkey == subkey;
        }
    }
}

