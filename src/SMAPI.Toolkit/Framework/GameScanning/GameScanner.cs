using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using StardewModdingAPI.Toolkit.Utilities;
using System.Reflection;
#if SMAPI_FOR_WINDOWS
using Microsoft.Win32;
using VdfParser;
#endif

namespace StardewModdingAPI.Toolkit.Framework.GameScanning
{
    /// <summary>Finds installed game folders.</summary>
    [SuppressMessage("ReSharper", "StringLiteralTypo", Justification = "These are valid game install paths.")]
    public class GameScanner
    {
        /*********
        ** Fields
        *********/
        /// <summary>The current OS.</summary>
        private readonly Platform Platform;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public GameScanner()
        {
            this.Platform = EnvironmentUtility.DetectPlatform();
        }

        /// <summary>Find all valid Stardew Valley install folders.</summary>
        /// <remarks>This checks default game locations, and on Windows checks the Windows registry for GOG/Steam install data. A folder is considered 'valid' if it contains the Stardew Valley executable for the current OS.</remarks>
        public IEnumerable<DirectoryInfo> Scan()
        {
            // get install paths
            IEnumerable<string> paths = this
                .GetCustomInstallPaths()
                .Concat(this.GetDefaultInstallPaths())
                .Select(path => PathUtilities.NormalizePath(path))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            // yield valid folders
            foreach (string path in paths)
            {
                DirectoryInfo folder = new(path);
                if (this.LooksLikeGameFolder(folder))
                    yield return folder;
            }
        }

        /// <summary>Get whether a folder seems to contain the game.</summary>
        /// <param name="dir">The folder to check.</param>
        public bool LooksLikeGameFolder(DirectoryInfo dir)
        {
            return this.GetGameFolderType(dir) == GameFolderType.Valid;
        }

        /// <summary>Detect the validity of a game folder based on file structure heuristics.</summary>
        /// <param name="dir">The folder to check.</param>
        public GameFolderType GetGameFolderType(DirectoryInfo dir)
        {
            // no such folder
            if (!dir.Exists)
                return GameFolderType.NoGameFound;

            // apparently valid
            if (dir.EnumerateFiles("Stardew Valley.dll").Any())
                return GameFolderType.Valid;

            // doesn't contain any version of Stardew Valley
            FileInfo executable = new(Path.Combine(dir.FullName, "Stardew Valley.exe"));
            if (!executable.Exists)
                executable = new(Path.Combine(dir.FullName, "StardewValley.exe")); // pre-1.5.5 Linux/macOS executable
            if (!executable.Exists)
                return GameFolderType.NoGameFound;

            // get assembly version
            Version? version;
            try
            {
                version = AssemblyName.GetAssemblyName(executable.FullName).Version;
                if (version == null)
                    return GameFolderType.InvalidUnknown;
            }
            catch
            {
                // The executable exists but it doesn't seem to be a valid assembly. This would
                // happen with Stardew Valley 1.5.5+, but that should have been flagged as a valid
                // folder before this point.
                return GameFolderType.InvalidUnknown;
            }

            // ignore Stardew Valley 1.5.5+ at this point
            if (version.Major == 1 && version.Minor == 3 && version.Build == 37)
                return GameFolderType.InvalidUnknown;

            // incompatible version
            if (version.Major == 1 && version.Minor < 4)
            {
                // Stardew Valley 1.5.4 and earlier have assembly versions <= 1.3.7853.31734
                if (version.Minor < 3 || version.Build <= 7853)
                    return GameFolderType.Legacy154OrEarlier;

                // Stardew Valley 1.5.5+ legacy compatibility branch
                return GameFolderType.LegacyCompatibilityBranch;
            }

            return GameFolderType.InvalidUnknown;
        }

