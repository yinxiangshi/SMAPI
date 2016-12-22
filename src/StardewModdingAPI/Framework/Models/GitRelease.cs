using Newtonsoft.Json;

namespace StardewModdingAPI.Framework.Models
{
    /// <summary>Metadata about a GitHub release tag.</summary>
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
    }
}