using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using StardewModdingAPI.Toolkit;
using StardewModdingAPI.Toolkit.Framework.Clients.Wiki;
using StardewModdingAPI.Web.Framework.ConfigModels;
using StardewModdingAPI.Web.ViewModels;

namespace StardewModdingAPI.Web.Controllers
{
    /// <summary>Provides user-friendly info about SMAPI mods.</summary>
    internal class ModsController : Controller
    {
        /*********
        ** Fields
        *********/
        /// <summary>The cache in which to store mod metadata.</summary>
        private readonly IMemoryCache Cache;

        /// <summary>The number of minutes successful update checks should be cached before refetching them.</summary>
        private readonly int CacheMinutes;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="cache">The cache in which to store mod metadata.</param>
        /// <param name="configProvider">The config settings for mod update checks.</param>
        public ModsController(IMemoryCache cache, IOptions<ModCompatibilityListConfig> configProvider)
        {
            ModCompatibilityListConfig config = configProvider.Value;

            this.Cache = cache;
            this.CacheMinutes = config.CacheMinutes;
        }

        /// <summary>Display information for all mods.</summary>
        [HttpGet]
        [Route("mods")]
        public async Task<ViewResult> Index()
        {
            return this.View("Index", await this.FetchDataAsync());
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Asynchronously fetch mod metadata from the wiki.</summary>
        public async Task<ModListModel> FetchDataAsync()
        {
            return await this.Cache.GetOrCreateAsync($"{nameof(ModsController)}_mod_list", async entry =>
            {
                WikiModList data = await new ModToolkit().GetWikiCompatibilityListAsync();
                ModListModel model = new ModListModel(
                    stableVersion: data.StableVersion,
                    betaVersion: data.BetaVersion,
                    mods: data
                        .Mods
                        .Select(mod => new ModModel(mod))
                        .OrderBy(p => Regex.Replace(p.Name.ToLower(), "[^a-z0-9]", "")) // ignore case, spaces, and special characters when sorting
                );

                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(this.CacheMinutes);
                return model;
            });
        }
    }
}
