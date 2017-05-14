using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewModdingAPI.Framework.Models;
using StardewModdingAPI.Framework.Serialisation;

namespace StardewModdingAPI.Framework.ModLoading
{
    /// <summary>Finds and processes mod metadata.</summary>
    internal class ModResolver
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get manifest metadata for each folder in the given root path.</summary>
        /// <param name="rootPath">The root path to search for mods.</param>
        /// <param name="jsonHelper">The JSON helper with which to read manifests.</param>
        /// <param name="compatibilityRecords">Metadata about mods that SMAPI should assume is compatible or broken, regardless of whether it detects incompatible code.</param>
        /// <returns>Returns the manifests by relative folder.</returns>
        public IEnumerable<IModMetadata> ReadManifests(string rootPath, JsonHelper jsonHelper, IEnumerable<ModCompatibility> compatibilityRecords)
        {
            compatibilityRecords = compatibilityRecords.ToArray();
            foreach (DirectoryInfo modDir in this.GetModFolders(rootPath))
            {
                // read file
                Manifest manifest = null;
                string path = Path.Combine(modDir.FullName, "manifest.json");
                string error = null;
                try
                {
                    // read manifest
                    manifest = jsonHelper.ReadJsonFile<Manifest>(path);

                    // validate
                    if (manifest == null)
                    {
                        error = File.Exists(path)
                            ? "its manifest is invalid."
                            : "it doesn't have a manifest.";
                    }
                    else if (string.IsNullOrWhiteSpace(manifest.EntryDll))
                        error = "its manifest doesn't set an entry DLL.";
                }
                catch (Exception ex)
                {
                    error = $"parsing its manifest failed:\n{ex.GetLogSummary()}";
                }

                // get compatibility record
                ModCompatibility compatibility = null;
                if(manifest != null)
                {
                    string key = !string.IsNullOrWhiteSpace(manifest.UniqueID) ? manifest.UniqueID : manifest.EntryDll;
                    compatibility = (
                        from mod in compatibilityRecords
                        where
                            mod.ID == key
                            && (mod.LowerSemanticVersion == null || !manifest.Version.IsOlderThan(mod.LowerSemanticVersion))
                            && !manifest.Version.IsNewerThan(mod.UpperSemanticVersion)
                        select mod
                    ).FirstOrDefault();
                }
                // build metadata
                string displayName = !string.IsNullOrWhiteSpace(manifest?.Name)
                    ? manifest.Name
                    : modDir.FullName.Replace(rootPath, "").Trim('/', '\\');
                ModMetadataStatus status = error == null
                    ? ModMetadataStatus.Found
                    : ModMetadataStatus.Failed;

                yield return new ModMetadata(displayName, modDir.FullName, manifest, compatibility).SetStatus(status, error);
            }
        }

        /// <summary>Validate manifest metadata.</summary>
        /// <param name="mods">The mod manifests to validate.</param>
        /// <param name="apiVersion">The current SMAPI version.</param>
        public void ValidateManifests(IEnumerable<IModMetadata> mods, ISemanticVersion apiVersion)
        {
            foreach (IModMetadata mod in mods)
            {
                // skip if already failed
                if (mod.Status == ModMetadataStatus.Failed)
                    continue;

                // validate compatibility
                {
                    ModCompatibility compatibility = mod.Compatibility;
                    if (compatibility?.Compatibility == ModCompatibilityType.AssumeBroken)
                    {
                        bool hasOfficialUrl = !string.IsNullOrWhiteSpace(mod.Compatibility.UpdateUrl);
                        bool hasUnofficialUrl = !string.IsNullOrWhiteSpace(mod.Compatibility.UnofficialUpdateUrl);

                        string reasonPhrase = compatibility.ReasonPhrase ?? "it's not compatible with the latest version of the game";
                        string error = $"{reasonPhrase}. Please check for a version newer than {compatibility.UpperVersion} here:";
                        if (hasOfficialUrl)
                            error += !hasUnofficialUrl ? $" {compatibility.UpdateUrl}" : $"{Environment.NewLine}- official mod: {compatibility.UpdateUrl}";
                        if (hasUnofficialUrl)
                            error += $"{Environment.NewLine}- unofficial update: {compatibility.UnofficialUpdateUrl}";

                        mod.SetStatus(ModMetadataStatus.Failed, error);
                        continue;
                    }
                }

                // validate SMAPI version
                if (!string.IsNullOrWhiteSpace(mod.Manifest.MinimumApiVersion))
                {
                    if (!SemanticVersion.TryParse(mod.Manifest.MinimumApiVersion, out ISemanticVersion minVersion))
                    {
                        mod.SetStatus(ModMetadataStatus.Failed, $"it has an invalid minimum SMAPI version '{mod.Manifest.MinimumApiVersion}'. This should be a semantic version number like {apiVersion}.");
                        continue;
                    }
                    if (minVersion.IsNewerThan(apiVersion))
                    {
                        mod.SetStatus(ModMetadataStatus.Failed, $"it needs SMAPI {minVersion} or later. Please update SMAPI to the latest version to use this mod.");
                        continue;
                    }
                }

                // validate DLL path
                string assemblyPath = Path.Combine(mod.DirectoryPath, mod.Manifest.EntryDll);
                if (!File.Exists(assemblyPath))
                    mod.SetStatus(ModMetadataStatus.Failed, $"its DLL '{mod.Manifest.EntryDll}' doesn't exist.");
            }
        }

