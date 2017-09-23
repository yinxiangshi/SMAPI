using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pathoschild.Http.Client;
using StardewModdingAPI.Web.Models;

namespace StardewModdingAPI.Web.Framework.ModRepositories
{
    /// <summary>An HTTP client for fetching mod metadata from GitHub project releases.</summary>
    internal class GitHubRepository : IModRepository
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying HTTP client.</summary>
        private readonly IClient Client;


        /*********
        ** Accessors
        *********/
        /// <summary>The unique key for this vendor.</summary>
        public string VendorKey { get; }

        /// <summary>The URL for a Nexus Mods API query excluding the base URL, where {0} is the mod ID.</summary>
        public string ReleaseUrlFormat { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="vendorKey">The unique key for this vendor.</param>
        /// <param name="baseUrl">The base URL for the Nexus Mods API.</param>
        /// <param name="releaseUrlFormat">The URL for a Nexus Mods API query excluding the <paramref name="baseUrl"/>, where {0} is the mod ID.</param>
        /// <param name="userAgent">The user agent for the GitHub API client.</param>
        /// <param name="acceptHeader">The Accept header value expected by the GitHub API.</param>
        /// <param name="username">The username with which to authenticate to the GitHub API.</param>
        /// <param name="password">The password with which to authenticate to the GitHub API.</param>
        public GitHubRepository(string vendorKey, string baseUrl, string releaseUrlFormat, string userAgent, string acceptHeader, string username, string password)
        {
            this.VendorKey = vendorKey;
            this.ReleaseUrlFormat = releaseUrlFormat;

            this.Client = new FluentClient(baseUrl)
                .SetUserAgent(string.Format(userAgent, this.GetType().Assembly.GetName().Version))
                .AddDefault(req => req.WithHeader("Accept", acceptHeader));
            if (!string.IsNullOrWhiteSpace(username))
                this.Client = this.Client.SetBasicAuthentication(username, password);
        }

        /// <summary>Get metadata about a mod in the repository.</summary>
        /// <param name="id">The mod ID in this repository.</param>
        public async Task<ModInfoModel> GetModInfoAsync(string id)
        {
            try
            {
                GitRelease release = await this.Client
                    .GetAsync(string.Format(this.ReleaseUrlFormat, id))
                    .As<GitRelease>();

                return new ModInfoModel(id, release.Tag, $"https://github.com/{id}/releases");
            }
            catch (Exception ex)
            {
                return new ModInfoModel(ex.ToString());
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            this.Client.Dispose();
        }


        /*********
        ** Private models
        *********/
        /// <summary>Metadata about a GitHub release tag.</summary>
        private class GitRelease
        {
            /*********
            ** Accessors
            *********/
            /// <summary>The display name.</summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>The semantic version string.</summary>
            [JsonProperty("tag_name")]
            public string Tag { get; set; }
        }
    }
}
