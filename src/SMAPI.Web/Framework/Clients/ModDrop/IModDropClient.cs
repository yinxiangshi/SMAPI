using System;
using System.Threading.Tasks;

namespace StardewModdingAPI.Web.Framework.Clients.ModDrop
{
    /// <summary>An HTTP client for fetching mod metadata from the ModDrop API.</summary>
    internal interface IModDropClient : IDisposable
    {
        /*********
        ** Methods
        *********/
        /// <summary>Get metadata about a mod.</summary>
        /// <param name="id">The ModDrop mod ID.</param>
        /// <returns>Returns the mod info if found, else <c>null</c>.</returns>
        Task<ModDropMod> GetModAsync(long id);
    }
}
