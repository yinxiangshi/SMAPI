namespace StardewModdingAPI.Framework.Models
{
    /// <summary>Metadata about for a mod that should never be loaded.</summary>
    internal class DisabledMod
    {
        /*********
        ** Accessors
        *********/
        /****
        ** From config
        ****/
        /// <summary>The unique mod IDs.</summary>
        public string[] ID { get; set; }

        /// <summary>The mod name.</summary>
        public string Name { get; set; }

        /// <summary>The reason phrase to show in the warning, or <c>null</c> to use the default value.</summary>
        /// <example>"this mod is no longer supported or used"</example>
        public string ReasonPhrase { get; set; }
    }
}
