using System;
using System.Threading.Tasks;
using StardewModdingAPI.Web.Models;

namespace StardewModdingAPI.Web.Framework
{
    /// <summary>A repository which provides mod metadata.</summary>
    internal interface IModRepository : IDisposable
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get metadata about a mod in the repository.</summary>
        /// <param name="id">The mod ID in this repository.</param>
        Task<ModGenericModel> GetModInfoAsync(int id);
    }
}
