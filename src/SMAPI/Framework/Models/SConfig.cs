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

        /// <summary>Whether to check for newer versions of SMAPI and mods on startup.</summary>
        public bool CheckForUpdates { get; set; }

        /// <summary>Whether to show beta versions as valid updates.</summary>
        public bool UseBetaChannel { get; set; } = Constants.ApiVersion.Build != null;

        /// <summary>SMAPI's GitHub project name, used to perform update checks.</summary>
        public string GitHubProjectName { get; set; }

        /// <summary>The base URL for SMAPI's web API, used to perform update checks.</summary>
        public string WebApiBaseUrl { get; set; }

        /// <summary>Whether SMAPI should log more information about the game context.</summary>
        public bool VerboseLogging { get; set; }

        /// <summary>The console color scheme to use.</summary>
        public MonitorColorScheme ColorScheme { get; set; }
    }
}
