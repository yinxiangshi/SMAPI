using Newtonsoft.Json;

namespace StardewModdingAPI.Web.Models
{
    /// <summary>A mod metadata response from Nexus Mods.</summary>
    public class NexusResponseModel : IModModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique mod ID.</summary>
        public int ID { get; set; }

        /// <summary>The mod name.</summary>
        public string Name { get; set; }

        /// <summary>The mod's semantic version number.</summary>
        public string Version { get; set; }

        /// <summary>The mod's web URL.</summary>
        [JsonProperty("mod_page_uri")]
        public string Url { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get basic mod metadata.</summary>
        public ModGenericModel ModInfo()
        {
            return new ModGenericModel
            {
                ID = this.ID,
                Version = this.Version,
                Name = this.Name,
                Url = this.Url,
                Vendor = "Nexus"
            };
        }
    }
}
