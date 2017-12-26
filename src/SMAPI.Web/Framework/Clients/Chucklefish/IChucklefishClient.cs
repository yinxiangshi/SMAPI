using System;
using System.Threading.Tasks;

namespace StardewModdingAPI.Web.Framework.Clients.Chucklefish
{
    /// <summary>An HTTP client for fetching mod metadata from the Chucklefish mod site.</summary>
    internal interface IChucklefishClient : IDisposable
    {
        /*********
        ** Methods
        *********/
        /// <summary>Get metadata about a mod.</summary>
        /// <param name="id">The Chucklefish mod ID.</param>
        /// <returns>Returns the mod info if found, else <c>null</c>.</returns>
        Task<ChucklefishMod> GetModAsync(uint id);
    }
}
