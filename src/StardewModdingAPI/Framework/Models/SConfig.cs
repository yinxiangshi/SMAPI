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

        /// <summary>A list of mod versions which should be considered incompatible.</summary>
        public IncompatibleMod[] IncompatibleMods { get; set; }
    }
}
