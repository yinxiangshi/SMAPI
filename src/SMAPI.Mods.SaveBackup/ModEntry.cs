using System;
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
                ModConfig config = this.Helper.ReadConfig<ModConfig>();

                // init backup folder
                DirectoryInfo backupFolder = new DirectoryInfo(Path.Combine(this.Helper.DirectoryPath, "backups"));
                backupFolder.Create();

                // back up saves
                this.CreateBackup(backupFolder);
                this.PruneBackups(backupFolder, config.BackupsToKeep);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error backing up saves: {ex}");
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Back up the current saves.</summary>
        /// <param name="backupFolder">The folder containing save backups.</param>
        private void CreateBackup(DirectoryInfo backupFolder)
        {
            FileInfo file = new FileInfo(Path.Combine(backupFolder.FullName, this.FileName));
            if (!file.Exists)
            {
                this.Monitor.Log($"Adding {file.Name}...", LogLevel.Trace);
                ZipFile.CreateFromDirectory(Constants.SavesPath, file.FullName, CompressionLevel.Fastest, includeBaseDirectory: false);
            }
        }

        /// <summary>Remove old backups if we've exceeded the limit.</summary>
        /// <param name="backupFolder">The folder containing save backups.</param>
        /// <param name="backupsToKeep">The number of backups to keep.</param>
        private void PruneBackups(DirectoryInfo backupFolder, int backupsToKeep)
        {
            var oldBackups = backupFolder
                .GetFiles()
                .OrderByDescending(p => p.CreationTimeUtc)
                .Skip(backupsToKeep);

            foreach (FileInfo file in oldBackups)
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
    }
}
