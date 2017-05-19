namespace StardewModdingAPI.Framework.Models
{
    /// <summary>The SMAPI configuration settings.</summary>
    internal class SConfig
    {
        /********
        ** Accessors
        ********/
        /// <summary>Whether to enable development features.</summary>
        public bool DeveloperMode { get; set; }

        /// <summary>Whether to check if a newer version of SMAPI is available on startup.</summary>
        public bool CheckForUpdates { get; set; } = true;

        /// <summary>Whether SMAPI should log more information about the game context.</summary>
        public bool VerboseLogging { get; set; } = false;

        /// <summary>A list of mod versions which should be considered compatible or incompatible regardless of whether SMAPI detects incompatible code.</summary>
        public ModCompatibility[] ModCompatibility { get; set; }
    }
}
