using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using StardewModdingAPI.Mods.SaveBackup.Framework;
using StardewValley;

namespace StardewModdingAPI.Mods.SaveBackup
{
    /// <summary>The main entry point for the mod.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The name of the save archive to create.</summary>
        private readonly string FileName = $"{DateTime.UtcNow:yyyy-MM-dd} - SMAPI {Constants.ApiVersion} with Stardew Valley {Game1.version}.zip";


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            try
            {
                // read config
                ModConfig config = this.Helper.ReadConfig<ModConfig>();

                // init backup folder
                DirectoryInfo folder = new DirectoryInfo(Path.Combine(this.Helper.DirectoryPath, "backups"));
                folder.Create();

                // back up saves
                {
                    FileInfo file = new FileInfo(Path.Combine(folder.FullName, this.FileName));
                    if (!file.Exists)
                    {
                        this.Monitor.Log($"Adding {file.Name}...", LogLevel.Trace);
                        ZipFile.CreateFromDirectory(Constants.SavesPath, file.FullName, CompressionLevel.Fastest, includeBaseDirectory: false);
                    }
                }

                // prune old saves
                foreach (FileInfo file in this.GetOldBackups(folder, config.BackupsToKeep))
                {
                    try
                    {
                        this.Monitor.Log($"Deleting {file.Name}...", LogLevel.Trace);
                        file.Delete();
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"Error deleting old save backup '{file.Name}': {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error backing up saves: {ex}");
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get backups ordered by creation date.</summary>
        /// <param name="folder">The folder to search.</param>
        /// <param name="skip">The number of backups to skip.</param>
        private IEnumerable<FileInfo> GetOldBackups(DirectoryInfo folder, int skip)
        {
            return folder
                .GetFiles()
                .OrderByDescending(p => p.CreationTimeUtc)
                .Skip(skip);
        }
    }
}
