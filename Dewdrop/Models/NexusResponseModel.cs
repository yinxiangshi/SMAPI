using System;
using Newtonsoft.Json;

namespace Dewdrop.Models
{
    public class NexusResponseModel : IModModel
    {
        /// <summary>
        /// The name of the mod.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The version of the mod.
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// The NexusMod ID for the mod.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// The URL of the mod.
        /// </summary>
        [JsonProperty("mod_page_uri")]
        public string Url { get; set; }

        /// <summary>
        /// Return mod information about a Nexus mod
        /// </summary>
        /// <returns><see cref="ModGenericModel"/></returns>
        public ModGenericModel ModInfo()
        {
            return new ModGenericModel
            {
                Id = Id,
                Version = Version,
                Name = Name,
                Url = Url,
                Vendor = "Nexus"
            };
        }
    }
}
