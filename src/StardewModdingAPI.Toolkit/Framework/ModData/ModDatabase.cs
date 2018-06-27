using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewModdingAPI.Toolkit.Framework.ModData
{
    /// <summary>Handles access to SMAPI's internal mod metadata list.</summary>
    public class ModDatabase
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
            if (!this.TryGetRaw(manifest?.UniqueID, out string displayName, out ModDataRecord record))
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
            if (!string.IsNullOrWhiteSpace(id))
            {
                foreach (var entry in this.Records)
                {
                    displayName = entry.Key;
                    record = entry.Value;

                    // try main ID
                    if (record.ID != null && record.ID.Equals(id, StringComparison.InvariantCultureIgnoreCase))
                        return true;

                    // try former IDs
                    if (record.FormerIDs != null)
                    {
                        foreach (string part in record.FormerIDs.Split('|'))
                        {
                            if (part.Trim().Equals(id, StringComparison.InvariantCultureIgnoreCase))
                                return true;
                        }
                    }
                }
            }

            displayName = null;
            record = null;
            return false;
        }
    }
}
