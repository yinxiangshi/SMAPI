using System;
using System.Threading.Tasks;

namespace StardewModdingAPI.Web.Framework.Clients.GitHub
{
    /// <summary>An HTTP client for fetching metadata from GitHub.</summary>
    internal interface IGitHubClient : IDisposable
    {
        /*********
        ** Methods
        *********/
        /// <summary>Get the latest release for a GitHub repository.</summary>
        /// <param name="repo">The repository key (like <c>Pathoschild/SMAPI</c>).</param>
        /// <param name="includePrerelease">Whether to return a prerelease version if it's latest.</param>
        /// <returns>Returns the release if found, else <c>null</c>.</returns>
        Task<GitRelease> GetLatestReleaseAsync(string repo, bool includePrerelease = false);
    }
}
