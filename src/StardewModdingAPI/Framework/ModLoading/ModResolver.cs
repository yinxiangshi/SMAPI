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
        ** Properties
        *********/
        /// <summary>Metadata about mods that SMAPI should assume is compatible or broken, regardless of whether it detects incompatible code.</summary>
        private readonly ModCompatibility[] CompatibilityRecords;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="compatibilityRecords">Metadata about mods that SMAPI should assume is compatible or broken, regardless of whether it detects incompatible code.</param>
        public ModResolver(IEnumerable<ModCompatibility> compatibilityRecords)
        {
            this.CompatibilityRecords = compatibilityRecords.ToArray();
        }

        /// <summary>Read mod metadata from the given folder in dependency order.</summary>
        /// <param name="rootPath">The root path to search for mods.</param>
        /// <param name="jsonHelper">The JSON helper with which to read manifests.</param>
        public IEnumerable<ModMetadata> GetMods(string rootPath, JsonHelper jsonHelper)
        {
            ModMetadata[] mods = this.GetDataFromFolder(rootPath, jsonHelper).ToArray();
            mods = this.ProcessDependencies(mods.ToArray());
            return mods;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Find all mods in the given folder.</summary>
        /// <param name="rootPath">The root mod path to search.</param>
        /// <param name="jsonHelper">The JSON helper with which to read the manifest file.</param>
        private IEnumerable<ModMetadata> GetDataFromFolder(string rootPath, JsonHelper jsonHelper)
        {
            // load mod metadata
            foreach (DirectoryInfo modDir in this.GetModFolders(rootPath))
            {
                string displayName = modDir.FullName.Replace(rootPath, "").Trim('/', '\\');

                // read manifest
                Manifest manifest;
                {
                    string manifestPath = Path.Combine(modDir.FullName, "manifest.json");
                    if (!this.TryReadManifest(manifestPath, jsonHelper, out manifest, out string error))
                        yield return new ModMetadata(displayName, modDir.FullName, null, null, ModMetadataStatus.Failed, error);
                }
                if (!string.IsNullOrWhiteSpace(manifest.Name))
                    displayName = manifest.Name;

                // validate compatibility
                ModCompatibility compatibility = this.GetCompatibilityRecord(manifest);
                if (compatibility?.Compatibility == ModCompatibilityType.AssumeBroken)
                {
                    bool hasOfficialUrl = !string.IsNullOrWhiteSpace(compatibility.UpdateUrl);
                    bool hasUnofficialUrl = !string.IsNullOrWhiteSpace(compatibility.UnofficialUpdateUrl);

                    string reasonPhrase = compatibility.ReasonPhrase ?? "it's not compatible with the latest version of the game";
                    string error = $"{reasonPhrase}. Please check for a version newer than {compatibility.UpperVersion} here:";
                    if (hasOfficialUrl)
                        error += !hasUnofficialUrl ? $" {compatibility.UpdateUrl}" : $"{Environment.NewLine}- official mod: {compatibility.UpdateUrl}";
                    if (hasUnofficialUrl)
                        error += $"{Environment.NewLine}- unofficial update: {compatibility.UnofficialUpdateUrl}";

                    yield return new ModMetadata(displayName, modDir.FullName, manifest, compatibility, ModMetadataStatus.Failed, error);
                }

                // validate SMAPI version
                if (!string.IsNullOrWhiteSpace(manifest.MinimumApiVersion))
                {
                    if (!SemanticVersion.TryParse(manifest.MinimumApiVersion, out ISemanticVersion minVersion))
                        yield return new ModMetadata(displayName, modDir.FullName, manifest, compatibility, ModMetadataStatus.Failed, $"it has an invalid minimum SMAPI version '{manifest.MinimumApiVersion}'. This should be a semantic version number like {Constants.ApiVersion}.");
                    if (minVersion.IsNewerThan(Constants.ApiVersion))
                        yield return new ModMetadata(displayName, modDir.FullName, manifest, compatibility, ModMetadataStatus.Failed, $"it needs SMAPI {minVersion} or later. Please update SMAPI to the latest version to use this mod.");
                }

                // validate DLL path
                string assemblyPath = Path.Combine(modDir.FullName, manifest.EntryDll);
                if (!File.Exists(assemblyPath))
                {
                    yield return new ModMetadata(displayName, modDir.FullName, manifest, compatibility, ModMetadataStatus.Failed, $"its DLL '{manifest.EntryDll}' doesn't exist.");
                    continue;
                }

                // add mod metadata
                yield return new ModMetadata(displayName, modDir.FullName, manifest, compatibility);
            }
        }

        /// <summary>Sort a set of mods by the order they should be loaded, and remove any mods that can't be loaded due to missing or conflicting dependencies.</summary>
        /// <param name="mods">The mods to process.</param>
        private ModMetadata[] ProcessDependencies(ModMetadata[] mods)
        {
            var unsortedMods = mods.ToList();
            var sortedMods = new Stack<ModMetadata>();
            var visitedMods = new bool[unsortedMods.Count];
            var currentChain = new List<ModMetadata>();
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

        /// <summary>Sort a mod's dependencies by the order they should be loaded, and remove any mods that can't be loaded due to missing or conflicting dependencies.</summary>
        /// <param name="modIndex">The index of the mod being processed in the <paramref name="unsortedMods"/>.</param>
        /// <param name="visitedMods">The mods which have been processed.</param>
        /// <param name="sortedMods">The list in which to save mods sorted by dependency order.</param>
        /// <param name="currentChain">The current change of mod dependencies.</param>
        /// <param name="unsortedMods">The mods remaining to sort.</param>
        /// <returns>Returns whether the mod can be loaded.</returns>
        private bool ProcessDependencies(int modIndex, bool[] visitedMods, Stack<ModMetadata> sortedMods, List<ModMetadata> currentChain, List<ModMetadata> unsortedMods)
        {
            // visit mod
            if (visitedMods[modIndex])
                return true; // already sorted
            visitedMods[modIndex] = true;

            // mod already failed
            ModMetadata mod = unsortedMods[modIndex];
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
                        mod.Status = ModMetadataStatus.Failed;
                        mod.Error = $"it requires mods which aren't installed ({missingMods.Substring(0, missingMods.Length - 2)}).";
                        return false;
                    }
                }

                // get mods which should be loaded before this one
                ModMetadata[] modsToLoadFirst =
                    (
                        from unsorted in unsortedMods
                        where mod.Manifest.Dependencies.Any(required => required.UniqueID == unsorted.Manifest.UniqueID)
                        select unsorted
                    )
                    .ToArray();

                // detect circular references
                ModMetadata circularReferenceMod = currentChain.FirstOrDefault(modsToLoadFirst.Contains);
                if (circularReferenceMod != null)
                {
                    mod.Status = ModMetadataStatus.Failed;
                    mod.Error = $"its dependencies have a circular reference: {string.Join(" => ", currentChain.Select(p => p.DisplayName))} => {circularReferenceMod.DisplayName}).";
                    return false;
                }
                currentChain.Add(mod);

                // recursively sort dependencies
                foreach (ModMetadata requiredMod in modsToLoadFirst)
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

        /// <summary>Read a manifest file if it's valid, else set a relevant error phrase.</summary>
        /// <param name="path">The absolute path to the manifest file.</param>
        /// <param name="jsonHelper">The JSON helper with which to read the manifest file.</param>
        /// <param name="manifest">The loaded manifest, if reading succeeded.</param>
        /// <param name="errorPhrase">The read error, if reading failed.</param>
        /// <returns>Returns whether the manifest was read successfully.</returns>
        private bool TryReadManifest(string path, JsonHelper jsonHelper, out Manifest manifest, out string errorPhrase)
        {
            try
            {
                // validate path
                if (!File.Exists(path))
                {
                    manifest = null;
                    errorPhrase = "it doesn't have a manifest.";
                    return false;
                }

                // parse manifest
                manifest = jsonHelper.ReadJsonFile<Manifest>(path);
                if (manifest == null)
                {
                    errorPhrase = "its manifest is invalid.";
                    return false;
                }

                // validate manifest
                if (string.IsNullOrWhiteSpace(manifest.EntryDll))
                {
                    errorPhrase = "its manifest doesn't set an entry DLL.";
                    return false;
                }

                errorPhrase = null;
                return true;
            }
            catch (Exception ex)
            {
                manifest = null;
                errorPhrase = $"parsing its manifest failed:\n{ex.GetLogSummary()}";
                return false;
            }
        }

        /// <summary>Get metadata that indicates whether SMAPI should assume the mod is compatible or broken, regardless of whether it detects incompatible code.</summary>
        /// <param name="manifest">The mod manifest.</param>
        /// <returns>Returns the incompatibility record if applicable, else <c>null</c>.</returns>
        private ModCompatibility GetCompatibilityRecord(IManifest manifest)
        {
            string key = !string.IsNullOrWhiteSpace(manifest.UniqueID) ? manifest.UniqueID : manifest.EntryDll;
            return (
                from mod in this.CompatibilityRecords
                where
                mod.ID == key
                && (mod.LowerSemanticVersion == null || !manifest.Version.IsOlderThan(mod.LowerSemanticVersion))
                && !manifest.Version.IsNewerThan(mod.UpperSemanticVersion)
                select mod
            ).FirstOrDefault();
        }
    }
}
