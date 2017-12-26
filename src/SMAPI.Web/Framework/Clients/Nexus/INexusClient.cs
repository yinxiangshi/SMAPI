using System;
using System.Threading.Tasks;

namespace StardewModdingAPI.Web.Framework.Clients.Nexus
{
    /// <summary>An HTTP client for fetching mod metadata from Nexus Mods.</summary>
    internal interface INexusClient : IDisposable
    {
        /*********
        ** Methods
        *********/
        /// <summary>Get metadata about a mod.</summary>
        /// <param name="id">The Nexus mod ID.</param>
        /// <returns>Returns the mod info if found, else <c>null</c>.</returns>
        Task<NexusMod> GetModAsync(uint id);
    }
}
