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
        /// <summary>The Mods subfolder containing this mod.</summary>
        public DirectoryInfo SearchDirectory { get; }

        /// <summary>The folder containing manifest.json.</summary>
        public DirectoryInfo ActualDirectory { get; }

        /// <summary>The mod manifest.</summary>
        public Manifest Manifest { get; }

        /// <summary>The error which occurred parsing the manifest, if any.</summary>
        public string ManifestParseError { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance when a mod wasn't found in a folder.</summary>
        /// <param name="searchDirectory">The directory that was searched.</param>
        public ModFolder(DirectoryInfo searchDirectory)
        {
            this.SearchDirectory = searchDirectory;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="searchDirectory">The Mods subfolder containing this mod.</param>
        /// <param name="actualDirectory">The folder containing manifest.json.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="manifestParseError">The error which occurred parsing the manifest, if any.</param>
        public ModFolder(DirectoryInfo searchDirectory, DirectoryInfo actualDirectory, Manifest manifest, string manifestParseError = null)
        {
            this.SearchDirectory = searchDirectory;
            this.ActualDirectory = actualDirectory;
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
