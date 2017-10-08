using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Script.Serialization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using StardewModdingAPI.Common;

namespace StardewModdingAPI.ModBuildConfig
{
    /// <summary>A build task which deploys the mod files and prepares a release zip.</summary>
    public class DeployModTask : Task
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
        public string ModZipPath { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>When overridden in a derived class, executes the task.</summary>
        /// <returns>true if the task successfully executed; otherwise, false.</returns>
        public override bool Execute()
        {
            try
            {
                string modVersion = this.GetManifestVersion();
                this.CreateReleaseZip(this.Files, this.ModName, modVersion, this.ModZipPath);

                return true;
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex);
                return false;
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Create a release zip in the recommended format for uploading to mod sites.</summary>
        /// <param name="files">The files to include.</param>
        /// <param name="modName">The name of the mod.</param>
        /// <param name="modVersion">The mod version string.</param>
        /// <param name="outputFolderPath">The absolute or relative path to the folder which should contain the generated zip file.</param>
        private void CreateReleaseZip(ITaskItem[] files, string modName, string modVersion, string outputFolderPath)
        {
            // get names
            string zipName = this.EscapeInvalidFilenameCharacters($"{modName}-{modVersion}.zip");
            string folderName = this.EscapeInvalidFilenameCharacters(modName);
            string zipPath = Path.Combine(outputFolderPath, zipName);

            // create zip file
            Directory.CreateDirectory(outputFolderPath);
            using (Stream zipStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                foreach (ITaskItem file in files)
                {
                    // get file info
                    string filePath = file.ItemSpec;
                    string entryName = folderName + '/' + file.GetMetadata("RecursiveDir") + file.GetMetadata("Filename") + file.GetMetadata("Extension");
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
        }

        /// <summary>Get a semantic version from the mod manifest (if available).</summary>
        /// <exception cref="InvalidOperationException">The manifest file wasn't found or is invalid.</exception>
        private string GetManifestVersion()
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

            // get version field
            object versionObj = data.ContainsKey("Version") ? data["Version"] : null;
            if (versionObj == null)
                throw new InvalidOperationException($"The mod's {this.ManifestFileName} must have a version field.");

            // get version string
            if (versionObj is IDictionary<string, object> versionFields) // SMAPI 1.x
            {
                int major = versionFields.ContainsKey("MajorVersion") ? (int)versionFields["MajorVersion"] : 0;
                int minor = versionFields.ContainsKey("MinorVersion") ? (int)versionFields["MinorVersion"] : 0;
                int patch = versionFields.ContainsKey("PatchVersion") ? (int)versionFields["PatchVersion"] : 0;
                string tag = versionFields.ContainsKey("Build") ? (string)versionFields["Build"] : null;
                return new SemanticVersionImpl(major, minor, patch, tag).ToString();
            }
            return new SemanticVersionImpl(versionObj.ToString()).ToString(); // SMAPI 2.0+
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
