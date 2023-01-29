// Copyright 2022 Jamie Taylor
namespace StardewModdingAPI.Web.Framework.Clients.UpdateManifest.ResponseModels
{
    /// <summary>The data model for a mod in an update manifest file.</summary>
    internal class UpdateManifestModModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod's name.</summary>
        public string? Name { get; }

        /// <summary>The mod page URL from which to download updates.</summary>
        public string? Url { get; }

        /// <summary>The available versions for this mod.</summary>
        public UpdateManifestVersionModel[]? Versions { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The mod's name.</param>
        /// <param name="url">The mod page URL from which to download updates.</param>
        /// <param name="versions">The available versions for this mod.</param>
        public UpdateManifestModModel(string? name, string? url, UpdateManifestVersionModel[]? versions)
        {
            this.Name = name;
            this.Url = url;
            this.Versions = versions;
        }
    }
}
