using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using StardewModdingAPI.ModBuildConfig.Framework;

namespace StardewModdingAPI.ModBuildConfig
{
    /// <summary>A build task which deploys the mod files and prepares a release zip.</summary>
    public class DeployModTask : Task
    {
        /*********
        ** Properties
        *********/
        /// <summary>The MSBuild platforms recognised by the build configuration.</summary>
        private readonly HashSet<string> ValidPlatforms = new HashSet<string>(new[] { "OSX", "Unix", "Windows_NT" }, StringComparer.InvariantCultureIgnoreCase);

        /// <summary>The name of the game's main executable file.</summary>
        private string GameExeName => this.Platform == "Windows_NT"
            ? "Stardew Valley.exe"
            : "StardewValley.exe";

        /// <summary>The name of SMAPI's main executable file.</summary>
        private readonly string SmapiExeName = "StardewModdingAPI.exe";


        /*********
        ** Accessors
        *********/
        /// <summary>The name of the mod folder.</summary>
        [Required]
        public string ModFolderName { get; set; }

        /// <summary>The absolute or relative path to the folder which should contain the generated zip file.</summary>
        [Required]
        public string ModZipPath { get; set; }

        /// <summary>The folder containing the project files.</summary>
        [Required]
        public string ProjectDir { get; set; }

        /// <summary>The folder containing the build output.</summary>
        [Required]
        public string TargetDir { get; set; }

        /// <summary>The folder containing the game files.</summary>
        [Required]
        public string GameDir { get; set; }

        /// <summary>The MSBuild OS value.</summary>
        [Required]
        public string Platform { get; set; }

        /// <summary>Whether to enable copying the mod files into the game's Mods folder.</summary>
        [Required]
        public bool EnableModDeploy { get; set; }

        /// <summary>Whether to enable the release zip.</summary>
        [Required]
        public bool EnableModZip { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>When overridden in a derived class, executes the task.</summary>
        /// <returns>true if the task successfully executed; otherwise, false.</returns>
        public override bool Execute()
        {
            if (!this.EnableModDeploy && !this.EnableModZip)
                return true; // nothing to do

            try
            {
                // validate context
                if (!this.ValidPlatforms.Contains(this.Platform))
                    throw new UserErrorException($"The mod build package doesn't recognise OS type '{this.Platform}'.");
                if (!Directory.Exists(this.GameDir))
                    throw new UserErrorException("The mod build package can't find your game path. See https://github.com/Pathoschild/SMAPI/blob/develop/docs/mod-build-config.md for help specifying it.");
                if (!File.Exists(Path.Combine(this.GameDir, this.GameExeName)))
                    throw new UserErrorException($"The mod build package found a game folder at {this.GameDir}, but it doesn't contain the {this.GameExeName} file. If this folder is invalid, delete it and the package will autodetect another game install path.");
                if (!File.Exists(Path.Combine(this.GameDir, this.SmapiExeName)))
                    throw new UserErrorException($"The mod build package found a game folder at {this.GameDir}, but it doesn't contain SMAPI. You need to install SMAPI before building the mod.");

                // get mod info
                ModFileManager package = new ModFileManager(this.ProjectDir, this.TargetDir);

                // deploy mod files
                if (this.EnableModDeploy)
                {
                    string outputPath = Path.Combine(this.GameDir, "Mods", this.EscapeInvalidFilenameCharacters(this.ModFolderName));
                    this.Log.LogMessage(MessageImportance.High, $"The mod build package is copying the mod files to {outputPath}...");
                    this.CreateModFolder(package.GetFiles(), outputPath);
                }

                // create release zip
                if (this.EnableModZip)
                {
                    this.Log.LogMessage(MessageImportance.High, $"The mod build package is generating a release zip at {this.ModZipPath} for {this.ModFolderName}...");
                    this.CreateReleaseZip(package.GetFiles(), this.ModFolderName, package.GetManifestVersion(), this.ModZipPath);
                }

                return true;
            }
            catch (UserErrorException ex)
            {
                this.Log.LogErrorFromException(ex);
                return false;
            }
            catch (Exception ex)
            {
                this.Log.LogError($"The mod build package failed trying to deploy the mod.\n{ex}");
                return false;
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Copy the mod files into the game's mod folder.</summary>
        /// <param name="files">The files to include.</param>
        /// <param name="modFolderPath">The folder path to create with the mod files.</param>
        private void CreateModFolder(IDictionary<string, FileInfo> files, string modFolderPath)
        {
            Directory.CreateDirectory(modFolderPath);
            foreach (var entry in files)
            {
                string fromPath = entry.Value.FullName;
                string toPath = Path.Combine(modFolderPath, entry.Key);
                File.Copy(fromPath, toPath, overwrite: true);
            }
        }

        /// <summary>Create a release zip in the recommended format for uploading to mod sites.</summary>
        /// <param name="files">The files to include.</param>
        /// <param name="modName">The name of the mod.</param>
        /// <param name="modVersion">The mod version string.</param>
        /// <param name="outputFolderPath">The absolute or relative path to the folder which should contain the generated zip file.</param>
        private void CreateReleaseZip(IDictionary<string, FileInfo> files, string modName, string modVersion, string outputFolderPath)
        {
            // get names
            string zipName = this.EscapeInvalidFilenameCharacters($"{modName} {modVersion}.zip");
            string folderName = this.EscapeInvalidFilenameCharacters(modName);
            string zipPath = Path.Combine(outputFolderPath, zipName);

            // create zip file
            Directory.CreateDirectory(outputFolderPath);
            using (Stream zipStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                foreach (var fileEntry in files)
                {
                    string relativePath = fileEntry.Key;
                    FileInfo file = fileEntry.Value;

                    // get file info
                    string filePath = file.FullName;
                    string entryName = folderName + '/' + relativePath.Replace(Path.DirectorySeparatorChar, '/');
                    if (new FileInfo(filePath).Directory.Name.Equals("i18n", StringComparison.InvariantCultureIgnoreCase))
                        entryName = Path.Combine("i18n", entryName);

                    // add to zip
                    using (Stream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (Stream fileStreamInZip = archive.CreateEntry(entryName).Open())
                        fileStream.CopyTo(fileStreamInZip);
                }
            }
        }

        /// <summary>Get a copy of a filename with all invalid filename characters substituted.</summary>
        /// <param name="name">The filename.</param>
        private string EscapeInvalidFilenameCharacters(string name)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                name = name.Replace(invalidChar, '.');
            return name;
        }
    }
}
