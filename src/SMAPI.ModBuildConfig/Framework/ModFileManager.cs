using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using StardewModdingAPI.Toolkit;

namespace StardewModdingAPI.ModBuildConfig.Framework
{
    /// <summary>Manages the files that are part of a mod package.</summary>
    internal class ModFileManager
    {
        /*********
        ** Properties
        *********/
        /// <summary>The name of the manifest file.</summary>
        private readonly string ManifestFileName = "manifest.json";

        /// <summary>The files that are part of the package.</summary>
        private readonly IDictionary<string, FileInfo> Files;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="projectDir">The folder containing the project files.</param>
        /// <param name="targetDir">The folder containing the build output.</param>
        /// <exception cref="UserErrorException">The mod package isn't valid.</exception>
        public ModFileManager(string projectDir, string targetDir)
        {
            this.Files = new Dictionary<string, FileInfo>(StringComparer.InvariantCultureIgnoreCase);

            // validate paths
            if (!Directory.Exists(projectDir))
                throw new UserErrorException("Could not create mod package because the project folder wasn't found.");
            if (!Directory.Exists(targetDir))
                throw new UserErrorException("Could not create mod package because no build output was found.");

            // project manifest
            bool hasProjectManifest = false;
            {
                FileInfo manifest = new FileInfo(Path.Combine(projectDir, "manifest.json"));
                if (manifest.Exists)
                {
                    this.Files[this.ManifestFileName] = manifest;
                    hasProjectManifest = true;
                }
            }

            // project i18n files
            bool hasProjectTranslations = false;
            DirectoryInfo translationsFolder = new DirectoryInfo(Path.Combine(projectDir, "i18n"));
            if (translationsFolder.Exists)
            {
                foreach (FileInfo file in translationsFolder.EnumerateFiles())
                    this.Files[Path.Combine("i18n", file.Name)] = file;
                hasProjectTranslations = true;
            }

            // build output
            DirectoryInfo buildFolder = new DirectoryInfo(targetDir);
            foreach (FileInfo file in buildFolder.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                // get relative paths
                string relativePath = file.FullName.Replace(buildFolder.FullName, "");
                string relativeDirPath = file.Directory.FullName.Replace(buildFolder.FullName, "");

                // prefer project manifest/i18n files
                if (hasProjectManifest && this.EqualsInvariant(relativePath, this.ManifestFileName))
                    continue;
                if (hasProjectTranslations && this.EqualsInvariant(relativeDirPath, "i18n"))
                    continue;

                // ignore release zips
                if (this.ShouldIgnore(file))
                    continue;

                // add file
                this.Files[relativePath] = file;
            }

            // check for missing manifest
            if (!this.Files.ContainsKey(this.ManifestFileName))
                throw new UserErrorException($"Could not create mod package because no {this.ManifestFileName} was found in the project or build output.");

            // check for missing DLL
            // ReSharper disable once SimplifyLinqExpression
            if (!this.Files.Any(p => !p.Key.EndsWith(".dll")))
                throw new UserErrorException("Could not create mod package because no .dll file was found in the project or build output.");
        }

        /// <summary>Get the files in the mod package.</summary>
        public IDictionary<string, FileInfo> GetFiles()
        {
            return new Dictionary<string, FileInfo>(this.Files, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>Get a semantic version from the mod manifest.</summary>
        /// <exception cref="UserErrorException">The manifest is missing or invalid.</exception>
        public string GetManifestVersion()
        {
            // get manifest file
            if (!this.Files.TryGetValue(this.ManifestFileName, out FileInfo manifestFile))
                throw new InvalidOperationException($"The mod does not have a {this.ManifestFileName} file."); // shouldn't happen since we validate in constructor

            // read content
            string json = File.ReadAllText(manifestFile.FullName);
            if (string.IsNullOrWhiteSpace(json))
                throw new UserErrorException("The mod's manifest must not be empty.");

            // parse JSON
            IDictionary<string, object> data;
            try
            {
                data = this.Parse(json);
            }
            catch (Exception ex)
            {
                throw new UserErrorException($"The mod's manifest couldn't be parsed. It doesn't seem to be valid JSON.\n{ex}");
            }

            // get version field
            object versionObj = data.ContainsKey("Version") ? data["Version"] : null;
            if (versionObj == null)
                throw new UserErrorException("The mod's manifest must have a version field.");

            // get version string
            if (versionObj is IDictionary<string, object> versionFields) // SMAPI 1.x
            {
                int major = versionFields.ContainsKey("MajorVersion") ? (int)versionFields["MajorVersion"] : 0;
                int minor = versionFields.ContainsKey("MinorVersion") ? (int)versionFields["MinorVersion"] : 0;
                int patch = versionFields.ContainsKey("PatchVersion") ? (int)versionFields["PatchVersion"] : 0;
                string tag = versionFields.ContainsKey("Build") ? (string)versionFields["Build"] : null;
                return new SemanticVersion(major, minor, patch, tag).ToString();
            }
            return new SemanticVersion(versionObj.ToString()).ToString(); // SMAPI 2.0+
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a build output file should be ignored.</summary>
        /// <param name="file">The file info.</param>
        private bool ShouldIgnore(FileInfo file)
        {
            return
                // release zips
                this.EqualsInvariant(file.Extension, ".zip")

                // Json.NET (bundled into SMAPI)
                || this.EqualsInvariant(file.Name, "Newtonsoft.Json.dll")
                || this.EqualsInvariant(file.Name, "Newtonsoft.Json.xml")

                // code analysis files
                || file.Name.EndsWith(".CodeAnalysisLog.xml", StringComparison.InvariantCultureIgnoreCase)
                || file.Name.EndsWith(".lastcodeanalysissucceeded", StringComparison.InvariantCultureIgnoreCase)

                // OS metadata files
                || this.EqualsInvariant(file.Name, ".DS_Store")
                || this.EqualsInvariant(file.Name, "Thumbs.db");
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

        /// <summary>Get whether a string is equal to another case-insensitively.</summary>
        /// <param name="str">The string value.</param>
        /// <param name="other">The string to compare with.</param>
        private bool EqualsInvariant(string str, string other)
        {
            return str.Equals(other, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
