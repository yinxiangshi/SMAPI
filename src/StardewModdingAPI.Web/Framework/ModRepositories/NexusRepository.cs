using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pathoschild.Http.Client;
using StardewModdingAPI.Models;

namespace StardewModdingAPI.Web.Framework.ModRepositories
{
    /// <summary>An HTTP client for fetching mod metadata from Nexus Mods.</summary>
    internal class NexusRepository : RepositoryBase
    {
        /*********
        ** Properties
        *********/
        /// <summary>The URL for a Nexus Mods API query excluding the base URL, where {0} is the mod ID.</summary>
        private readonly string ModUrlFormat;

        /// <summary>The underlying HTTP client.</summary>
        private readonly IClient Client;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="vendorKey">The unique key for this vendor.</param>
        /// <param name="userAgent">The user agent for the Nexus Mods API client.</param>
        /// <param name="baseUrl">The base URL for the Nexus Mods API.</param>
        /// <param name="modUrlFormat">The URL for a Nexus Mods API query excluding the <paramref name="baseUrl"/>, where {0} is the mod ID.</param>
        public NexusRepository(string vendorKey, string userAgent, string baseUrl, string modUrlFormat)
            : base(vendorKey)
        {
            this.ModUrlFormat = modUrlFormat;
            this.Client = new FluentClient(baseUrl).SetUserAgent(userAgent);
        }

        /// <summary>Get metadata about a mod in the repository.</summary>
        /// <param name="id">The mod ID in this repository.</param>
        public override async Task<ModInfoModel> GetModInfoAsync(string id)
        {
            // validate ID format
            if (!uint.TryParse(id, out uint _))
                return new ModInfoModel($"The value '{id}' isn't a valid Nexus mod ID, must be an integer ID.");

            // fetch info
            try
            {
                NexusResponseModel response = await this.Client
                    .GetAsync(string.Format(this.ModUrlFormat, id))
                    .As<NexusResponseModel>();

                return response != null
                    ? new ModInfoModel(response.Name, this.NormaliseVersion(response.Version), response.Url)
                    : new ModInfoModel("Found no mod with this ID.");
            }
            catch (Exception ex)
            {
                return new ModInfoModel(ex.ToString());
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
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
