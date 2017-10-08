using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Web.Script.Serialization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace StardewModdingAPI.ModBuildConfig.Tasks
{
    /// <summary>A build task which packs mod files into a conventional release zip.</summary>
    public class CreateModReleaseZip : Task
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod files to pack.</summary>
        [Required]
        public ITaskItem[] Files { get; set; }

        /// <summary>The name of the mod.</summary>
        [Required]
        public string ModName { get; set; }

        /// <summary>The absolute or relative path to the folder which should contain the generated zip file.</summary>
        [Required]
        public string OutputFolderPath { get; set; }


        /*********
        ** Public methods
        *********/
        public override bool Execute()
        {
            try
            {
                // create output path if needed
                Directory.CreateDirectory(this.OutputFolderPath);

                // get zip filename
                string fileName = string.Format("{0}-{1}.zip", this.ModName, this.GetManifestVersion());

                // clear old zip file if present
                string zipPath = Path.Combine(this.OutputFolderPath, fileName);
                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                // create zip file
                using (Stream zipStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    foreach (ITaskItem file in this.Files)
                    {
                        // get file info
                        string filePath = file.ItemSpec;
                        string entryName = this.ModName + '/' + file.GetMetadata("RecursiveDir") + file.GetMetadata("Filename") + file.GetMetadata("Extension");
                        if (new FileInfo(filePath).Directory.Name.Equals("i18n", StringComparison.InvariantCultureIgnoreCase))
                            entryName = Path.Combine("i18n", entryName);

                        // add to zip
                        using (Stream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        using (Stream fileStreamInZip = archive.CreateEntry(entryName).Open())
                        {
                            fileStream.CopyTo(fileStreamInZip);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex);
                return false;
            }
        }

        /// <summary>Get a semantic version from the mod manifest (if available).</summary>
        public string GetManifestVersion()
        {
            // Get the file JSON string
            string json = "";
            foreach (ITaskItem file in this.Files)
            {
                if (Path.GetFileName(file.ItemSpec).ToLower() != "manifest.json")
                    continue;
                json = File.ReadAllText(file.ItemSpec);
                break;
            }

            // Serialize the manifest json into a data object, then get a version object from that.
            IDictionary<string, object> data = (IDictionary<string, object>)new JavaScriptSerializer().DeserializeObject(json);
            IDictionary<string, object> version = (IDictionary<string, object>)data["Version"];

            // Store our version numbers for ease of use
            int major = (int)version["MajorVersion"];
            int minor = (int)version["MinorVersion"];
            int patch = (int)version["PatchVersion"];

            return String.Format("{0}.{1}.{2}", major, minor, patch);
        }
    }
}
