using System;
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
        /// <summary>The URL for a GitHub releases API query excluding the base URL, where {0} is the repository owner and name.</summary>
        private readonly string ReleaseUrlFormat;

        /// <summary>The underlying HTTP client.</summary>
        private readonly IClient Client;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="baseUrl">The base URL for the GitHub API.</param>
        /// <param name="releaseUrlFormat">The URL for a GitHub releases API query excluding the <paramref name="baseUrl"/>, where {0} is the repository owner and name.</param>
        /// <param name="userAgent">The user agent for the API client.</param>
        /// <param name="acceptHeader">The Accept header value expected by the GitHub API.</param>
        /// <param name="username">The username with which to authenticate to the GitHub API.</param>
        /// <param name="password">The password with which to authenticate to the GitHub API.</param>
        public GitHubClient(string baseUrl, string releaseUrlFormat, string userAgent, string acceptHeader, string username, string password)
        {
            this.ReleaseUrlFormat = releaseUrlFormat;

            this.Client = new FluentClient(baseUrl)
                .SetUserAgent(userAgent)
                .AddDefault(req => req.WithHeader("Accept", acceptHeader));
            if (!string.IsNullOrWhiteSpace(username))
                this.Client = this.Client.SetBasicAuthentication(username, password);
        }

        /// <summary>Get the latest release for a GitHub repository.</summary>
        /// <param name="repo">The repository key (like <c>Pathoschild/SMAPI</c>).</param>
        /// <returns>Returns the latest release if found, else <c>null</c>.</returns>
        public async Task<GitRelease> GetLatestReleaseAsync(string repo)
        {
            // validate key format
            if (!repo.Contains("/") || repo.IndexOf("/", StringComparison.InvariantCultureIgnoreCase) != repo.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException($"The value '{repo}' isn't a valid GitHub repository key, must be a username and project name like 'Pathoschild/SMAPI'.", nameof(repo));

            // fetch info
            try
            {
                return await this.Client
                    .GetAsync(string.Format(this.ReleaseUrlFormat, repo))
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
    }
}
