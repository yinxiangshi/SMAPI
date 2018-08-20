using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewModdingAPI.Toolkit.Serialisation.Models;

namespace StardewModdingAPI.Toolkit.Framework.ModScanning
{
    /// <summary>The info about a mod read from its folder.</summary>
    public class ModFolder
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The folder containing the mod's manifest.json.</summary>
        public DirectoryInfo Directory { get; }

        /// <summary>The mod manifest.</summary>
        public Manifest Manifest { get; }

        /// <summary>The error which occurred parsing the manifest, if any.</summary>
        public string ManifestParseError { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="directory">The folder containing the mod's manifest.json.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="manifestParseError">The error which occurred parsing the manifest, if any.</param>
        public ModFolder(DirectoryInfo directory, Manifest manifest, string manifestParseError = null)
        {
            this.Directory = directory;
            this.Manifest = manifest;
            this.ManifestParseError = manifestParseError;
        }

        /// <summary>Get the update keys for a mod.</summary>
        /// <param name="manifest">The mod manifest.</param>
        public IEnumerable<string> GetUpdateKeys(Manifest manifest)
        {
            return
                (manifest.UpdateKeys ?? new string[0])
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();
        }
    }
}
