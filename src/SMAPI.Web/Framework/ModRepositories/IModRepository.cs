using System;
using System.Threading.Tasks;
using StardewModdingAPI.Common.Models;

namespace StardewModdingAPI.Web.Framework.ModRepositories
{
    /// <summary>A repository which provides mod metadata.</summary>
    internal interface IModRepository : IDisposable
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique key for this vendor.</summary>
        string VendorKey { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get metadata about a mod in the repository.</summary>
        /// <param name="id">The mod ID in this repository.</param>
        Task<ModInfoModel> GetModInfoAsync(string id);
    }
}
