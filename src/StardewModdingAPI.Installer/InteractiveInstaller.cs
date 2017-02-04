using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#if SMAPI_FOR_WINDOWS
using Microsoft.Win32;
#endif
using StardewModdingApi.Installer.Enums;

namespace StardewModdingApi.Installer
{
    /// <summary>Interactively performs the install and uninstall logic.</summary>
    internal class InteractiveInstaller
    {
        /*********
        ** Properties
        *********/
        /// <summary>The default file paths where Stardew Valley can be installed.</summary>
        /// <remarks>Derived from the crossplatform mod config: https://github.com/Pathoschild/Stardew.ModBuildConfig. </remarks>
        private IEnumerable<string> DefaultInstallPaths
        {
            get
            {
                // Linux
                yield return $"{Environment.GetEnvironmentVariable("HOME")}/GOG Games/Stardew Valley/game";
                yield return $"{Environment.GetEnvironmentVariable("HOME")}/.local/share/Steam/steamapps/common/Stardew Valley";

                // Mac
                yield return "/Applications/Stardew Valley.app/Contents/MacOS";
                yield return $"{Environment.GetEnvironmentVariable("HOME")}/Library/Application Support/Steam/steamapps/common/Stardew Valley/Contents/MacOS";

                // Windows
                yield return @"C:\Program Files (x86)\GalaxyClient\Games\Stardew Valley";
                yield return @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley";

                // Windows registry
#if SMAPI_FOR_WINDOWS
                IDictionary<string, string> registryKeys = new Dictionary<string, string>
                {
                    [@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 413150"] = "InstallLocation", // Steam
                    [@"SOFTWARE\WOW6432Node\GOG.com\Games\1453375253"] = "PATH", // GOG on 64-bit Windows
                };
                foreach (var pair in registryKeys)
                {
                    string path = this.GetLocalMachineRegistryValue(pair.Key, pair.Value);
                    if (!string.IsNullOrWhiteSpace(path))
                        yield return path;
                }
#endif
            }
        }

        /// <summary>The directory or file paths to remove when uninstalling SMAPI, relative to the game directory.</summary>
        private readonly string[] UninstallPaths =
        {
            // common
            "StardewModdingAPI.exe",
            "StardewModdingAPI.config.json",
            "StardewModdingAPI.data.json",
            "StardewModdingAPI.AssemblyRewriters.dll",
            "steam_appid.txt",

            // Linux/Mac only
            "Mono.Cecil.dll",
            "Mono.Cecil.Rocks.dll",
            "Newtonsoft.Json.dll",
            "StardewModdingAPI",
            "StardewModdingAPI.exe.mdb",
            "System.Numerics.dll",
            "System.Runtime.Caching.dll",

            // Windows only
            "StardewModdingAPI.pdb",

            // obsolete
            "Mods/.cache", // 1.3-1.4
            "StardewModdingAPI-settings.json" // 1.0-1.4
        };

        /// <summary>Whether the current console supports color formatting.</summary>
        private static readonly bool ConsoleSupportsColor = InteractiveInstaller.GetConsoleSupportsColor();


        /*********
        ** Public methods
        *********/
        /// <summary>Run the install or uninstall script.</summary>
        /// <param name="args">The command line arguments.</param>
        /// <remarks>
        /// Initialisation flow:
        ///     1. Collect information (mainly OS and install path) and validate it.
        ///     2. Ask the user whether to install or uninstall.
        /// 
        /// Uninstall logic:
        ///     1. On Linux/Mac: if a backup of the launcher exists, delete the launcher and restore the backup.
        ///     2. Delete all files and folders in the game directory matching one of the <see cref="UninstallPaths"/>.
        /// 
        /// Install flow:
        ///     1. Run the uninstall flow.
        ///     2. Copy the SMAPI files from package/Windows or package/Mono into the game directory.
        ///     3. On Linux/Mac: back up the game launcher and replace it with the SMAPI launcher. (This isn't possible on Windows, so the user needs to configure it manually.)
        ///     4. Create the 'Mods' directory.
        ///     5. Copy the bundled mods into the 'Mods' directory (deleting any existing versions).
        ///     6. Move any mods from app data into game's mods directory.
        /// </remarks>
        public void Run(string[] args)
        {
            /****
            ** collect details
            ****/
            Platform platform = this.DetectPlatform();
            DirectoryInfo packageDir = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "internal", platform.ToString()));
            DirectoryInfo installDir = this.InteractivelyGetInstallPath(platform);
            var paths = new
            {
                executable = Path.Combine(installDir.FullName, platform == Platform.Mono ? "StardewValley.exe" : "Stardew Valley.exe"),
                unixSmapiLauncher = Path.Combine(installDir.FullName, "StardewModdingAPI"),
                unixLauncher = Path.Combine(installDir.FullName, "StardewValley"),
                unixLauncherBackup = Path.Combine(installDir.FullName, "StardewValley-original")
            };
            this.PrintDebug($"Detected {(platform == Platform.Windows ? "Windows" : "Linux or Mac")} with game in {installDir}.");

