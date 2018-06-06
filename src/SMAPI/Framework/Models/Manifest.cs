using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace StardewModdingAPI.Framework.Models
{
    /// <summary>A manifest which describes a mod for SMAPI.</summary>
    internal class Manifest : IManifest
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod name.</summary>
        public string Name { get; }

        /// <summary>A brief description of the mod.</summary>
        public string Description { get; }

        /// <summary>The mod author's name.</summary>
        public string Author { get; }

        /// <summary>The mod version.</summary>
        public ISemanticVersion Version { get; }

        /// <summary>The minimum SMAPI version required by this mod, if any.</summary>
        public ISemanticVersion MinimumApiVersion { get; }

        /// <summary>The name of the DLL in the directory that has the <see cref="IMod.Entry"/> method. Mutually exclusive with <see cref="ContentPackFor"/>.</summary>
        public string EntryDll { get; }

        /// <summary>The mod which will read this as a content pack. Mutually exclusive with <see cref="IManifest.EntryDll"/>.</summary>
        public IManifestContentPackFor ContentPackFor { get; }

        /// <summary>The other mods that must be loaded before this mod.</summary>
        public IManifestDependency[] Dependencies { get; }

        /// <summary>The namespaced mod IDs to query for updates (like <c>Nexus:541</c>).</summary>
        public string[] UpdateKeys { get; set; }

        /// <summary>The unique mod ID.</summary>
        public string UniqueID { get; }

        /// <summary>Any manifest fields which didn't match a valid field.</summary>
        [JsonExtensionData]
        public IDictionary<string, object> ExtraFields { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="manifest">The toolkit manifest.</param>
        public Manifest(Toolkit.Serialisation.Models.Manifest manifest)
            : this(
                uniqueID: manifest.UniqueID,
                name: manifest.Name,
                author: manifest.Author,
                description: manifest.Description,
                version: manifest.Version != null ? new SemanticVersion(manifest.Version) : null,
                entryDll: manifest.EntryDll,
                minimumApiVersion: manifest.MinimumApiVersion != null ? new SemanticVersion(manifest.MinimumApiVersion) : null,
                contentPackFor: manifest.ContentPackFor != null ? new ManifestContentPackFor(manifest.ContentPackFor) : null,
                dependencies: manifest.Dependencies?.Select(p => p != null ? (IManifestDependency)new ManifestDependency(p) : null).ToArray(),
                updateKeys: manifest.UpdateKeys,
                extraFields: manifest.ExtraFields
            )
        { }

        /// <summary>Construct an instance for a transitional content pack.</summary>
        /// <param name="uniqueID">The unique mod ID.</param>
        /// <param name="name">The mod name.</param>
        /// <param name="author">The mod author's name.</param>
        /// <param name="description">A brief description of the mod.</param>
        /// <param name="version">The mod version.</param>
        /// <param name="entryDll">The name of the DLL in the directory that has the <see cref="IMod.Entry"/> method. Mutually exclusive with <paramref name="contentPackFor"/>.</param>
        /// <param name="minimumApiVersion">The minimum SMAPI version required by this mod, if any.</param>
        /// <param name="contentPackFor">The modID  which will read this as a content pack. Mutually exclusive with <paramref name="entryDll"/>.</param>
        /// <param name="dependencies">The other mods that must be loaded before this mod.</param>
        /// <param name="updateKeys">The namespaced mod IDs to query for updates (like <c>Nexus:541</c>).</param>
        /// <param name="extraFields">Any manifest fields which didn't match a valid field.</param>
        public Manifest(string uniqueID, string name, string author, string description, ISemanticVersion version, string entryDll = null, ISemanticVersion minimumApiVersion = null, IManifestContentPackFor contentPackFor = null, IManifestDependency[] dependencies = null, string[] updateKeys = null, IDictionary<string, object> extraFields = null)
        {
            this.Name = name;
            this.Author = author;
            this.Description = description;
            this.Version = version;
            this.UniqueID = uniqueID;
            this.UpdateKeys = new string[0];
            this.EntryDll = entryDll;
            this.ContentPackFor = contentPackFor;
            this.MinimumApiVersion = minimumApiVersion;
            this.Dependencies = dependencies ?? new IManifestDependency[0];
            this.UpdateKeys = updateKeys ?? new string[0];
            this.ExtraFields = extraFields;
        }
    }
}
