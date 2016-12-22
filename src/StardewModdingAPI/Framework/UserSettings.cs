namespace StardewModdingAPI.Framework
{
    /// <summary>Contains user settings from SMAPI's JSON configuration file.</summary>
    internal class UserSettings
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to enable development features.</summary>
        public bool DeveloperMode { get; set; }

        /// <summary>Whether to check if a newer version of SMAPI is available on startup.</summary>
        public bool CheckForUpdates { get; set; } = true;
    }
}
