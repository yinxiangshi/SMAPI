using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StardewModdingAPI.Web.Framework.ModRepositories;
using StardewModdingAPI.Web.Models;

namespace StardewModdingAPI.Web.Controllers
{
    /// <summary>Provides an API to perform mod update checks.</summary>
    [Route("api/v1.0/mods")]
    [Produces("application/json")]
    public class ModsController : Controller
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod repositories which provide mod metadata.</summary>
        private readonly IDictionary<string, IModRepository> Repositories =
            new IModRepository[]
            {
                new NexusRepository()
            }
            .ToDictionary(p => p.VendorKey, StringComparer.CurrentCultureIgnoreCase);


        /*********
        ** Public methods
        *********/
        /// <summary>Fetch version metadata for the given mods.</summary>
        /// <param name="modKeys">The namespaced mod keys to search as a comma-delimited array.</param>
        [HttpGet]
        public async Task<IDictionary<string, ModInfoModel>> GetAsync(string modKeys)
        {
            return await this.Post(new ModSearchModel
            {
                ModKeys = modKeys?.Split(',').Select(p => p.Trim()).ToArray() ?? new string[0]
            });
        }

        /// <summary>Fetch version metadata for the given mods.</summary>
        /// <param name="search">The search options.</param>
        [HttpPost]
        public async Task<IDictionary<string, ModInfoModel>> Post([FromBody] ModSearchModel search)
        {
            // sort & filter keys
            string[] modKeys = (search.ModKeys ?? new string[0])
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
                    result[modKey] = new ModInfoModel("The mod key isn't in a valid format. It should contain the mod repository key and mod ID like 'Nexus:541'.");
                    continue;
                }

                // get matching repository
                if (!this.Repositories.TryGetValue(vendorKey, out IModRepository repository))
                {
                    result[modKey] = new ModInfoModel("There's no mod repository matching this namespaced mod ID.");
                    continue;
                }

                // fetch mod info
                result[modKey] = await repository.GetModInfoAsync(modID);
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
            vendorKey = parts[0];
            modID = parts[1];
            return true;
        }
    }
}