        /// <summary>Sort the given mods by the order they should be loaded.</summary>
        /// <param name="mods">The mods to process.</param>
        public IEnumerable<IModMetadata> ProcessDependencies(IEnumerable<IModMetadata> mods)
        {
            var unsortedMods = mods.ToList();
            var sortedMods = new Stack<IModMetadata>();
            var visitedMods = new bool[unsortedMods.Count];
            var currentChain = new List<IModMetadata>();
            bool success = true;

            for (int index = 0; index < unsortedMods.Count; index++)
            {
                if (unsortedMods[index].Status == ModMetadataStatus.Failed)
                    continue;

                success = this.ProcessDependencies(index, visitedMods, sortedMods, currentChain, unsortedMods);
                if (!success)
                    break;
            }

            if (!success)
                return new ModMetadata[0];

            return sortedMods.Reverse().ToArray();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Sort a mod's dependencies by the order they should be loaded, and remove any mods that can't be loaded due to missing or conflicting dependencies.</summary>
        /// <param name="modIndex">The index of the mod being processed in the <paramref name="unsortedMods"/>.</param>
        /// <param name="visitedMods">The mods which have been processed.</param>
        /// <param name="sortedMods">The list in which to save mods sorted by dependency order.</param>
        /// <param name="currentChain">The current change of mod dependencies.</param>
        /// <param name="unsortedMods">The mods remaining to sort.</param>
        /// <returns>Returns whether the mod can be loaded.</returns>
        private bool ProcessDependencies(int modIndex, bool[] visitedMods, Stack<IModMetadata> sortedMods, List<IModMetadata> currentChain, List<IModMetadata> unsortedMods)
        {
            // visit mod
            if (visitedMods[modIndex])
                return true; // already sorted
            visitedMods[modIndex] = true;

            // mod already failed
            IModMetadata mod = unsortedMods[modIndex];
            if (mod.Status == ModMetadataStatus.Failed)
                return false;

            // process dependencies
            bool success = true;
            if (mod.Manifest.Dependencies != null && mod.Manifest.Dependencies.Any())
            {
                // validate required dependencies are present
                {
                    string missingMods = null;
                    foreach (IManifestDependency dependency in mod.Manifest.Dependencies)
                    {
                        if (!unsortedMods.Any(m => m.Manifest.UniqueID.Equals(dependency.UniqueID)))
                            missingMods += $"{dependency.UniqueID}, ";
                    }
                    if (missingMods != null)
                    {
                        mod.SetStatus(ModMetadataStatus.Failed, $"it requires mods which aren't installed ({missingMods.Substring(0, missingMods.Length - 2)}).");
                        return false;
                    }
                }

                // get mods which should be loaded before this one
                IModMetadata[] modsToLoadFirst =
                    (
                        from unsorted in unsortedMods
                        where mod.Manifest.Dependencies.Any(required => required.UniqueID == unsorted.Manifest.UniqueID)
                        select unsorted
                    )
                    .ToArray();

                // detect circular references
                IModMetadata circularReferenceMod = currentChain.FirstOrDefault(modsToLoadFirst.Contains);
                if (circularReferenceMod != null)
                {
                    mod.SetStatus(ModMetadataStatus.Failed, $"its dependencies have a circular reference: {string.Join(" => ", currentChain.Select(p => p.DisplayName))} => {circularReferenceMod.DisplayName}).");
                    return false;
                }
                currentChain.Add(mod);

                // recursively sort dependencies
                foreach (IModMetadata requiredMod in modsToLoadFirst)
                {
                    int index = unsortedMods.IndexOf(requiredMod);
                    success = this.ProcessDependencies(index, visitedMods, sortedMods, currentChain, unsortedMods);
                    if (!success)
                        break;
                }
            }

            // mark mod sorted
            sortedMods.Push(mod);
            currentChain.Remove(mod);
            return success;
        }

        /// <summary>Get all mod folders in a root folder, passing through empty folders as needed.</summary>
        /// <param name="rootPath">The root folder path to search.</param>
        private IEnumerable<DirectoryInfo> GetModFolders(string rootPath)
        {
            foreach (string modRootPath in Directory.GetDirectories(rootPath))
            {
                DirectoryInfo directory = new DirectoryInfo(modRootPath);

                // if a folder only contains another folder, check the inner folder instead
                while (!directory.GetFiles().Any() && directory.GetDirectories().Length == 1)
                    directory = directory.GetDirectories().First();

                yield return directory;
            }
        }
    }
}
