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
        public string VendorKey { get; }

        /// <summary>The URL for a Nexus Mods API query excluding the base URL, where {0} is the mod ID.</summary>
        public string ModUrlFormat { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="vendorKey">The unique key for this vendor.</param>
        /// <param name="userAgent">The user agent for the Nexus Mods API client.</param>
        /// <param name="baseUrl">The base URL for the Nexus Mods API.</param>
        /// <param name="modUrlFormat">The URL for a Nexus Mods API query excluding the <paramref name="baseUrl"/>, where {0} is the mod ID.</param>
        public NexusRepository(string vendorKey, string userAgent, string baseUrl, string modUrlFormat)
        {
            this.VendorKey = vendorKey;
            this.ModUrlFormat = modUrlFormat;
            this.Client = new FluentClient(baseUrl).SetUserAgent(userAgent);
        }

        /// <summary>Get metadata about a mod in the repository.</summary>
        /// <param name="id">The mod ID in this repository.</param>
        public async Task<ModInfoModel> GetModInfoAsync(string id)
        {
            try
            {
                NexusResponseModel response = await this.Client
                    .GetAsync(string.Format(this.ModUrlFormat, id))
                    .As<NexusResponseModel>();

                return response != null
                    ? new ModInfoModel(response.Name, response.Version, response.Url)
                    : new ModInfoModel("Found no mod with this ID.");
            }
            catch (Exception ex)
            {
                return new ModInfoModel(ex.ToString());
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
