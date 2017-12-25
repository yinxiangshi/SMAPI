using Newtonsoft.Json;

namespace StardewModdingAPI.Web.Framework.Clients.Nexus
{
    /// <summary>Mod metadata from Nexus Mods.</summary>
    internal class NexusMod
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod name.</summary>
        public string Name { get; set; }

        /// <summary>The mod's semantic version number.</summary>
        public string Version { get; set; }

        /// <summary>The mod's web URL.</summary>
        [JsonProperty("mod_page_uri")]
        public string Url { get; set; }
    }
}
