using System;
using StardewModdingAPI.Toolkit.Framework.UpdateData;
using StardewModdingAPI.Web.Framework.ModRepositories;

namespace StardewModdingAPI.Web.Framework.Caching.Mods
{
    /// <summary>Manages cached mod data.</summary>
    internal interface IModCacheRepository : ICacheRepository
    {
        /*********
        ** Methods
        *********/
        /// <summary>Get the cached mod data.</summary>
        /// <param name="site">The mod site to search.</param>
        /// <param name="id">The mod's unique ID within the <paramref name="site"/>.</param>
        /// <param name="mod">The fetched mod.</param>
        /// <param name="markRequested">Whether to update the mod's 'last requested' date.</param>
        bool TryGetMod(ModRepositoryKey site, string id, out Cached<ModInfoModel> mod, bool markRequested = true);

        /// <summary>Save data fetched for a mod.</summary>
        /// <param name="site">The mod site on which the mod is found.</param>
        /// <param name="id">The mod's unique ID within the <paramref name="site"/>.</param>
        /// <param name="mod">The mod data.</param>
        void SaveMod(ModRepositoryKey site, string id, ModInfoModel mod);

        /// <summary>Delete data for mods which haven't been requested within a given time limit.</summary>
        /// <param name="age">The minimum age for which to remove mods.</param>
        void RemoveStaleMods(TimeSpan age);
    }
}