            /****
            ** validate assumptions
            ****/
            if (!packageDir.Exists)
            {
                this.ExitError($"The 'internal/{platform}' package folder is missing (should be at {packageDir}).");
                return;
            }
            if (!File.Exists(paths.executable))
            {
                this.ExitError("The detected game install path doesn't contain a Stardew Valley executable.");
                return;
            }
            Console.WriteLine();

            /****
            ** ask user what to do
            ****/
            Console.WriteLine("You can....");
            Console.WriteLine(platform == Platform.Mono
                ? "[1] Install SMAPI. This will safely update the files so you can launch the game the same way as before."
                : "[1] Install SMAPI. You'll need to launch StardewModdingAPI.exe instead afterwards; see the readme.txt for details."
            );
            Console.WriteLine("[2] Uninstall SMAPI.");

            ScriptAction action;
            {
                string choice = this.InteractivelyChoose("What do you want to do? Type 1 or 2, then press enter.", "1", "2");
                switch (choice)
                {
                    case "1":
                        action = ScriptAction.Install;
                        break;
                    case "2":
                        action = ScriptAction.Uninstall;
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected action key '{choice}'.");
                }
            }
            Console.WriteLine();

            /****
            ** Always uninstall old files
            ****/
            // restore game launcher
            if (platform == Platform.Mono && File.Exists(paths.unixLauncherBackup))
            {
                this.PrintDebug("Removing SMAPI launcher...");
                if (File.Exists(paths.unixLauncher))
                    File.Delete(paths.unixLauncher);
                File.Move(paths.unixLauncherBackup, paths.unixLauncher);
            }

            // remove old files
            string[] removePaths = this.UninstallPaths
                .Select(path => Path.Combine(installDir.FullName, path))
                .Where(path => Directory.Exists(path) || File.Exists(path))
                .ToArray();
            if (removePaths.Any())
            {
                this.PrintDebug(action == ScriptAction.Install ? "Removing previous SMAPI files..." : "Removing SMAPI files...");
                foreach (string path in removePaths)
                {
                    if (Directory.Exists(path))
                        Directory.Delete(path, recursive: true);
                    else
                        File.Delete(path);
                }
            }

            /****
            ** Install new files
            ****/
            if (action == ScriptAction.Install)
            {
                // copy SMAPI files to game dir
                this.PrintDebug("Adding SMAPI files...");
                foreach (FileInfo sourceFile in packageDir.EnumerateFiles())
                {
                    string targetPath = Path.Combine(installDir.FullName, sourceFile.Name);
                    if (File.Exists(targetPath))
                        File.Delete(targetPath);
                    sourceFile.CopyTo(targetPath);
                }

                // replace mod launcher (if possible)
                if (platform == Platform.Mono)
                {
                    this.PrintDebug("Safely replacing game launcher...");
                    if (!File.Exists(paths.unixLauncherBackup))
                        File.Move(paths.unixLauncher, paths.unixLauncherBackup);
                    else if (File.Exists(paths.unixLauncher))
                        File.Delete(paths.unixLauncher);

                    File.Move(paths.unixSmapiLauncher, paths.unixLauncher);
                }

                // create mods directory (if needed)
                DirectoryInfo modsDir = new DirectoryInfo(Path.Combine(installDir.FullName, "Mods"));
                if (!modsDir.Exists)
                {
                    this.PrintDebug("Creating mods directory...");
                    modsDir.Create();
                }

                // add or replace bundled mods
                Directory.CreateDirectory(Path.Combine(installDir.FullName, "Mods"));
                DirectoryInfo packagedModsDir = new DirectoryInfo(Path.Combine(packageDir.FullName, "Mods"));
                if (packagedModsDir.Exists && packagedModsDir.EnumerateDirectories().Any())
                {
                    this.PrintDebug("Adding bundled mods...");
                    foreach (DirectoryInfo sourceDir in packagedModsDir.EnumerateDirectories())
                    {
                        this.PrintDebug($"   adding {sourceDir.Name}...");

                        // initialise target dir
                        DirectoryInfo targetDir = new DirectoryInfo(Path.Combine(modsDir.FullName, sourceDir.Name));
                        if (targetDir.Exists)
                            targetDir.Delete(recursive: true);
                        targetDir.Create();

                        // copy files
                        foreach (FileInfo sourceFile in sourceDir.EnumerateFiles())
                            sourceFile.CopyTo(Path.Combine(targetDir.FullName, sourceFile.Name));
                    }
                }

                // remove obsolete appdata mods
                this.InteractivelyRemoveAppDataMods(platform, modsDir, packagedModsDir);
            }
            Console.WriteLine();

