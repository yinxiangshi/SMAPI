using System.Collections.Generic;
using StardewModdingAPI.Framework.ModData;

namespace StardewModdingAPI.Framework.Models
{
    /// <summary>The SMAPI predefined metadata.</summary>
    internal class SMetadata
    {
        /********
        ** Accessors
        ********/
        /// <summary>Extra metadata about mods.</summary>
        public IDictionary<string, ModDataRecord> ModData { get; set; }
    }
}
