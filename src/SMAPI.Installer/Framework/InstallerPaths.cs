using System.IO;

namespace StardewModdingAPI.Installer.Framework
{
    /// <summary>Manages paths for the SMAPI installer.</summary>
    internal class InstallerPaths
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The directory containing the installer files for the current platform.</summary>
        public DirectoryInfo PackageDir { get; }

        /// <summary>The directory containing the installed game.</summary>
        public DirectoryInfo GameDir { get; }

        /// <summary>The directory into which to install mods.</summary>
        public DirectoryInfo ModsDir { get; }

        /// <summary>The full path to the directory containing the installer files for the current platform.</summary>
        public string PackagePath => this.PackageDir.FullName;

        /// <summary>The full path to the directory containing the installed game.</summary>
        public string GamePath => this.GameDir.FullName;

        /// <summary>The full path to the directory into which to install mods.</summary>
        public string ModsPath => this.ModsDir.FullName;

        /// <summary>The full path to the installed SMAPI executable file.</summary>
        public string ExecutablePath { get; }

        /// <summary>The full path to the vanilla game launcher on Linux/Mac.</summary>
        public string UnixLauncherPath { get; }

        /// <summary>The full path to the installed SMAPI launcher on Linux/Mac before it's renamed.</summary>
        public string UnixSmapiLauncherPath { get; }

        /// <summary>The full path to the vanilla game launcher on Linux/Mac after SMAPI is installed.</summary>
        public string UnixBackupLauncherPath { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="packageDir">The directory path containing the installer files for the current platform.</param>
        /// <param name="gameDir">The directory path for the installed game.</param>
        /// <param name="gameExecutableName">The name of the game's executable file for the current platform.</param>
        public InstallerPaths(DirectoryInfo packageDir, DirectoryInfo gameDir, string gameExecutableName)
        {
            this.PackageDir = packageDir;
            this.GameDir = gameDir;
            this.ModsDir = new DirectoryInfo(Path.Combine(gameDir.FullName, "Mods"));

            this.ExecutablePath = Path.Combine(gameDir.FullName, gameExecutableName);
            this.UnixLauncherPath = Path.Combine(gameDir.FullName, "StardewValley");
            this.UnixSmapiLauncherPath = Path.Combine(gameDir.FullName, "StardewModdingAPI");
            this.UnixBackupLauncherPath = Path.Combine(gameDir.FullName, "StardewValley-original");
        }
    }
}
