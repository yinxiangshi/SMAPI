namespace StardewModdingAPI.Framework.Models
{
    /// <summary>Contains abstract metadata about an incompatible mod.</summary>
    internal class IncompatibleMod
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique mod ID.</summary>
        public string ID { get; set; }

        /// <summary>The mod name.</summary>
        public string Name { get; set; }

        /// <summary>The most recent incompatible mod version.</summary>
        public string Version { get; set; }

        /// <summary>The URL the user can check for an official updated version.</summary>
        public string UpdateUrl { get; set; }

        /// <summary>The URL the user can check for an unofficial updated version.</summary>
        public string UnofficialUpdateUrl { get; set; }
    }
}