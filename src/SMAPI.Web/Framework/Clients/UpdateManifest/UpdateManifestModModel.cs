// Copyright 2022 Jamie Taylor
using System;
namespace StardewModdingAPI.Web.Framework.Clients.UpdateManifest {
    /// <summary>Data model for a mod in an update manifest.</summary>
    internal class UpdateManifestModModel {
        /// <summary>The mod's name.</summary>
        public string Name { get; }

        /// <summary>The mod's URL.</summary>
        public string? Url { get; }

        /// <summary>The versions for this mod.</summary>
        public UpdateManifestVersionModel[] Versions { get; }

        /// <summary>Construct an instance.</summary>
        /// <param name="name">The mod's name.</param>
        /// <param name="url">The mod's URL.</param>
        /// <param name="versions">The versions for this mod.</param>
        public UpdateManifestModModel(string name, string? url, UpdateManifestVersionModel[] versions) {
            this.Name = name;
            this.Url = url;
            this.Versions = versions;
        }
    }
}

