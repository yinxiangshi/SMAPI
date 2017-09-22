using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pathoschild.Http.Client;
using StardewModdingAPI.Web.Models;

namespace StardewModdingAPI.Web.Framework.ModRepositories
{
    /// <summary>An HTTP client for fetching mod metadata from Nexus Mods.</summary>
    internal class NexusRepository : IModRepository
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying HTTP client.</summary>
        private readonly IClient Client;


        /*********
        ** Accessors
        *********/
        /// <summary>The unique key for this vendor.</summary>
        public string VendorKey { get; } = "Nexus";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public NexusRepository()
        {
            this.Client = new FluentClient("http://www.nexusmods.com/stardewvalley")
                .SetUserAgent("Nexus Client v0.63.15");
        }

        /// <summary>Get metadata about a mod in the repository.</summary>
        /// <param name="id">The mod ID in this repository.</param>
        public async Task<ModInfoModel> GetModInfoAsync(string id)
        {
            try
            {
                NexusResponseModel response = await this.Client
                    .GetAsync($"mods/{id}")
                    .As<NexusResponseModel>();
                return new ModInfoModel($"{this.VendorKey}:{id}", response.Name, response.Version, response.Url);
            }
            catch (Exception ex)
            {
                return new ModInfoModel($"{this.VendorKey}:{id}", ex.ToString());
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            this.Client.Dispose();
        }


        /*********
        ** Private models
        *********/
        /// <summary>A mod metadata response from Nexus Mods.</summary>
        private class NexusResponseModel
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
}
