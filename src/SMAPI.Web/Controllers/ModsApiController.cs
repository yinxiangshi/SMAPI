using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using StardewModdingAPI.Common.Models;
using StardewModdingAPI.Web.Framework.Clients.Chucklefish;
using StardewModdingAPI.Web.Framework.Clients.GitHub;
using StardewModdingAPI.Web.Framework.Clients.Nexus;
using StardewModdingAPI.Web.Framework.ConfigModels;
using StardewModdingAPI.Web.Framework.ModRepositories;

namespace StardewModdingAPI.Web.Controllers
{
    /// <summary>Provides an API to perform mod update checks.</summary>
    [Produces("application/json")]
    [Route("api/v{version:semanticVersion}/mods")]
    internal class ModsApiController : Controller
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod repositories which provide mod metadata.</summary>
        private readonly IDictionary<string, IModRepository> Repositories;

        /// <summary>The cache in which to store mod metadata.</summary>
        private readonly IMemoryCache Cache;

        /// <summary>The number of minutes update checks should be cached before refetching them.</summary>
        private readonly int CacheMinutes;

        /// <summary>A regex which matches SMAPI-style semantic version.</summary>
        private readonly string VersionRegex;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="cache">The cache in which to store mod metadata.</param>
        /// <param name="configProvider">The config settings for mod update checks.</param>
        /// <param name="chucklefish">The Chucklefish API client.</param>
        /// <param name="github">The GitHub API client.</param>
        /// <param name="nexus">The Nexus API client.</param>
        public ModsApiController(IMemoryCache cache, IOptions<ModUpdateCheckConfig> configProvider, IChucklefishClient chucklefish, IGitHubClient github, INexusClient nexus)
        {
            ModUpdateCheckConfig config = configProvider.Value;

            this.Cache = cache;
            this.CacheMinutes = config.CacheMinutes;
            this.VersionRegex = config.SemanticVersionRegex;
            this.Repositories =
                new IModRepository[]
                {
                    new ChucklefishRepository(config.ChucklefishKey, chucklefish),
                    new GitHubRepository(config.GitHubKey, github),
                    new NexusRepository(config.NexusKey, nexus)
                }
                .ToDictionary(p => p.VendorKey, StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>Fetch version metadata for the given mods.</summary>
        /// <param name="modKeys">The namespaced mod keys to search as a comma-delimited array.</param>
        [HttpGet]
        public async Task<IDictionary<string, ModInfoModel>> GetAsync(string modKeys)
        {
            string[] modKeysArray = modKeys?.Split(',').ToArray();
            if (modKeysArray == null || !modKeysArray.Any())
                return new Dictionary<string, ModInfoModel>();

            return await this.PostAsync(new ModSearchModel(modKeysArray));
        }

        /// <summary>Fetch version metadata for the given mods.</summary>
        /// <param name="search">The mod search criteria.</param>
        [HttpPost]
        public async Task<IDictionary<string, ModInfoModel>> PostAsync([FromBody] ModSearchModel search)
        {
            // sort & filter keys
            string[] modKeys = (search?.ModKeys?.ToArray() ?? new string[0])
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(p => p, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();

            // fetch mod info
            IDictionary<string, ModInfoModel> result = new Dictionary<string, ModInfoModel>(StringComparer.CurrentCultureIgnoreCase);
            foreach (string modKey in modKeys)
            {
                // parse mod key
                if (!this.TryParseModKey(modKey, out string vendorKey, out string modID))
                {
                    result[modKey] = new ModInfoModel("The mod key isn't in a valid format. It should contain the site key and mod ID like 'Nexus:541'.");
                    continue;
                }

                // get matching repository
                if (!this.Repositories.TryGetValue(vendorKey, out IModRepository repository))
                {
                    result[modKey] = new ModInfoModel($"There's no mod site with key '{vendorKey}'. Expected one of [{string.Join(", ", this.Repositories.Keys)}].");
                    continue;
                }

                // fetch mod info
                result[modKey] = await this.Cache.GetOrCreateAsync($"{repository.VendorKey}:{modID}".ToLower(), async entry =>
                {
                    entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(this.CacheMinutes);

                    ModInfoModel info = await repository.GetModInfoAsync(modID);
                    if (info.Error == null && (info.Version == null || !Regex.IsMatch(info.Version, this.VersionRegex, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)))
                        info = new ModInfoModel(info.Name, info.Version, info.Url, info.Version == null ? "Mod has no version number." : $"Mod has invalid semantic version '{info.Version}'.");

                    return info;
                });
            }

            return result;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse a namespaced mod ID.</summary>
        /// <param name="raw">The raw mod ID to parse.</param>
        /// <param name="vendorKey">The parsed vendor key.</param>
        /// <param name="modID">The parsed mod ID.</param>
        /// <returns>Returns whether the value could be parsed.</returns>
        private bool TryParseModKey(string raw, out string vendorKey, out string modID)
        {
            // split parts
            string[] parts = raw?.Split(':');
            if (parts == null || parts.Length != 2)
            {
                vendorKey = null;
                modID = null;
                return false;
            }

            // parse
            vendorKey = parts[0].Trim();
            modID = parts[1].Trim();
            return true;
        }
    }
}
