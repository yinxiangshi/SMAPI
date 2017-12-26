using System.Threading.Tasks;
using Pathoschild.Http.Client;

namespace StardewModdingAPI.Web.Framework.Clients.Nexus
{
    /// <summary>An HTTP client for fetching mod metadata from Nexus Mods.</summary>
    internal class NexusClient : INexusClient
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
        /// <param name="userAgent">The user agent for the Nexus Mods API client.</param>
        /// <param name="baseUrl">The base URL for the Nexus Mods API.</param>
        /// <param name="modUrlFormat">The URL for a Nexus Mods API query excluding the <paramref name="baseUrl"/>, where {0} is the mod ID.</param>
        public NexusClient(string userAgent, string baseUrl, string modUrlFormat)
        {
            this.ModUrlFormat = modUrlFormat;
            this.Client = new FluentClient(baseUrl).SetUserAgent(userAgent);
        }

        /// <summary>Get metadata about a mod.</summary>
        /// <param name="id">The Nexus mod ID.</param>
        /// <returns>Returns the mod info if found, else <c>null</c>.</returns>
        public async Task<NexusMod> GetModAsync(uint id)
        {
            return await this.Client
                .GetAsync(string.Format(this.ModUrlFormat, id))
                .As<NexusMod>();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            this.Client?.Dispose();
        }
    }
}
