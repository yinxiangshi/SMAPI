using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Toolkit.Framework.UpdateData;
using StardewModdingAPI.Web.Framework.ModRepositories;

namespace StardewModdingAPI.Web.Framework.Caching.Mods
{
    /// <summary>Manages cached mod data in-memory.</summary>
    internal class ModCacheMemoryRepository : BaseCacheRepository, IModCacheRepository
    {
        /*********
        ** Fields
        *********/
        /// <summary>The cached mod data indexed by <c>{site key}:{ID}</c>.</summary>
        private readonly IDictionary<string, Cached<ModInfoModel>> Mods = new Dictionary<string, Cached<ModInfoModel>>(StringComparer.InvariantCultureIgnoreCase);


        /*********
        ** Public methods
        *********/
        /// <summary>Get the cached mod data.</summary>
        /// <param name="site">The mod site to search.</param>
        /// <param name="id">The mod's unique ID within the <paramref name="site"/>.</param>
        /// <param name="mod">The fetched mod.</param>
        /// <param name="markRequested">Whether to update the mod's 'last requested' date.</param>
        public bool TryGetMod(ModRepositoryKey site, string id, out Cached<ModInfoModel> mod, bool markRequested = true)
        {
            // get mod
            if (!this.Mods.TryGetValue(this.GetKey(site, id), out var cachedMod))
            {
                mod = null;
                return false;
            }

            // bump 'last requested'
            if (markRequested)
                cachedMod.LastRequested = DateTimeOffset.UtcNow;

            mod = cachedMod;
            return true;
        }

        /// <summary>Save data fetched for a mod.</summary>
        /// <param name="site">The mod site on which the mod is found.</param>
        /// <param name="id">The mod's unique ID within the <paramref name="site"/>.</param>
        /// <param name="mod">The mod data.</param>
        public void SaveMod(ModRepositoryKey site, string id, ModInfoModel mod)
        {
            string key = this.GetKey(site, id);
            this.Mods[key] = new Cached<ModInfoModel>(mod);
        }

        /// <summary>Delete data for mods which haven't been requested within a given time limit.</summary>
        /// <param name="age">The minimum age for which to remove mods.</param>
        public void RemoveStaleMods(TimeSpan age)
        {
            DateTimeOffset minDate = DateTimeOffset.UtcNow.Subtract(age);

            string[] staleKeys = this.Mods
                .Where(p => p.Value.LastRequested < minDate)
                .Select(p => p.Key)
                .ToArray();

            foreach (string key in staleKeys)
                this.Mods.Remove(key);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a cache key.</summary>
        /// <param name="site">The mod site.</param>
        /// <param name="id">The mod ID.</param>
        private string GetKey(ModRepositoryKey site, string id)
        {
            return $"{site}:{id.Trim()}".ToLower();
        }
    }
}
