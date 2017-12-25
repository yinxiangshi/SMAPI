using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StardewModdingAPI.Web.Framework.Clients.GitHub;
using StardewModdingAPI.Web.ViewModels;

namespace StardewModdingAPI.Web.Controllers
{
    /// <summary>Provides an info/download page about SMAPI.</summary>
    [Route("")]
    [Route("install")]
    internal class IndexController : Controller
    {
        /*********
        ** Properties
        *********/
        /// <summary>The GitHub API client.</summary>
        private readonly IGitHubClient GitHub;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="github">The GitHub API client.</param>
        public IndexController(IGitHubClient github)
        {
            this.GitHub = github;
        }

        /// <summary>Display the index page.</summary>
        [HttpGet]
        public async Task<ViewResult> Index()
        {
            // fetch latest SMAPI release
            GitRelease release = await this.GitHub.GetLatestReleaseAsync("Pathoschild/SMAPI");
            string downloadUrl = this.GetMainDownloadUrl(release);
            string devDownloadUrl = this.GetDevDownloadUrl(release);

            // render view
            var model = new IndexModel(release.Name, release.Body, downloadUrl, devDownloadUrl);
            return this.View(model);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the main download URL for a SMAPI release.</summary>
        /// <param name="release">The SMAPI release.</param>
        private string GetMainDownloadUrl(GitRelease release)
        {
            // get main download URL
            foreach (GitAsset asset in release.Assets ?? new GitAsset[0])
            {
                if (Regex.IsMatch(asset.FileName, @"SMAPI-[\d\.]+-installer.zip"))
                    return asset.DownloadUrl;
            }

            // fallback just in case
            return "https://github.com/pathoschild/SMAPI/releases";
        }

        /// <summary>Get the for-developers download URL for a SMAPI release.</summary>
        /// <param name="release">The SMAPI release.</param>
        private string GetDevDownloadUrl(GitRelease release)
        {
            // get dev download URL
            foreach (GitAsset asset in release.Assets ?? new GitAsset[0])
            {
                if (Regex.IsMatch(asset.FileName, @"SMAPI-[\d\.]+-installer-for-developers.zip"))
                    return asset.DownloadUrl;
            }

            // fallback just in case
            return "https://github.com/pathoschild/SMAPI/releases";
        }
    }
}
