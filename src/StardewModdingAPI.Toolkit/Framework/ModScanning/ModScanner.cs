using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewModdingAPI.Toolkit.Serialisation;
using StardewModdingAPI.Toolkit.Serialisation.Models;

namespace StardewModdingAPI.Toolkit.Framework.ModScanning
{
    /// <summary>Scans folders for mod data.</summary>
    public class ModScanner
    {
        /*********
        ** Properties
        *********/
        /// <summary>The JSON helper with which to read manifests.</summary>
        private readonly JsonHelper JsonHelper;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="jsonHelper">The JSON helper with which to read manifests.</param>
        public ModScanner(JsonHelper jsonHelper)
        {
            this.JsonHelper = jsonHelper;
        }

        /// <summary>Extract information about all mods in the given folder.</summary>
        /// <param name="rootPath">The root folder containing mods.</param>
        public IEnumerable<ModFolder> GetModFolders(string rootPath)
        {
            foreach (DirectoryInfo folder in new DirectoryInfo(rootPath).EnumerateDirectories())
                yield return this.ReadFolder(rootPath, folder);
        }

        /// <summary>Extract information from a mod folder.</summary>
        /// <param name="rootPath">The root folder containing mods.</param>
        /// <param name="searchFolder">The folder to search for a mod.</param>
        public ModFolder ReadFolder(string rootPath, DirectoryInfo searchFolder)
        {
            // find manifest.json
            FileInfo manifestFile = this.FindManifest(searchFolder);
            if (manifestFile == null)
                return new ModFolder(searchFolder, null, null, "it doesn't have a manifest.");

            // read mod info
            Manifest manifest = null;
            string manifestError = null;
            {
                try
                {
                    if (!this.JsonHelper.ReadJsonFileIfExists<Manifest>(manifestFile.FullName, out manifest))
                        manifestError = "its manifest is invalid.";
                }
                catch (SParseException ex)
                {
                    manifestError = $"parsing its manifest failed: {ex.Message}";
                }
                catch (Exception ex)
                {
                    manifestError = $"parsing its manifest failed:\n{ex}";
                }
            }

            return new ModFolder(searchFolder, manifestFile.Directory, manifest, manifestError);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Find the manifest for a mod folder.</summary>
        /// <param name="folder">The folder to search.</param>
        private FileInfo FindManifest(DirectoryInfo folder)
        {
            while (true)
            {
                // check for manifest in current folder
                FileInfo file = new FileInfo(Path.Combine(folder.FullName, "manifest.json"));
                if (file.Exists)
                    return file;

                // check for single subfolder
                FileSystemInfo[] entries = folder.EnumerateFileSystemInfos().Take(2).ToArray();
                if (entries.Length == 1 && entries[0] is DirectoryInfo subfolder)
                {
                    folder = subfolder;
                    continue;
                }

                // not found
                return null;
            }
        }
    }
}
