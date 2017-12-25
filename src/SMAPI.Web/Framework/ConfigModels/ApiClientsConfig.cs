namespace StardewModdingAPI.Web.Framework.ConfigModels
{
    /// <summary>The config settings for the API clients.</summary>
    internal class ApiClientsConfig
    {
        /*********
        ** Accessors
        *********/
        /****
        ** Generic
        ****/
        /// <summary>The user agent for API clients, where {0} is the SMAPI version.</summary>
        public string UserAgent { get; set; }


        /****
        ** Chucklefish
        ****/
        /// <summary>The base URL for the Chucklefish mod site.</summary>
        public string ChucklefishBaseUrl { get; set; }

        /// <summary>The URL for a mod page on the Chucklefish mod site excluding the <see cref="GitHubBaseUrl"/>, where {0} is the mod ID.</summary>
        public string ChucklefishModPageUrlFormat { get; set; }


        /****
        ** GitHub
        ****/
        /// <summary>The base URL for the GitHub API.</summary>
        public string GitHubBaseUrl { get; set; }

        /// <summary>The URL for a GitHub API latest-release query excluding the <see cref="GitHubBaseUrl"/>, where {0} is the organisation and project name.</summary>
        public string GitHubReleaseUrlFormat { get; set; }

        /// <summary>The Accept header value expected by the GitHub API.</summary>
        public string GitHubAcceptHeader { get; set; }

        /// <summary>The username with which to authenticate to the GitHub API (if any).</summary>
        public string GitHubUsername { get; set; }

        /// <summary>The password with which to authenticate to the GitHub API (if any).</summary>
        public string GitHubPassword { get; set; }

        /****
        ** Nexus Mods
        ****/
        /// <summary>The user agent for the Nexus Mods API client.</summary>
        public string NexusUserAgent { get; set; }

        /// <summary>The base URL for the Nexus Mods API.</summary>
        public string NexusBaseUrl { get; set; }

        /// <summary>The URL for a Nexus Mods API query excluding the <see cref="NexusBaseUrl"/>, where {0} is the mod ID.</summary>
        public string NexusModUrlFormat { get; set; }
    }
}
