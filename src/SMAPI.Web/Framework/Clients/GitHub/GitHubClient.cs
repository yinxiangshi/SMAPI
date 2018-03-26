using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Pathoschild.Http.Client;

namespace StardewModdingAPI.Web.Framework.Clients.GitHub
{
    /// <summary>An HTTP client for fetching metadata from GitHub.</summary>
    internal class GitHubClient : IGitHubClient
    {
        /*********
        ** Properties
        *********/
        /// <summary>The URL for a GitHub API query for the latest stable release, excluding the base URL, where {0} is the organisation and project name.</summary>
        private readonly string StableReleaseUrlFormat;

        /// <summary>The URL for a GitHub API query for the latest release (including prerelease), excluding the base URL, where {0} is the organisation and project name.</summary>
        private readonly string AnyReleaseUrlFormat;

        /// <summary>The underlying HTTP client.</summary>
        private readonly IClient Client;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="baseUrl">The base URL for the GitHub API.</param>
        /// <param name="stableReleaseUrlFormat">The URL for a GitHub API query for the latest stable release, excluding the <paramref name="baseUrl"/>, where {0} is the organisation and project name.</param>
        /// <param name="anyReleaseUrlFormat">The URL for a GitHub API query for the latest release (including prerelease), excluding the <paramref name="baseUrl"/>, where {0} is the organisation and project name.</param>
        /// <param name="userAgent">The user agent for the API client.</param>
        /// <param name="acceptHeader">The Accept header value expected by the GitHub API.</param>
        /// <param name="username">The username with which to authenticate to the GitHub API.</param>
        /// <param name="password">The password with which to authenticate to the GitHub API.</param>
        public GitHubClient(string baseUrl, string stableReleaseUrlFormat, string anyReleaseUrlFormat, string userAgent, string acceptHeader, string username, string password)
        {
            this.StableReleaseUrlFormat = stableReleaseUrlFormat;
            this.AnyReleaseUrlFormat = anyReleaseUrlFormat;

            this.Client = new FluentClient(baseUrl)
                .SetUserAgent(userAgent)
                .AddDefault(req => req.WithHeader("Accept", acceptHeader));
            if (!string.IsNullOrWhiteSpace(username))
                this.Client = this.Client.SetBasicAuthentication(username, password);
        }

        /// <summary>Get the latest release for a GitHub repository.</summary>
        /// <param name="repo">The repository key (like <c>Pathoschild/SMAPI</c>).</param>
        /// <param name="includePrerelease">Whether to return a prerelease version if it's latest.</param>
        /// <returns>Returns the release if found, else <c>null</c>.</returns>
        public async Task<GitRelease> GetLatestReleaseAsync(string repo, bool includePrerelease = false)
        {
            this.AssetKeyFormat(repo);
            try
            {
                if (includePrerelease)
                {
                    GitRelease[] results = await this.Client
                        .GetAsync(string.Format(this.AnyReleaseUrlFormat, repo))
                        .AsArray<GitRelease>();
                    return results.FirstOrDefault();
                }

                return await this.Client
                    .GetAsync(string.Format(this.StableReleaseUrlFormat, repo))
                    .As<GitRelease>();
            }
            catch (ApiException ex) when (ex.Status == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            this.Client?.Dispose();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Assert that a repository key is formatted correctly.</summary>
        /// <param name="repo">The repository key (like <c>Pathoschild/SMAPI</c>).</param>
        /// <exception cref="ArgumentException">The repository key is invalid.</exception>
        private void AssetKeyFormat(string repo)
        {
            if (repo == null || !repo.Contains("/") || repo.IndexOf("/", StringComparison.InvariantCultureIgnoreCase) != repo.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException($"The value '{repo}' isn't a valid GitHub repository key, must be a username and project name like 'Pathoschild/SMAPI'.", nameof(repo));
        }
    }
}
