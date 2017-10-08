using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Script.Serialization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace StardewModdingAPI.ModBuildConfig.Tasks
{
    /// <summary>A build task which packs mod files into a conventional release zip.</summary>
    public class CreateModReleaseZip : Task
    {
        /*********
        ** Properties
        *********/
        /// <summary>The name of the manifest file.</summary>
        private readonly string ManifestFileName = "manifest.json";


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
        /// <summary>When overridden in a derived class, executes the task.</summary>
        /// <returns>true if the task successfully executed; otherwise, false.</returns>
        public override bool Execute()
        {
            try
            {
                // create output path if needed
                Directory.CreateDirectory(this.OutputFolderPath);

                // get zip filename
                string fileName = $"{this.ModName}-{this.GetManifestVersion()}.zip";

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
        /// <exception cref="InvalidOperationException">The manifest file wasn't found or is invalid.</exception>
        public string GetManifestVersion()
        {
            // find manifest file
            ITaskItem file = this.Files.FirstOrDefault(p => this.ManifestFileName.Equals(Path.GetFileName(p.ItemSpec), StringComparison.InvariantCultureIgnoreCase));
            if (file == null)
                throw new InvalidOperationException($"The mod must include a {this.ManifestFileName} file.");

            // read content
            string json = File.ReadAllText(file.ItemSpec);
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException($"The mod's {this.ManifestFileName} file must not be empty.");

            // parse JSON
            IDictionary<string, object> data;
            try
            {
                data = this.Parse(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"The mod's {this.ManifestFileName} couldn't be parsed. It doesn't seem to be valid JSON.", ex);
            }

            // extract version dictionary
            IDictionary<string, object> versionFields = (IDictionary<string, object>)data["Version"];
            int major = versionFields.ContainsKey("MajorVersion") ? (int)versionFields["MajorVersion"] : 0;
            int minor = versionFields.ContainsKey("MinorVersion") ? (int)versionFields["MinorVersion"] : 0;
            int patch = versionFields.ContainsKey("PatchVersion") ? (int)versionFields["PatchVersion"] : 0;

            return $"{major}.{minor}.{patch}";
        }

        /// <summary>Get a case-insensitive dictionary matching the given JSON.</summary>
        /// <param name="json">The JSON to parse.</param>
        private IDictionary<string, object> Parse(string json)
        {
            IDictionary<string, object> MakeCaseInsensitive(IDictionary<string, object> dict)
            {
                foreach (var field in dict.ToArray())
                {
                    if (field.Value is IDictionary<string, object> value)
                        dict[field.Key] = MakeCaseInsensitive(value);
                }
                return new Dictionary<string, object>(dict, StringComparer.InvariantCultureIgnoreCase);
            }

            IDictionary<string, object> data = (IDictionary<string, object>)new JavaScriptSerializer().DeserializeObject(json);
            return MakeCaseInsensitive(data);
        }
    }
}
