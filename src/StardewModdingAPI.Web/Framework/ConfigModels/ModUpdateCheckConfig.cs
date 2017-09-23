namespace StardewModdingAPI.Web.Framework.ConfigModels
{
    /// <summary>The config settings for mod update checks.</summary>
    public class ModUpdateCheckConfig
    {
        /// <summary>The number of minutes update checks should be cached before refetching them.</summary>
        public int CacheMinutes { get; set; }

        /// <summary>The repository key for Nexus Mods.</summary>
        public string NexusKey { get; set; }

        /// <summary>The user agent for the Nexus Mods API client.</summary>
        public string NexusUserAgent { get; set; }

        /// <summary>The base URL for the Nexus Mods API.</summary>
        public string NexusBaseUrl { get; set; }

        /// <summary>The URL for a Nexus Mods API query excluding the <see cref="NexusBaseUrl"/>, where {0} is the mod ID.</summary>
        public string NexusModUrlFormat { get; set; }
    }
}