            /****
            ** exit
            ****/
            this.PrintColor("Done!", ConsoleColor.DarkGreen);
            if (platform == Platform.Windows)
            {
                this.PrintColor(
                    action == ScriptAction.Install
                        ? "Don't forget to launch StardewModdingAPI.exe instead of the normal game executable. See the readme.txt for details."
                        : "If you manually changed shortcuts or Steam to launch SMAPI, don't forget to change those back.",
                    ConsoleColor.DarkGreen
                );
            }
            else if (action == ScriptAction.Install)
                this.PrintColor("You can launch the game the same way as before to play with mods.", ConsoleColor.DarkGreen);
            Console.ReadKey();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Detect the game's platform.</summary>
        /// <exception cref="NotSupportedException">The platform is not supported.</exception>
        private Platform DetectPlatform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    return Platform.Mono;

                default:
                    return Platform.Windows;
            }
        }

        /// <summary>Test whether the current console supports color formatting.</summary>
        private static bool GetConsoleSupportsColor()
        {
            try
            {
                Console.ForegroundColor = Console.ForegroundColor;
                return true;
            }
            catch (Exception)
            {
                return false; // Mono bug
            }
        }

#if SMAPI_FOR_WINDOWS
        /// <summary>Get the value of a key in the Windows registry.</summary>
        /// <param name="key">The full path of the registry key relative to HKLM.</param>
        /// <param name="name">The name of the value.</param>
        private string GetLocalMachineRegistryValue(string key, string name)
        {
            RegistryKey localMachine = Environment.Is64BitOperatingSystem ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) : Registry.LocalMachine;
            RegistryKey openKey = localMachine.OpenSubKey(key);
            if (openKey == null)
                return null;
            using (openKey)
                return (string)openKey.GetValue(name);
        }
