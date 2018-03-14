using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;

namespace StardewModdingAPI.Framework.ModData
{
    /// <summary>Handles access to SMAPI's internal mod metadata list.</summary>
    internal class ModDatabase
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying mod data records indexed by default display name.</summary>
        private readonly IDictionary<string, ModDataRecord> Records;

        /// <summary>Get an update URL for an update key (if valid).</summary>
        private readonly Func<string, string> GetUpdateUrl;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an empty instance.</summary>
        public ModDatabase()
        : this(new Dictionary<string, ModDataRecord>(), key => null) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="records">The underlying mod data records indexed by default display name.</param>
        /// <param name="getUpdateUrl">Get an update URL for an update key (if valid).</param>
        public ModDatabase(IDictionary<string, ModDataRecord> records, Func<string, string> getUpdateUrl)
        {
            this.Records = records;
            this.GetUpdateUrl = getUpdateUrl;
        }

        /// <summary>Get a parsed representation of the <see cref="ModDataRecord.Fields"/> which match a given manifest.</summary>
        /// <param name="manifest">The manifest to match.</param>
        public ParsedModDataRecord GetParsed(IManifest manifest)
        {
            // get raw record
            if (!this.TryGetRaw(manifest, out string displayName, out ModDataRecord record))
                return null;

            // parse fields
            ParsedModDataRecord parsed = new ParsedModDataRecord { DisplayName = displayName, DataRecord = record };
            foreach (ModDataField field in record.GetFields().Where(field => field.IsMatch(manifest)))
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

        /// <summary>Get the display name for a given mod ID (if available).</summary>
        /// <param name="id">The unique mod ID.</param>
        public string GetDisplayNameFor(string id)
        {
            return this.TryGetRaw(id, out string displayName, out ModDataRecord _)
                ? displayName
                : null;
        }

        /// <summary>Get the mod page URL for a mod (if available).</summary>
        /// <param name="id">The unique mod ID.</param>
        public string GetModPageUrlFor(string id)
        {
            // get raw record
            if (!this.TryGetRaw(id, out string _, out ModDataRecord record))
                return null;

            // get update key
            ModDataField updateKeyField = record.GetFields().FirstOrDefault(p => p.Key == ModDataFieldKey.UpdateKey);
            if (updateKeyField == null)
                return null;

            // get update URL
            return this.GetUpdateUrl(updateKeyField.Value);
        }


        /*********
        ** Private models
        *********/
        /// <summary>Get a raw data record.</summary>
        /// <param name="id">The mod ID to match.</param>
        /// <param name="displayName">The mod's default display name.</param>
        /// <param name="record">The raw mod record.</param>
        private bool TryGetRaw(string id, out string displayName, out ModDataRecord record)
        {
            foreach (var entry in this.Records)
            {
                if (entry.Value.ID != null && entry.Value.ID.Equals(id, StringComparison.InvariantCultureIgnoreCase))
                {
                    displayName = entry.Key;
                    record = entry.Value;
                    return true;
                }
            }

            displayName = null;
            record = null;
            return false;
        }
        /// <summary>Get a raw data record.</summary>
        /// <param name="manifest">The mod manifest whose fields to match.</param>
        /// <param name="displayName">The mod's default display name.</param>
        /// <param name="record">The raw mod record.</param>
        private bool TryGetRaw(IManifest manifest, out string displayName, out ModDataRecord record)
        {
            if (manifest != null)
            {
                foreach (var entry in this.Records)
                {
                    displayName = entry.Key;
                    record = entry.Value;

                    // try main ID
                    if (record.ID != null && record.ID.Equals(manifest.UniqueID, StringComparison.InvariantCultureIgnoreCase))
                        return true;

                    // try former IDs
                    if (record.FormerIDs != null)
                    {
                        foreach (string part in record.FormerIDs.Split('|'))
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
                                        || (manifest.ExtraFields != null && manifest.ExtraFields.ContainsKey("Authour") && snapshot.Author.Equals(manifest.ExtraFields["Authour"].ToString(), StringComparison.InvariantCultureIgnoreCase))
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
                }
            }

            displayName = null;
            record = null;
            return false;
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
