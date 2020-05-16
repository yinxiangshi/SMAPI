using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using StardewModdingAPI.Toolkit.Framework.Clients.Wiki;

namespace StardewModdingAPI.Web.Framework.Caching.Wiki
{
    /// <summary>Manages cached wiki data in-memory.</summary>
    internal class WikiCacheMemoryRepository : BaseCacheRepository, IWikiCacheRepository
    {
        /*********
        ** Fields
        *********/
        /// <summary>The saved wiki metadata.</summary>
        private CachedWikiMetadata Metadata;

        /// <summary>The cached wiki data.</summary>
        private CachedWikiMod[] Mods = new CachedWikiMod[0];


        /*********
        ** Public methods
        *********/
        /// <summary>Get the cached wiki metadata.</summary>
        /// <param name="metadata">The fetched metadata.</param>
        public bool TryGetWikiMetadata(out CachedWikiMetadata metadata)
        {
            metadata = this.Metadata;
            return metadata != null;
        }

        /// <summary>Get the cached wiki mods.</summary>
        /// <param name="filter">A filter to apply, if any.</param>
        public IEnumerable<CachedWikiMod> GetWikiMods(Expression<Func<CachedWikiMod, bool>> filter = null)
        {
            return filter != null
                ? this.Mods.Where(filter.Compile())
                : this.Mods.ToArray();
        }

        /// <summary>Save data fetched from the wiki compatibility list.</summary>
        /// <param name="stableVersion">The current stable Stardew Valley version.</param>
        /// <param name="betaVersion">The current beta Stardew Valley version.</param>
        /// <param name="mods">The mod data.</param>
        /// <param name="cachedMetadata">The stored metadata record.</param>
        /// <param name="cachedMods">The stored mod records.</param>
        public void SaveWikiData(string stableVersion, string betaVersion, IEnumerable<WikiModEntry> mods, out CachedWikiMetadata cachedMetadata, out CachedWikiMod[] cachedMods)
        {
            this.Metadata = cachedMetadata = new CachedWikiMetadata(stableVersion, betaVersion);
            this.Mods = cachedMods = mods.Select(mod => new CachedWikiMod(mod)).ToArray();
        }
    }
}
