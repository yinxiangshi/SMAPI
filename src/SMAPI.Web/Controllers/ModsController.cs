using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StardewModdingAPI.Toolkit;
using StardewModdingAPI.Toolkit.Framework.Clients.Wiki;
using StardewModdingAPI.Web.ViewModels;

namespace StardewModdingAPI.Web.Controllers
{
    /// <summary>Provides user-friendly info about SMAPI mods.</summary>
    internal class ModsController : Controller
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Display information for all mods.</summary>
        [HttpGet]
        [Route("mods")]
        public async Task<ViewResult> Index()
        {
            WikiModEntry[] mods = await new ModToolkit().GetWikiCompatibilityListAsync();
            ModListModel viewModel = new ModListModel(
                stableVersion: "1.3.28",
                betaVersion: "1.3.31-beta",
                mods: mods
                    .Select(mod => new ModModel(mod))
                    .OrderBy(p => Regex.Replace(p.Name.ToLower(), "[^a-z0-9]", "")) // ignore case, spaces, and special characters when sorting
            );
            return this.View("Index", viewModel);
        }
    }
}
