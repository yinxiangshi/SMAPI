using Newtonsoft.Json;

namespace StardewModdingAPI.Web.Framework.Clients.GitHub
{
    /// <summary>A GitHub project release.</summary>
    internal class GitRelease
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The display name.</summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>The semantic version string.</summary>
        [JsonProperty("tag_name")]
        public string Tag { get; set; }

        /// <summary>The Markdown description for the release.</summary>
        public string Body { get; set; }

        /// <summary>Whether this is a draft version.</summary>
        [JsonProperty("draft")]
        public bool IsDraft { get; set; }

        /// <summary>Whether this is a prerelease version.</summary>
        [JsonProperty("prerelease")]
        public bool IsPrerelease { get; set; }

        /// <summary>The attached files.</summary>
        public GitAsset[] Assets { get; set; }
    }
}