        /*********
        ** Private methods
        *********/
        /// <summary>The default file paths where Stardew Valley can be installed.</summary>
        /// <remarks>Derived from the <a href="https://github.com/Pathoschild/Stardew.ModBuildConfig">crossplatform mod config</a>.</remarks>
        private IEnumerable<string> GetDefaultInstallPaths()
        {
            switch (this.Platform)
            {
                case Platform.Linux:
                case Platform.Mac:
                    {
                        string home = Environment.GetEnvironmentVariable("HOME")!;

                        // Linux
                        yield return $"{home}/GOG Games/Stardew Valley/game";
                        yield return Directory.Exists($"{home}/.steam/steam/steamapps/common/Stardew Valley")
                            ? $"{home}/.steam/steam/steamapps/common/Stardew Valley"
                            : $"{home}/.local/share/Steam/steamapps/common/Stardew Valley";

                        // macOS
                        yield return "/Applications/Stardew Valley.app/Contents/MacOS";
                        yield return $"{home}/Library/Application Support/Steam/steamapps/common/Stardew Valley/Contents/MacOS";
                    }
                    break;

                case Platform.Windows:
                    {
                        // Windows registry
#if SMAPI_FOR_WINDOWS
                        IDictionary<string, string> registryKeys = new Dictionary<string, string>
                        {
                            [@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 413150"] = "InstallLocation", // Steam
                            [@"SOFTWARE\WOW6432Node\GOG.com\Games\1453375253"] = "PATH", // GOG on 64-bit Windows
                        };
                        foreach (var pair in registryKeys)
                        {
                            string? path = this.GetLocalMachineRegistryValue(pair.Key, pair.Value);
                            if (!string.IsNullOrWhiteSpace(path))
                                yield return path;
                        }

                        // via Steam library path
                        string? steamPath = this.GetCurrentUserRegistryValue(@"Software\Valve\Steam", "SteamPath");
                        if (steamPath != null)
                        {
                            yield return Path.Combine(steamPath.Replace('/', '\\'), @"steamapps\common\Stardew Valley");

                            // Check for Steam libraries in other locations
                            string? path = this.GetPathFromSteamLibrary(steamPath);
                            if (!string.IsNullOrWhiteSpace(path))
                                yield return path;
                        }
#endif

                        // default GOG/Steam paths
                        foreach (string programFiles in new[] { @"C:\Program Files", @"C:\Program Files (x86)" })
                        {
                            yield return $@"{programFiles}\GalaxyClient\Games\Stardew Valley";
                            yield return $@"{programFiles}\GOG Galaxy\Games\Stardew Valley";
                            yield return $@"{programFiles}\GOG Games\Stardew Valley";
                            yield return $@"{programFiles}\Steam\steamapps\common\Stardew Valley";
                        }

                        // default Xbox app paths
                        // The Xbox app saves the install path to the registry, but we can't use it
                        // here since it saves the internal readonly path (like C:\Program Files\WindowsApps\Mutable\<package ID>)
                        // instead of the mods-enabled path(like C:\Program Files\ModifiableWindowsApps\Stardew Valley).
                        // Fortunately we can cheat a bit: players can customize the install drive, but they can't
                        // change the install path on the drive.
                        for (char driveLetter = 'C'; driveLetter <= 'H'; driveLetter++)
                            yield return $@"{driveLetter}:\Program Files\ModifiableWindowsApps\Stardew Valley";
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Unknown platform '{this.Platform}'.");
            }
        }

        /// <summary>Get the custom install path from the <c>stardewvalley.targets</c> file in the home directory, if any.</summary>
        private IEnumerable<string> GetCustomInstallPaths()
        {
            // get home path
            string homePath = Environment.GetEnvironmentVariable(this.Platform == Platform.Windows ? "USERPROFILE" : "HOME")!;
            if (string.IsNullOrWhiteSpace(homePath))
                yield break;

            // get targets file
            FileInfo file = new(Path.Combine(homePath, "stardewvalley.targets"));
            if (!file.Exists)
                yield break;

            // parse file
            XElement root;
            try
            {
                using FileStream stream = file.OpenRead();
                root = XElement.Load(stream);
            }
            catch
            {
                yield break;
            }

            // get install path
            XElement? element = root.XPathSelectElement("//*[local-name() = 'GamePath']"); // can't use '//GamePath' due to the default namespace
            if (!string.IsNullOrWhiteSpace(element?.Value))
                yield return element.Value.Trim();
        }

#if SMAPI_FOR_WINDOWS
        /// <summary>Get the value of a key in the Windows HKLM registry.</summary>
        /// <param name="key">The full path of the registry key relative to HKLM.</param>
        /// <param name="name">The name of the value.</param>
        private string? GetLocalMachineRegistryValue(string key, string name)
        {
            RegistryKey localMachine = Environment.Is64BitOperatingSystem ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) : Registry.LocalMachine;
            RegistryKey? openKey = localMachine.OpenSubKey(key);
            if (openKey == null)
                return null;
            using (openKey)
                return (string?)openKey.GetValue(name);
        }

        /// <summary>Get the value of a key in the Windows HKCU registry.</summary>
        /// <param name="key">The full path of the registry key relative to HKCU.</param>
        /// <param name="name">The name of the value.</param>
        private string? GetCurrentUserRegistryValue(string key, string name)
        {
            RegistryKey currentUser = Environment.Is64BitOperatingSystem ? RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64) : Registry.CurrentUser;
            RegistryKey? openKey = currentUser.OpenSubKey(key);
            if (openKey == null)
                return null;
            using (openKey)
                return (string?)openKey.GetValue(name);
        }

        /// <summary>Get the game directory path from alternative Steam library locations.</summary>
        /// <param name="steamPath">The full path to the directory containing steam.exe.</param>
        /// <returns>The game directory, if found.</returns>
        private string? GetPathFromSteamLibrary(string? steamPath)
        {
            string stardewAppId = "413150";
            if (steamPath != null)
            {
                string? libraryFoldersPath = Path.Combine(steamPath.Replace('/', '\\'), "steamapps\\libraryfolders.vdf");
                using FileStream fs = File.OpenRead(libraryFoldersPath);
                VdfDeserializer deserializer = new VdfDeserializer();
                SteamLibraryCollection libraries = deserializer.Deserialize<SteamLibraryCollection>(fs);
                if (libraries.libraryfolders != null)
                {
                    var stardewLibrary = libraries.libraryfolders.FirstOrDefault(f =>
                    {
                        var apps = f.Value?.apps;
                        return apps != null && apps.Any(a => a.Key.Equals(stardewAppId));
                    });
                    if (stardewLibrary.Value?.path != null)
                    {
                        return Path.Combine(stardewLibrary.Value.path.Replace("\\\\", "\\"), @"steamapps\common\Stardew Valley");
                    }
                }
            }
            return null;
        }
#endif
    }
}
