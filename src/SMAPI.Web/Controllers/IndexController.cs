using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using StardewModdingAPI.Internal;
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
        /// <summary>The cache in which to store release data.</summary>
        private readonly IMemoryCache Cache;

        /// <summary>The GitHub API client.</summary>
        private readonly IGitHubClient GitHub;

        /// <summary>The cache time for release info.</summary>
        private readonly TimeSpan CacheTime = TimeSpan.FromSeconds(1);

        /// <summary>The GitHub repository name to check for update.</summary>
        private readonly string RepositoryName = "Pathoschild/SMAPI";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="cache">The cache in which to store release data.</param>
        /// <param name="github">The GitHub API client.</param>
        public IndexController(IMemoryCache cache, IGitHubClient github)
        {
            this.Cache = cache;
            this.GitHub = github;
        }

        /// <summary>Display the index page.</summary>
        [HttpGet]
        public async Task<ViewResult> Index()
        {
            // fetch SMAPI releases
            IndexVersionModel stableVersion = await this.Cache.GetOrCreateAsync("stable-version", async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.Add(this.CacheTime);
                GitRelease release = await this.GitHub.GetLatestReleaseAsync(this.RepositoryName, includePrerelease: false);
                return new IndexVersionModel(release.Name, release.Body, this.GetMainDownloadUrl(release), this.GetDevDownloadUrl(release));
            });
            IndexVersionModel betaVersion = await this.Cache.GetOrCreateAsync("beta-version", async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.Add(this.CacheTime);
                GitRelease release = await this.GitHub.GetLatestReleaseAsync(this.RepositoryName, includePrerelease: true);
                return release.IsPrerelease
                    ? this.GetBetaDownload(release)
                    : null;
            });

            // render view
            var model = new IndexModel(stableVersion, betaVersion);
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

        /// <summary>Get the latest beta download for a SMAPI release.</summary>
        /// <param name="release">The SMAPI release.</param>
        private IndexVersionModel GetBetaDownload(GitRelease release)
        {
            // get download with the latest version
            SemanticVersionImpl latestVersion = null;
            string latestUrl = null;
            foreach (GitAsset asset in release.Assets ?? new GitAsset[0])
            {
                // parse version
                Match versionMatch = Regex.Match(asset.FileName, @"SMAPI-([\d\.]+(?:-.+)?)-installer.zip");
                if (!versionMatch.Success || !SemanticVersionImpl.TryParse(versionMatch.Groups[1].Value, out SemanticVersionImpl version))
                    continue;

                // save latest version
                if (latestVersion == null || latestVersion.CompareTo(version) < 0)
                {
                    latestVersion = version;
                    latestUrl = asset.DownloadUrl;
                }
            }

            // return if prerelease
            return latestVersion?.Tag != null
                ? new IndexVersionModel(latestVersion.ToString(), release.Body, latestUrl, null)
                : null;
        }
    }
}
