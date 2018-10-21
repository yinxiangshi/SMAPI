using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Pathoschild.Http.Client;

namespace StardewModdingAPI.Toolkit.Framework.Clients.Wiki
{
    /// <summary>An HTTP client for fetching mod metadata from the wiki.</summary>
    public class WikiClient : IDisposable
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying HTTP client.</summary>
        private readonly IClient Client;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="userAgent">The user agent for the wiki API.</param>
        /// <param name="baseUrl">The base URL for the wiki API.</param>
        public WikiClient(string userAgent, string baseUrl = "https://stardewvalleywiki.com/mediawiki/api.php")
        {
            this.Client = new FluentClient(baseUrl).SetUserAgent(userAgent);
        }

        /// <summary>Fetch mods from the compatibility list.</summary>
        public async Task<WikiModList> FetchModsAsync()
        {
            // fetch HTML
            ResponseModel response = await this.Client
                .GetAsync("")
                .WithArguments(new
                {
                    action = "parse",
                    page = "Modding:SMAPI_compatibility",
                    format = "json"
                })
                .As<ResponseModel>();
            string html = response.Parse.Text["*"];

            // parse HTML
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // fetch game versions
            string stableVersion = doc.DocumentNode.SelectSingleNode("div[@class='game-stable-version']")?.InnerText;
            string betaVersion = doc.DocumentNode.SelectSingleNode("div[@class='game-beta-version']")?.InnerText;

            // find mod entries
            HtmlNodeCollection modNodes = doc.DocumentNode.SelectNodes("table[@id='mod-list']//tr[@class='mod']");
            if (modNodes == null)
                throw new InvalidOperationException("Can't parse wiki compatibility list, no mods found.");

            // parse
            WikiModEntry[] mods = this.ParseEntries(modNodes).ToArray();
            return new WikiModList
            {
                StableVersion = stableVersion,
                BetaVersion = betaVersion,
                Mods = mods
            };
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            this.Client?.Dispose();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse valid mod compatibility entries.</summary>
        /// <param name="nodes">The HTML compatibility entries.</param>
        private IEnumerable<WikiModEntry> ParseEntries(IEnumerable<HtmlNode> nodes)
        {
            foreach (HtmlNode node in nodes)
            {
                // extract fields
                string name = this.GetMetadataField(node, "mod-name");
                string alternateNames = this.GetMetadataField(node, "mod-name2");
                string author = this.GetMetadataField(node, "mod-author");
                string alternateAuthors = this.GetMetadataField(node, "mod-author2");
                string[] ids = this.GetMetadataField(node, "mod-id")?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray() ?? new string[0];
                int? nexusID = this.GetNullableIntField(node, "mod-nexus-id");
                int? chucklefishID = this.GetNullableIntField(node, "mod-cf-id");
                string githubRepo = this.GetMetadataField(node, "mod-github");
                string customSourceUrl = this.GetMetadataField(node, "mod-custom-source");
                string customUrl = this.GetMetadataField(node, "mod-url");
                string brokeIn = this.GetMetadataField(node, "mod-broke-in");
                string anchor = this.GetMetadataField(node, "mod-anchor");

                // parse stable compatibility
                WikiCompatibilityInfo compatibility = new WikiCompatibilityInfo
                {
                    Status = this.GetStatusField(node, "mod-status") ?? WikiCompatibilityStatus.Ok,
                    UnofficialVersion = this.GetSemanticVersionField(node, "mod-unofficial-version"),
                    UnofficialUrl = this.GetMetadataField(node, "mod-unofficial-url"),
                    Summary = this.GetMetadataField(node, "mod-summary")?.Trim()
                };

                // parse beta compatibility
                WikiCompatibilityInfo betaCompatibility = null;
                {
                    WikiCompatibilityStatus? betaStatus = this.GetStatusField(node, "mod-beta-status");
                    if (betaStatus.HasValue)
                    {
                        betaCompatibility = new WikiCompatibilityInfo
                        {
                            Status = betaStatus.Value,
                            UnofficialVersion = this.GetSemanticVersionField(node, "mod-beta-unofficial-version"),
                            UnofficialUrl = this.GetMetadataField(node, "mod-beta-unofficial-url"),
                            Summary = this.GetMetadataField(node, "mod-beta-summary")
                        };
                    }
                }

                // yield model
                yield return new WikiModEntry
                {
                    ID = ids,
                    Name = name,
                    AlternateNames = alternateNames,
                    Author = author,
                    AlternateAuthors = alternateAuthors,
                    NexusID = nexusID,
                    ChucklefishID = chucklefishID,
                    GitHubRepo = githubRepo,
                    CustomSourceUrl = customSourceUrl,
                    CustomUrl = customUrl,
                    BrokeIn = brokeIn,
                    Compatibility = compatibility,
                    BetaCompatibility = betaCompatibility,
                    Anchor = anchor
                };
            }
        }

        /// <summary>Get the value of a metadata field.</summary>
        /// <param name="container">The metadata container.</param>
        /// <param name="name">The field name.</param>
        private string GetMetadataField(HtmlNode container, string name)
        {
            return container.Descendants().FirstOrDefault(p => p.HasClass(name))?.InnerHtml;
        }

        /// <summary>Get the value of a metadata field as a compatibility status.</summary>
        /// <param name="container">The metadata container.</param>
        /// <param name="name">The field name.</param>
        private WikiCompatibilityStatus? GetStatusField(HtmlNode container, string name)
        {
            string raw = this.GetMetadataField(container, name);
            if (raw == null)
                return null;
            if (!Enum.TryParse(raw, true, out WikiCompatibilityStatus status))
                throw new InvalidOperationException($"Unknown status '{raw}' when parsing compatibility list.");
            return status;
        }

        /// <summary>Get the value of a metadata field as a semantic version.</summary>
        /// <param name="container">The metadata container.</param>
        /// <param name="name">The field name.</param>
        private ISemanticVersion GetSemanticVersionField(HtmlNode container, string name)
        {
            string raw = this.GetMetadataField(container, name);
            return SemanticVersion.TryParse(raw, out ISemanticVersion version)
                ? version
                : null;
        }

        /// <summary>Get the value of a metadata field as a nullable integer.</summary>
        /// <param name="container">The metadata container.</param>
        /// <param name="name">The field name.</param>
        private int? GetNullableIntField(HtmlNode container, string name)
        {
            string raw = this.GetMetadataField(container, name);
            if (raw != null && int.TryParse(raw, out int value))
                return value;
            return null;
        }

        /// <summary>The response model for the MediaWiki parse API.</summary>
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class ResponseModel
        {
            /// <summary>The parse API results.</summary>
            public ResponseParseModel Parse { get; set; }
        }

        /// <summary>The inner response model for the MediaWiki parse API.</summary>
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class ResponseParseModel
        {
            /// <summary>The parsed text.</summary>
            public IDictionary<string, string> Text { get; set; }
        }
    }
}
