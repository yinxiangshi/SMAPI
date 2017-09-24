using System;
using Newtonsoft.Json;

namespace StardewModdingAPI.Framework.Models
{
    /// <summary>Uniquely identifies a mod in SMAPI's internal data.</summary>
    internal class ModDataID
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique mod ID.</summary>
        public string ID { get; set; }

        /// <summary>The mod name to disambiguate non-unique IDs, or <c>null</c> to ignore the mod name.</summary>
        public string Name { get; set; }

        /// <summary>The author name to disambiguate non-unique IDs, or <c>null</c> to ignore the author.</summary>
        public string Author { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public ModDataID() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The mod ID or a JSON string matching the <see cref="ModDataID"/> fields.</param>
        public ModDataID(string data)
        {
            // JSON can be stuffed into the ID string as a convenience hack to keep JSON mod lists
            // formatted readably. The tradeoff is that the format is a bit more magical, but that's
            // probably acceptable since players aren't meant to edit it. It's also fairly clear what
            // the JSON strings do, if not necessarily how.
            if (data.StartsWith("{"))
                JsonConvert.PopulateObject(data, this);
            else
                this.ID = data;
        }

        /// <summary>Get whether this ID matches a given mod manifest.</summary>
        /// <param name="id">The mod's unique ID, or a substitute ID if it isn't set in the manifest.</param>
        /// <param name="manifest">The manifest to check.</param>
        public bool Matches(string id, IManifest manifest)
        {
            return
                this.ID.Equals(id, StringComparison.InvariantCultureIgnoreCase)
                && (
                    this.Author == null
                    || this.Author.Equals(manifest.Author, StringComparison.InvariantCultureIgnoreCase)
                    || (manifest.ExtraFields.ContainsKey("Authour") && this.Author.Equals(manifest.ExtraFields["Authour"].ToString(), StringComparison.InvariantCultureIgnoreCase))
                )
                && (this.Name == null || this.Name.Equals(manifest.Name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