#endif

        /// <summary>Print a debug message.</summary>
        /// <param name="text">The text to print.</param>
        private void PrintDebug(string text)
        {
            this.PrintColor(text, ConsoleColor.DarkGray);
        }

        /// <summary>Print a warning message.</summary>
        /// <param name="text">The text to print.</param>
        private void PrintWarning(string text)
        {
            this.PrintColor(text, ConsoleColor.DarkYellow);
        }

        /// <summary>Print an error and pause the console if needed.</summary>
        /// <param name="error">The error text.</param>
        private void ExitError(string error)
        {
            this.PrintColor(error, ConsoleColor.Red);
            Console.ReadLine();
        }

        /// <summary>Print a message to the console.</summary>
        /// <param name="text">The message text.</param>
        /// <param name="color">The text foreground color.</param>
        private void PrintColor(string text, ConsoleColor color)
        {
            if (InteractiveInstaller.ConsoleSupportsColor)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.ResetColor();
            }
            else
                Console.WriteLine(text);
        }

        /// <summary>Interactively ask the user to choose a value.</summary>
        /// <param name="message">The message to print.</param>
        /// <param name="options">The allowed options (not case sensitive).</param>
        private string InteractivelyChoose(string message, params string[] options)
        {
            while (true)
            {
                Console.WriteLine(message);
                string input = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (!options.Contains(input))
                {
                    Console.WriteLine("That's not a valid option.");
                    continue;
                }
                return input;
            }
        }

        /// <summary>Interactively locate the game's install path.</summary>
        /// <param name="platform">The current platform.</param>
        private DirectoryInfo InteractivelyGetInstallPath(Platform platform)
        {
            // try default paths
            foreach (string defaultPath in this.DefaultInstallPaths)
            {
                if (Directory.Exists(defaultPath))
                    return new DirectoryInfo(defaultPath);
            }

            // ask user
            Console.WriteLine("Oops, couldn't find the game automatically.");
            while (true)
            {
                // get path from user
                Console.WriteLine($"Type the file path to the game directory (the one containing '{(platform == Platform.Mono ? "StardewValley.exe" : "Stardew Valley.exe")}'), then press enter.");
                string path = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(path))
                {
                    Console.WriteLine("   You must specify a directory path to continue.");
                    continue;
                }

                // normalise on Windows
                if (platform == Platform.Windows)
                    path = path.Replace("\"", ""); // in Windows, quotes are used to escape spaces and aren't part of the file path

                // get directory
                if (File.Exists(path))
                    path = Path.GetDirectoryName(path);
                DirectoryInfo directory = new DirectoryInfo(path);

                // validate path
                if (!directory.Exists)
                {
                    Console.WriteLine("   That directory doesn't seem to exist.");
                    continue;
                }
                if (!directory.EnumerateFiles("*.exe").Any(p => p.Name == "StardewValley.exe" || p.Name == "Stardew Valley.exe"))
                {
                    Console.WriteLine("   That directory doesn't contain a Stardew Valley executable.");
                    continue;
                }

                // looks OK
                Console.WriteLine("   OK!");
                return directory;
            }
        }

        /// <summary>Interactively move mods out of the appdata directory.</summary>
        /// <param name="platform">The current platform.</param>
        /// <param name="properModsDir">The directory which should contain all mods.</param>
        /// <param name="packagedModsDir">The installer directory containing packaged mods.</param>
        private void InteractivelyRemoveAppDataMods(Platform platform, DirectoryInfo properModsDir, DirectoryInfo packagedModsDir)
        {
            // get packaged mods to delete
            string[] packagedModNames = packagedModsDir.GetDirectories().Select(p => p.Name).ToArray();

            // get path
            string homePath = platform == Platform.Windows
                ? Environment.GetEnvironmentVariable("APPDATA")
                : Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".config");
            string appDataPath = Path.Combine(homePath, "StardewValley");
            DirectoryInfo modDir = new DirectoryInfo(Path.Combine(appDataPath, "Mods"));

            // check if migration needed
            if (!modDir.Exists)
                return;
            this.PrintDebug($"Found an obsolete mod path: {modDir.FullName}");
            this.PrintDebug("   Support for mods here was dropped in SMAPI 1.0 (it was never officially supported).");

            // move mods if no conflicts (else warn)
            foreach (FileSystemInfo entry in modDir.EnumerateFileSystemInfos())
            {
                // get type
                bool isDir = entry is DirectoryInfo;
                if (!isDir && !(entry is FileInfo))
                    continue; // should never happen

                // delete packaged mods (newer version bundled into SMAPI)
                if (isDir && packagedModNames.Contains(entry.Name, StringComparer.InvariantCultureIgnoreCase))
                {
                    this.PrintDebug($"   Deleting {entry.Name} because it's bundled into SMAPI...");
                    entry.Delete();
                    continue;
                }

                // check paths
                string newPath = Path.Combine(properModsDir.FullName, entry.Name);
                if (isDir ? Directory.Exists(newPath) : File.Exists(newPath))
                {
                    this.PrintWarning($"   Can't move {entry.Name} because it already exists in your game's mod directory.");
                    continue;
                }

                // move into mods
                this.PrintDebug($"   Moving {entry.Name} into the game's mod directory...");
                this.Move(entry, newPath);
            }

            // delete if empty
            if (modDir.EnumerateFileSystemInfos().Any())
                this.PrintWarning("   You have files in this folder which couldn't be moved automatically. These will be ignored by SMAPI.");
            else
            {
                this.PrintDebug("   Deleted empty directory.");
                modDir.Delete();
            }
        }

        /// <summary>Move a filesystem entry to a new parent directory.</summary>
        /// <param name="entry">The filesystem entry to move.</param>
        /// <param name="newPath">The destination path.</param>
        /// <remarks>We can't use <see cref="FileInfo.MoveTo"/> or <see cref="DirectoryInfo.MoveTo"/>, because those don't work across partitions.</remarks>
        private void Move(FileSystemInfo entry, string newPath)
        {
            // file
            if (entry is FileInfo)
            {
                FileInfo file = (FileInfo)entry;
                file.CopyTo(newPath);
                file.Delete();
            }

            // directory
            else
            {
                Directory.CreateDirectory(newPath);

                DirectoryInfo directory = (DirectoryInfo)entry;
                foreach (FileSystemInfo child in directory.EnumerateFileSystemInfos())
                    this.Move(child, Path.Combine(newPath, child.Name));

                directory.Delete();
            }
        }
    }
}
