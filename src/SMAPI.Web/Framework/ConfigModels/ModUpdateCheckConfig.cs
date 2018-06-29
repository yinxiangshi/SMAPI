namespace StardewModdingAPI.Web.Framework.ConfigModels
{
    /// <summary>The config settings for mod update checks.</summary>
    internal class ModUpdateCheckConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The number of minutes successful update checks should be cached before refetching them.</summary>
        public int SuccessCacheMinutes { get; set; }

        /// <summary>The number of minutes failed update checks should be cached before refetching them.</summary>
        public int ErrorCacheMinutes { get; set; }

        /// <summary>A regex which matches SMAPI-style semantic version.</summary>
        /// <remarks>Derived from SMAPI's SemanticVersion implementation.</remarks>
        public string SemanticVersionRegex { get; set; }

        /// <summary>The repository key for the Chucklefish mod site.</summary>
        public string ChucklefishKey { get; set; }

        /// <summary>The repository key for Nexus Mods.</summary>
        public string GitHubKey { get; set; }

        /// <summary>The repository key for Nexus Mods.</summary>
        public string NexusKey { get; set; }

        /// <summary>The web URL for the wiki compatibility list.</summary>
        public string WikiCompatibilityPageUrl { get; set; }
    }
}
