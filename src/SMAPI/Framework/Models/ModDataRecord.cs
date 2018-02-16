using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StardewModdingAPI.Framework.Models
{
    /// <summary>Raw mod metadata from SMAPI's internal mod list.</summary>
    internal class ModDataRecord
    {
        /*********
        ** Properties
        *********/
        /// <summary>This field stores properties that aren't mapped to another field before they're parsed into <see cref="Fields"/>.</summary>
        [JsonExtensionData]
        private IDictionary<string, JToken> ExtensionData;


        /*********
        ** Accessors
        *********/
        /// <summary>The mod's current unique ID.</summary>
        public string ID { get; set; }

        /// <summary>The former mod IDs (if any).</summary>
        /// <remarks>
        /// This uses a custom format which uniquely identifies a mod across multiple versions and
        /// supports matching other fields if no ID was specified. This doesn't include the latest
        /// ID, if any. Format rules:
        ///   1. If the mod's ID changed over time, multiple variants can be separated by the
        ///      <c>|</c> character.
        ///   2. Each variant can take one of two forms:
        ///      - A simple string matching the mod's UniqueID value.
        ///      - A JSON structure containing any of four manifest fields (ID, Name, Author, and
        ///        EntryDll) to match.
        /// </remarks>
        public string FormerIDs { get; set; }

        /// <summary>Maps local versions to a semantic version for update checks.</summary>
        public IDictionary<string, string> MapLocalVersions { get; set; } = new Dictionary<string, string>();

        /// <summary>Maps remote versions to a semantic version for update checks.</summary>
        public IDictionary<string, string> MapRemoteVersions { get; set; } = new Dictionary<string, string>();

        /// <summary>The versioned field data.</summary>
        /// <remarks>
        /// This maps field names to values. This should be accessed via <see cref="GetFields"/>.
        /// Format notes:
        ///   - Each key consists of a field name prefixed with any combination of version range
        ///     and <c>Default</c>, separated by pipes (whitespace trimmed). For example, <c>Name</c>
        ///     will always override the name, <c>Default | Name</c> will only override a blank
        ///     name, and <c>~1.1 | Default | Name</c> will override blank names up to version 1.1.
        ///   - The version format is <c>min~max</c> (where either side can be blank for unbounded), or
        ///     a single version number.
        ///   - The field name itself corresponds to a <see cref="ModDataFieldKey"/> value.
        /// </remarks>
        public IDictionary<string, string> Fields { get; set; } = new Dictionary<string, string>();


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the manifest matches the <see cref="FormerIDs"/> field.</summary>
        /// <param name="manifest">The mod manifest to check.</param>
        public bool Matches(IManifest manifest)
        {
            // try main ID
            if (this.ID != null && this.ID.Equals(manifest.UniqueID, StringComparison.InvariantCultureIgnoreCase))
                return true;

            // try former IDs
            if (this.FormerIDs != null)
            {
                foreach (string part in this.FormerIDs.Split('|'))
                {
                    // packed field snapshot
                    if (part.StartsWith("{"))
                    {
                        FieldSnapshot snapshot = JsonConvert.DeserializeObject<FieldSnapshot>(part);
                        bool isMatch =
                            (snapshot.ID == null || snapshot.ID.Equals(manifest.UniqueID, StringComparison.InvariantCultureIgnoreCase))
                            && (snapshot.EntryDll == null || snapshot.EntryDll.Equals(manifest.EntryDll, StringComparison.InvariantCultureIgnoreCase))
                            && (
                                snapshot.Author == null
                                || snapshot.Author.Equals(manifest.Author, StringComparison.InvariantCultureIgnoreCase)
                                || (manifest.ExtraFields.ContainsKey("Authour") && snapshot.Author.Equals(manifest.ExtraFields["Authour"].ToString(), StringComparison.InvariantCultureIgnoreCase))
                            )
                            && (snapshot.Name == null || snapshot.Name.Equals(manifest.Name, StringComparison.InvariantCultureIgnoreCase));

                        if (isMatch)
                            return true;
                    }

                    // plain ID
                    else if (part.Equals(manifest.UniqueID, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
            }

            // no match
            return false;
        }

        /// <summary>Get a parsed representation of the <see cref="Fields"/>.</summary>
        public IEnumerable<ModDataField> GetFields()
        {
            foreach (KeyValuePair<string, string> pair in this.Fields)
            {
                // init fields
                string packedKey = pair.Key;
                string value = pair.Value;
                bool isDefault = false;
                ISemanticVersion lowerVersion = null;
                ISemanticVersion upperVersion = null;

                // parse
                string[] parts = packedKey.Split('|').Select(p => p.Trim()).ToArray();
                ModDataFieldKey fieldKey = (ModDataFieldKey)Enum.Parse(typeof(ModDataFieldKey), parts.Last(), ignoreCase: true);
                foreach (string part in parts.Take(parts.Length - 1))
                {
                    // 'default'
                    if (part.Equals("Default", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isDefault = true;
                        continue;
                    }

                    // version range
                    if (part.Contains("~"))
                    {
                        string[] versionParts = part.Split(new[] { '~' }, 2);
                        lowerVersion = versionParts[0] != "" ? new SemanticVersion(versionParts[0]) : null;
                        upperVersion = versionParts[1] != "" ? new SemanticVersion(versionParts[1]) : null;
                        continue;
                    }

                    // single version
                    lowerVersion = new SemanticVersion(part);
                    upperVersion = new SemanticVersion(part);
                }

                yield return new ModDataField(fieldKey, value, isDefault, lowerVersion, upperVersion);
            }
        }

        /// <summary>Get a parsed representation of the <see cref="Fields"/> which match a given manifest.</summary>
        /// <param name="manifest">The manifest to match.</param>
        public ParsedModDataRecord ParseFieldsFor(IManifest manifest)
        {
            ParsedModDataRecord parsed = new ParsedModDataRecord { DataRecord = this };
            foreach (ModDataField field in this.GetFields().Where(field => field.IsMatch(manifest)))
            {
                switch (field.Key)
                {
                    // update key
                    case ModDataFieldKey.UpdateKey:
                        parsed.UpdateKey = field.Value;
                        break;

                    // alternative URL
                    case ModDataFieldKey.AlternativeUrl:
                        parsed.AlternativeUrl = field.Value;
                        break;

                    // status
                    case ModDataFieldKey.Status:
                        parsed.Status = (ModStatus)Enum.Parse(typeof(ModStatus), field.Value, ignoreCase: true);
                        parsed.StatusUpperVersion = field.UpperVersion;
                        break;

                    // status reason phrase
                    case ModDataFieldKey.StatusReasonPhrase:
                        parsed.StatusReasonPhrase = field.Value;
                        break;
                }
            }

            return parsed;
        }

        /// <summary>Get a semantic local version for update checks.</summary>
        /// <param name="version">The remote version to normalise.</param>
        public string GetLocalVersionForUpdateChecks(string version)
        {
            return this.MapLocalVersions != null && this.MapLocalVersions.TryGetValue(version, out string newVersion)
                ? newVersion
                : version;
        }

        /// <summary>Get a semantic remote version for update checks.</summary>
        /// <param name="version">The remote version to normalise.</param>
        public string GetRemoteVersionForUpdateChecks(string version)
        {
            return this.MapRemoteVersions != null && this.MapRemoteVersions.TryGetValue(version, out string newVersion)
                ? newVersion
                : version;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked after JSON deserialisation.</summary>
        /// <param name="context">The deserialisation context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (this.ExtensionData != null)
            {
                this.Fields = this.ExtensionData.ToDictionary(p => p.Key, p => p.Value.ToString());
                this.ExtensionData = null;
            }
        }


        /*********
        ** Private models
        *********/
        /// <summary>A unique set of fields which identifies the mod.</summary>
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "Used via JSON deserialisation.")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Used via JSON deserialisation.")]
        private class FieldSnapshot
        {
            /*********
            ** Accessors
            *********/
            /// <summary>The unique mod ID  (or <c>null</c> to ignore it).</summary>
            public string ID { get; set; }

            /// <summary>The entry DLL (or <c>null</c> to ignore it).</summary>
            public string EntryDll { get; set; }

            /// <summary>The mod name (or <c>null</c> to ignore it).</summary>
            public string Name { get; set; }

            /// <summary>The author name (or <c>null</c> to ignore it).</summary>
            public string Author { get; set; }
        }
    }
}
