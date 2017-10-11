using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using StardewModdingAPI.ModBuildConfig.Framework;

namespace StardewModdingAPI.ModBuildConfig.BuildTasks
{
    /// <summary>A build task which validates the install context.</summary>
    public class ValidateInstallTask : Task
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
        /// <summary>The folder containing the game files.</summary>
        public string GameDir { get; set; }

        /// <summary>The MSBuild OS value.</summary>
        public string Platform { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>When overridden in a derived class, executes the task.</summary>
        /// <returns>true if the task successfully executed; otherwise, false.</returns>
        public override bool Execute()
        {
            try
            {
                if (!this.ValidPlatforms.Contains(this.Platform))
                    throw new UserErrorException($"The mod build package doesn't recognise OS type '{this.Platform}'.");
                if (!Directory.Exists(this.GameDir))
                    throw new UserErrorException("The mod build package can't find your game path. See https://github.com/Pathoschild/SMAPI/blob/develop/docs/mod-build-config.md for help specifying it.");
                if (!File.Exists(Path.Combine(this.GameDir, this.GameExeName)))
                    throw new UserErrorException($"The mod build package found a game folder at {this.GameDir}, but it doesn't contain the {this.GameExeName} file. If this folder is invalid, delete it and the package will autodetect another game install path.");
                if (!File.Exists(Path.Combine(this.GameDir, this.SmapiExeName)))
                    throw new UserErrorException($"The mod build package found a game folder at {this.GameDir}, but it doesn't contain SMAPI. You need to install SMAPI before building the mod.");

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
    }
}
