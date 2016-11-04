using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private readonly string[] DefaultInstallPaths = {
            // Linux
            $"{Environment.GetEnvironmentVariable("HOME")}/GOG Games/Stardew Valley/game",
            $"{Environment.GetEnvironmentVariable("HOME")}/.local/share/Steam/steamapps/common/Stardew Valley",

            // Mac
            $"{Environment.GetEnvironmentVariable("HOME")}/Library/Application Support/Steam/steamapps/common/Stardew Valley/Contents/MacOS",

            // Windows
            @"C:\Program Files (x86)\GalaxyClient\Games\Stardew Valley",
            @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley"
        };


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
        /// Install flow:
        ///     1. Copy the SMAPI files from package/Windows or package/Mono into the game directory.
        ///     2. On Linux/Mac: back up the game launcher and replace it with the SMAPI launcher. (This isn't possible on Windows, so the user needs to configure it manually.)
        ///     3. Create the 'Mods' directory.
        ///     4. Copy the bundled mods into the 'Mods' directory (deleting any existing versions).
        /// 
        /// Uninstall logic:
        ///     1. On Linux/Mac: if a backup of the launcher exists, delete the launcher and restore the backup.
        ///     2. Delete all files in the game directory matching a file under package/Windows or package/Mono.
        /// </remarks>
        public void Run(string[] args)
        {
            /****
            ** collect details
            ****/
            this.PrintDebug("Collecting information...");
            Platform platform = this.DetectPlatform();
            DirectoryInfo packageDir = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), platform.ToString()));
            DirectoryInfo installDir = this.InteractivelyGetInstallPath();
            string executablePath = Path.Combine(installDir.FullName, platform == Platform.Mono ? "StardewValley.exe" : "Stardew Valley.exe");
            string unixGameLauncherPath = Path.Combine(installDir.FullName, "StardewValley");
            string unixGameLauncherBackupPath = Path.Combine(installDir.FullName, "StardewValley-original");
            string unixSmapiLauncherPath = Path.Combine(installDir.FullName, "StardewModdingAPI");

            this.PrintDebug($"Detected {(platform == Platform.Windows ? "Windows" : "Linux or Mac")}.");
            this.PrintDebug($"Detected game in {installDir}.");

            /****
            ** validate assumptions
            ****/
            this.PrintDebug("Verifying...");
            if (!packageDir.Exists)
            {
                this.ExitError($"The '{platform}' package directory is missing (should be at {packageDir}).");
                return;
            }
            if (!File.Exists(executablePath))
            {
                this.ExitError("The detected game install path doesn't contain a Stardew Valley executable.");
                return;
            }
            Console.WriteLine();

            /****
            ** ask user what to do
            ****/
            Console.WriteLine("You can....");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            if (platform == Platform.Mono)
                Console.WriteLine("[1] Install SMAPI. This will safely update the files so you can launch the game the same way as before.");
            else
                Console.WriteLine("[1] Install SMAPI. You'll need to launch StardewModdingAPI.exe instead afterwards; see the readme.txt for details.");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("[2] Uninstall SMAPI.");
            Console.ResetColor();

            ScriptAction action;
            {
                string choice = this.InteractivelyChoose("What do you want to do?", "1", "2");
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
            ** Perform action
            ****/
            switch (action)
            {
                case ScriptAction.Uninstall:
                    {
                        // restore game launcher
                        if (platform == Platform.Mono && File.Exists(unixGameLauncherBackupPath))
                        {
                            this.PrintDebug("Restoring game launcher...");
                            if (File.Exists(unixGameLauncherPath))
                                File.Delete(unixGameLauncherPath);
                            File.Move(unixGameLauncherBackupPath, unixGameLauncherPath);
                        }

                        // remove SMAPI files
                        this.PrintDebug("Removing SMAPI files...");
                        foreach (FileInfo sourceFile in packageDir.EnumerateFiles())
                        {
                            string targetPath = Path.Combine(installDir.FullName, sourceFile.Name);
                            if (File.Exists(targetPath))
                                File.Delete(targetPath);
                        }
                    }
                    break;

                case ScriptAction.Install:
                    {
                        // copy SMAPI files to game dir
                        this.PrintDebug("Copying SMAPI files to game directory...");
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
                            if (!File.Exists(unixGameLauncherBackupPath))
                                File.Move(unixGameLauncherPath, unixGameLauncherBackupPath);
                            else if (File.Exists(unixGameLauncherPath))
                                File.Delete(unixGameLauncherPath);

                            File.Move(unixSmapiLauncherPath, unixGameLauncherPath);
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
                    }
                    break;
            }
            Console.WriteLine();
            
            /****
            ** exit
            ****/
            Console.WriteLine("Done!");
            if (platform == Platform.Windows)
            {
                if(action == ScriptAction.Install)
                    Console.WriteLine("Don't forget to launch StardewModdingAPI.exe instead of the normal game executable. See the readme.txt for details.");
                else
                    Console.WriteLine("If you manually changed shortcuts or Steam to launch SMAPI, don't forget to change those back.");
            }
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

        /// <summary>Print a debug message.</summary>
        /// <param name="text">The text to print.</param>
        private void PrintDebug(string text)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        /// <summary>Print a warning message.</summary>
        /// <param name="text">The text to print.</param>
        private void PrintWarning(string text)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        /// <summary>Print an error and pause the console if needed.</summary>
        /// <param name="error">The error text.</param>
        private void ExitError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
            Console.ReadLine();
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
        private DirectoryInfo InteractivelyGetInstallPath()
        {
            // try default paths
            foreach (string defaultPath in this.DefaultInstallPaths)
            {
                if (Directory.Exists(defaultPath))
                    return new DirectoryInfo(defaultPath);
            }

            // ask user
            Console.WriteLine("Oops, couldn't find your Stardew Valley install path automatically. You'll need to specify where the game is installed (or install SMAPI manually).");
            while (true)
            {
                // get path from user
                Console.WriteLine("   Enter the game's full directory path (the one containing 'StardewValley.exe' or 'Stardew Valley.exe').");
                Console.Write("   > ");
                string path = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(path))
                {
                    Console.WriteLine("   You must specify a directory path to continue.");
                    continue;
                }

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
    }
}
