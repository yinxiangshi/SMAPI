using System;
using System.Linq;
using Newtonsoft.Json;

namespace StardewModdingAPI.Framework.Models
{
    /// <summary>Uniquely identifies a mod in SMAPI's internal data.</summary>
    /// <remarks>
    /// This represents a custom format which uniquely identifies a mod across all versions, even
    /// if its field values change or it doesn't specify a unique ID. This is mapped to a string
    /// with the following format:
    /// 
    /// 1. If the mod's identifier changed over time, multiple variants can be separated by the <c>|</c>
    ///    character.
    /// 2. Each variant can take one of two forms:
    ///    - A simple string matching the mod's UniqueID value.
    ///    - A JSON structure containing any of three manifest fields (ID, Name, and Author) to match.
    /// </remarks>
    internal class ModDataID
    {
        /*********
        ** Properties
        *********/
        /// <summary>The unique sets of field values which identify this mod.</summary>
        private readonly FieldSnapshot[] Snapshots;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public ModDataID() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The mod identifier string (see remarks on <see cref="ModDataID"/>).</param>
        public ModDataID(string data)
        {
            this.Snapshots =
                (
                    from string part in data.Split('|')
                    let str = part.Trim()
                    select str.StartsWith("{")
                        ? JsonConvert.DeserializeObject<FieldSnapshot>(str)
                        : new FieldSnapshot { ID = str }
                )
                .ToArray();
        }

        /// <summary>Get whether this ID matches a given mod manifest.</summary>
        /// <param name="id">The mod's unique ID, or a substitute ID if it isn't set in the manifest.</param>
        /// <param name="manifest">The manifest to check.</param>
        public bool Matches(string id, IManifest manifest)
        {
            return this.Snapshots.Any(snapshot =>
                snapshot.ID.Equals(id, StringComparison.InvariantCultureIgnoreCase)
                && (
                    snapshot.Author == null
                    || snapshot.Author.Equals(manifest.Author, StringComparison.InvariantCultureIgnoreCase)
                    || (manifest.ExtraFields.ContainsKey("Authour") && snapshot.Author.Equals(manifest.ExtraFields["Authour"].ToString(), StringComparison.InvariantCultureIgnoreCase))
                )
                && (snapshot.Name == null || snapshot.Name.Equals(manifest.Name, StringComparison.InvariantCultureIgnoreCase))
            );
        }


        /*********
        ** Private models
        *********/
        /// <summary>A unique set of fields which identifies the mod.</summary>
        private class FieldSnapshot
        {
            /*********
            ** Accessors
            *********/
            /// <summary>The unique mod ID.</summary>
            public string ID { get; set; }

            /// <summary>The mod name, or <c>null</c> to ignore the mod name.</summary>
            public string Name { get; set; }

            /// <summary>The author name, or <c>null</c> to ignore the author.</summary>
            public string Author { get; set; }
        }
    }
}
