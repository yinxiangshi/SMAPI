using System.Collections.Generic;

namespace StardewModdingAPI.Toolkit.Framework.ModData
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
