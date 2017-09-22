using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StardewModdingAPI.Web.Framework;
using StardewModdingAPI.Web.Models;

namespace StardewModdingAPI.Web.Controllers
{
    /// <summary>Provides an API to perform mod update checks.</summary>
    [Produces("application/json")]
    [Route("api/check")]
    public class CheckController : Controller
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Fetch version metadata for the given mods.</summary>
        /// <param name="mods">The mods for which to fetch update metadata.</param>
        [HttpPost]
        public async Task<ModGenericModel[]> Post([FromBody] ModSearchModel[] mods)
        {
            using (NexusModsClient client = new NexusModsClient())
            {
                List<ModGenericModel> result = new List<ModGenericModel>();

                foreach (ModSearchModel mod in mods)
                {
                    if (mod.NexusID.HasValue)
                        result.Add(await client.GetModInfoAsync(mod.NexusID.Value));
                    else
                        result.Add(new ModGenericModel(null, mod.NexusID ?? 0));
                }

                return result.ToArray();
            }
        }
    }
}
