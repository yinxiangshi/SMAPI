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
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Manages deprecation warnings.</summary>
        private readonly DeprecationManager DeprecationManager;

        /// <summary>Metadata about mods that SMAPI should assume is compatible or broken, regardless of whether it detects incompatible code.</summary>
        private readonly ModCompatibility[] CompatibilityRecords;


        /*********
        ** Public methods
        *********/
        public ModResolver(IMonitor monitor, DeprecationManager deprecationManager, IEnumerable<ModCompatibility> compatibilityRecords)
        {
            this.Monitor = monitor;
            this.DeprecationManager = deprecationManager;
            this.CompatibilityRecords = compatibilityRecords.ToArray();
        }

        /// <summary>Find all mods in the given folder.</summary>
        /// <param name="rootPath">The root mod path to search.</param>
        /// <param name="jsonHelper">The JSON helper with which to read the manifest file.</param>
        /// <param name="deprecationWarnings">A list to populate with any deprecation warnings.</param>
        public ModMetadata[] FindMods(string rootPath, JsonHelper jsonHelper, IList<Action> deprecationWarnings)
        {
            this.Monitor.Log("Finding mods...");
            void LogSkip(string displayName, string reasonPhrase, LogLevel level = LogLevel.Error) => this.Monitor.Log($"Skipped {displayName} because {reasonPhrase}", level);

            // load mod metadata
            List<ModMetadata> mods = new List<ModMetadata>();
            foreach (string modRootPath in Directory.GetDirectories(rootPath))
            {
                if (this.Monitor.IsExiting)
                    return new ModMetadata[0]; // exit in progress

                // init metadata
                string displayName = modRootPath.Replace(rootPath, "").Trim('/', '\\');

                // passthrough empty directories
                DirectoryInfo directory = new DirectoryInfo(modRootPath);
                while (!directory.GetFiles().Any() && directory.GetDirectories().Length == 1)
                    directory = directory.GetDirectories().First();

                // get manifest path
                string manifestPath = Path.Combine(directory.FullName, "manifest.json");
                if (!File.Exists(manifestPath))
                {
                    LogSkip(displayName, "it doesn't have a manifest.", LogLevel.Warn);
                    continue;
                }

                // read manifest
                Manifest manifest;
                try
                {
                    // read manifest file
                    string json = File.ReadAllText(manifestPath);
                    if (string.IsNullOrEmpty(json))
                    {
                        LogSkip(displayName, "its manifest is empty.");
                        continue;
                    }

                    // parse manifest
                    manifest = jsonHelper.ReadJsonFile<Manifest>(Path.Combine(directory.FullName, "manifest.json"));
                    if (manifest == null)
                    {
                        LogSkip(displayName, "its manifest is invalid.");
                        continue;
                    }

                    // validate manifest
                    if (string.IsNullOrWhiteSpace(manifest.EntryDll))
                    {
                        LogSkip(displayName, "its manifest doesn't set an entry DLL.");
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(manifest.UniqueID))
                        deprecationWarnings.Add(() => this.Monitor.Log($"{manifest.Name} doesn't have a {nameof(IManifest.UniqueID)} in its manifest. This will be required in an upcoming SMAPI release.", LogLevel.Warn));
                }
                catch (Exception ex)
                {
                    LogSkip(displayName, $"parsing its manifest failed:\n{ex.GetLogSummary()}");
                    continue;
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

                    LogSkip(displayName, error);
                }

                // validate SMAPI version
                if (!string.IsNullOrWhiteSpace(manifest.MinimumApiVersion))
                {
                    try
                    {
                        ISemanticVersion minVersion = new SemanticVersion(manifest.MinimumApiVersion);
                        if (minVersion.IsNewerThan(Constants.ApiVersion))
                        {
                            LogSkip(displayName, $"it needs SMAPI {minVersion} or later. Please update SMAPI to the latest version to use this mod.");
                            continue;
                        }
                    }
                    catch (FormatException ex) when (ex.Message.Contains("not a valid semantic version"))
                    {
                        LogSkip(displayName, $"it has an invalid minimum SMAPI version '{manifest.MinimumApiVersion}'. This should be a semantic version number like {Constants.ApiVersion}.");
                        continue;
                    }
                }

                // create per-save directory
                if (manifest.PerSaveConfigs)
                {
                    deprecationWarnings.Add(() => this.DeprecationManager.Warn(manifest.Name, $"{nameof(Manifest)}.{nameof(Manifest.PerSaveConfigs)}", "1.0", DeprecationLevel.Info));
                    try
                    {
                        string psDir = Path.Combine(directory.FullName, "psconfigs");
                        Directory.CreateDirectory(psDir);
                        if (!Directory.Exists(psDir))
                        {
                            LogSkip(displayName, "it requires per-save configuration files ('psconfigs') which couldn't be created for some reason.");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogSkip(displayName, $"it requires per-save configuration files ('psconfigs') which couldn't be created: {ex.GetLogSummary()}");
                        continue;
                    }
                }

                // validate DLL path
                string assemblyPath = Path.Combine(directory.FullName, manifest.EntryDll);
                if (!File.Exists(assemblyPath))
                {
                    LogSkip(displayName, $"its DLL '{manifest.EntryDll}' doesn't exist.");
                    continue;
                }

                // add mod metadata
                mods.Add(new ModMetadata(displayName, directory.FullName, manifest, compatibility));
            }

            return this.HandleModDependencies(mods.ToArray());
        }


        /*********
        ** Private methods
        *********/
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

        /// <summary>Sort a set of mods by the order they should be loaded, and remove any mods that can't be loaded due to missing or conflicting dependencies.</summary>
        /// <param name="mods">The mods to process.</param>
        private ModMetadata[] HandleModDependencies(ModMetadata[] mods)
        {
            this.Monitor.Log("Checking mod dependencies...");
            var unsortedMods = mods.ToList();
            var sortedMods = new Stack<ModMetadata>();
            var visitedMods = new bool[unsortedMods.Count];
            var currentChain = new List<ModMetadata>();
            bool success = true;

            for (int index = 0; index < unsortedMods.Count; index++)
            {
                success = this.HandleModDependencies(index, visitedMods, sortedMods, currentChain, unsortedMods);
                if (!success)
                    break;
            }

            if (!success)
            {
                // Failed to sort list, return no mods.
                this.Monitor.Log("No mods will be loaded.", LogLevel.Error);
                return new ModMetadata[0];
            }

            return sortedMods.Reverse().ToArray();
        }

        /// <summary>Sort a mod's dependencies by the order they should be loaded, and remove any mods that can't be loaded due to missing or conflicting dependencies.</summary>
        /// <param name="modIndex">The index of the mod being processed in the <paramref name="unsortedMods"/>.</param>
        /// <param name="visitedMods">The mods which have been processed.</param>
        /// <param name="sortedMods">The list in which to save mods sorted by dependency order.</param>
        /// <param name="currentChain">The current change of mod dependencies.</param>
        /// <param name="unsortedMods">The mods remaining to sort.</param>
        /// <returns>Returns whether the mod can be loaded.</returns>
        private bool HandleModDependencies(int modIndex, bool[] visitedMods, Stack<ModMetadata> sortedMods, List<ModMetadata> currentChain, List<ModMetadata> unsortedMods)
        {
            // visit mod
            if (visitedMods[modIndex])
                return true; // already sorted
            ModMetadata mod = unsortedMods[modIndex];
            visitedMods[modIndex] = true;

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
                        this.Monitor.Log($"Skipped {mod.DisplayName} because it requires mods which aren't installed ({missingMods.Substring(0, missingMods.Length - 2)}).", LogLevel.Error);
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
                    this.Monitor.Log($"Skipped {mod.DisplayName} because its dependencies have a circular reference: {string.Join(" => ", currentChain.Select(p => p.DisplayName))} => {circularReferenceMod.DisplayName}).", LogLevel.Error);
                    string chain = $"{mod.Manifest.UniqueID} -> {circularReferenceMod.Manifest.UniqueID}";
                    for (int i = currentChain.Count - 1; i >= 0; i--)
                    {
                        chain = $"{currentChain[i].Manifest.UniqueID} -> " + chain;
                        if (currentChain[i].Manifest.UniqueID.Equals(mod.Manifest.UniqueID)) break;
                    }
                    this.Monitor.Log(chain, LogLevel.Error);
                    return false;
                }
                currentChain.Add(mod);

                // recursively sort dependencies
                foreach (ModMetadata requiredMod in modsToLoadFirst)
                {
                    int index = unsortedMods.IndexOf(requiredMod);
                    success = this.HandleModDependencies(index, visitedMods, sortedMods, currentChain, unsortedMods);
                    if (!success)
                        break;
                }
            }

            // mark mod sorted
            sortedMods.Push(mod);
            currentChain.Remove(mod);
            return success;
        }
    }
}
