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

        /// <summary>The attached files.</summary>
        public GitAsset[] Assets { get; set; }
    }
}
