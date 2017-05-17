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
                if (manifest != null)
                {
                    string key = !string.IsNullOrWhiteSpace(manifest.UniqueID) ? manifest.UniqueID : manifest.EntryDll;
                    compatibility = (
                        from mod in compatibilityRecords
                        where
                            mod.ID.Contains(key, StringComparer.InvariantCultureIgnoreCase)
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
                        string error = $"{reasonPhrase}. Please check for a version newer than {compatibility.UpperVersionLabel ?? compatibility.UpperVersion} here:";
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
            mods = mods.ToArray();
            var sortedMods = new Stack<IModMetadata>();
            var states = mods.ToDictionary(mod => mod, mod => ModDependencyStatus.Queued);
            foreach (IModMetadata mod in mods)
                this.ProcessDependencies(mods.ToArray(), mod, states, sortedMods, new List<IModMetadata>());

            return sortedMods.Reverse();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Sort a mod's dependencies by the order they should be loaded, and remove any mods that can't be loaded due to missing or conflicting dependencies.</summary>
        /// <param name="mods">The full list of mods being validated.</param>
        /// <param name="mod">The mod whose dependencies to process.</param>
        /// <param name="states">The dependency state for each mod.</param>
        /// <param name="sortedMods">The list in which to save mods sorted by dependency order.</param>
        /// <param name="currentChain">The current change of mod dependencies.</param>
        /// <returns>Returns the mod dependency status.</returns>
        private ModDependencyStatus ProcessDependencies(IModMetadata[] mods, IModMetadata mod, IDictionary<IModMetadata, ModDependencyStatus> states, Stack<IModMetadata> sortedMods, ICollection<IModMetadata> currentChain)
        {
            // check if already visited
            switch (states[mod])
            {
                // already sorted or failed
                case ModDependencyStatus.Sorted:
                case ModDependencyStatus.Failed:
                    return states[mod];

                // dependency loop
                case ModDependencyStatus.Checking:
                    // This should never happen. The higher-level mod checks if the dependency is
                    // already being checked, so it can fail without visiting a mod twice. If this
                    // case is hit, that logic didn't catch the dependency loop for some reason.
                    throw new InvalidModStateException($"A dependency loop was not caught by the calling iteration ({string.Join(" => ", currentChain.Select(p => p.DisplayName))} => {mod.DisplayName})).");

                // not visited yet, start processing
                case ModDependencyStatus.Queued:
                    break;

                // sanity check
                default:
                    throw new InvalidModStateException($"Unknown dependency status '{states[mod]}'.");
            }

            // no dependencies, mark sorted
            if (mod.Manifest.Dependencies == null || !mod.Manifest.Dependencies.Any())
            {
                sortedMods.Push(mod);
                return states[mod] = ModDependencyStatus.Sorted;
            }

            // missing required dependencies, mark failed
            {
                string[] missingModIDs =
                    (
                        from dependency in mod.Manifest.Dependencies
                        where mods.All(m => m.Manifest.UniqueID != dependency.UniqueID)
                        orderby dependency.UniqueID
                        select dependency.UniqueID
                    )
                    .ToArray();
                if (missingModIDs.Any())
                {
                    sortedMods.Push(mod);
                    mod.SetStatus(ModMetadataStatus.Failed, $"it requires mods which aren't installed ({string.Join(", ", missingModIDs)}).");
                    return states[mod] = ModDependencyStatus.Failed;
                }
            }

            // process dependencies
            {
                states[mod] = ModDependencyStatus.Checking;

                // get mods to load first
                IModMetadata[] modsToLoadFirst =
                    (
                        from other in mods
                        where mod.Manifest.Dependencies.Any(required => required.UniqueID == other.Manifest.UniqueID)
                        select other
                    )
                    .ToArray();

                // recursively sort dependencies
                foreach (IModMetadata requiredMod in modsToLoadFirst)
                {
                    var subchain = new List<IModMetadata>(currentChain) { mod };

                    // detect dependency loop
                    if (states[requiredMod] == ModDependencyStatus.Checking)
                    {
                        sortedMods.Push(mod);
                        mod.SetStatus(ModMetadataStatus.Failed, $"its dependencies have a circular reference: {string.Join(" => ", subchain.Select(p => p.DisplayName))} => {requiredMod.DisplayName}).");
                        return states[mod] = ModDependencyStatus.Failed;
                    }

                    // recursively process each dependency
                    var substatus = this.ProcessDependencies(mods, requiredMod, states, sortedMods, subchain);
                    switch (substatus)
                    {
                        // sorted successfully
                        case ModDependencyStatus.Sorted:
                            break;

                        // failed, which means this mod can't be loaded either
                        case ModDependencyStatus.Failed:
                            sortedMods.Push(mod);
                            mod.SetStatus(ModMetadataStatus.Failed, $"it needs the '{requiredMod.DisplayName}' mod, which couldn't be loaded.");
                            return states[mod] = ModDependencyStatus.Failed;

                        // unexpected status
                        case ModDependencyStatus.Queued:
                        case ModDependencyStatus.Checking:
                            throw new InvalidModStateException($"Something went wrong sorting dependencies: mod '{requiredMod.DisplayName}' unexpectedly stayed in the '{substatus}' status.");

                        // sanity check
                        default:
                            throw new InvalidModStateException($"Unknown dependency status '{states[mod]}'.");
                    }
                }

                // all requirements sorted successfully
                sortedMods.Push(mod);
                return states[mod] = ModDependencyStatus.Sorted;
            }
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
