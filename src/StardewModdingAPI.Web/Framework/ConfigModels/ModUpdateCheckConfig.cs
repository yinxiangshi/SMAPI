namespace StardewModdingAPI.Web.Framework.ConfigModels
{
    /// <summary>The config settings for mod update checks.</summary>
    public class ModUpdateCheckConfig
    {
        /*********
        ** Accessors
        *********/
        /****
        ** General
        ****/
        /// <summary>The number of minutes update checks should be cached before refetching them.</summary>
        public int CacheMinutes { get; set; }

        /****
        ** GitHub
        ****/
        /// <summary>The repository key for Nexus Mods.</summary>
        public string GitHubKey { get; set; }

        /// <summary>The user agent for the GitHub API client.</summary>
        public string GitHubUserAgent { get; set; }

        /// <summary>The base URL for the GitHub API.</summary>
        public string GitHubBaseUrl { get; set; }

        /// <summary>The URL for a GitHub API latest-release query excluding the <see cref="GitHubBaseUrl"/>, where {0} is the organisation and project name.</summary>
        public string GitHubReleaseUrlFormat { get; set; }

        /// <summary>The Accept header value expected by the GitHub API.</summary>
        public string GitHubAcceptHeader { get; set; }

        /****
        ** Nexus Mods
        ****/
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
