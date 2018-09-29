using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Pathoschild.Http.Client;

namespace StardewModdingAPI.Toolkit.Framework.Clients.Wiki
{
    /// <summary>An HTTP client for fetching mod metadata from the wiki compatibility list.</summary>
    public class WikiCompatibilityClient : IDisposable
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
        public WikiCompatibilityClient(string userAgent, string baseUrl = "https://stardewvalleywiki.com/mediawiki/api.php")
        {
            this.Client = new FluentClient(baseUrl).SetUserAgent(userAgent);
        }

        /// <summary>Fetch mod compatibility entries.</summary>
        public async Task<WikiCompatibilityEntry[]> FetchAsync()
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

            // find mod entries
            HtmlNodeCollection modNodes = doc.DocumentNode.SelectNodes("table[@id='mod-list']//tr[@class='mod']");
            if (modNodes == null)
                throw new InvalidOperationException("Can't parse wiki compatibility list, no mods found.");

            // parse
            return this.ParseEntries(modNodes).ToArray();
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
        private IEnumerable<WikiCompatibilityEntry> ParseEntries(IEnumerable<HtmlNode> nodes)
        {
            foreach (HtmlNode node in nodes)
            {
                // parse mod info
                string name = node.Descendants("td").FirstOrDefault()?.Descendants("a")?.FirstOrDefault()?.InnerText?.Trim();
                string[] ids = this.GetAttribute(node, "data-id")?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray() ?? new string[0];
                int? nexusID = this.GetNullableIntAttribute(node, "data-nexus-id");
                int? chucklefishID = this.GetNullableIntAttribute(node, "data-chucklefish-id");
                string githubRepo = this.GetAttribute(node, "data-github");
                string customSourceUrl = this.GetAttribute(node, "data-custom-source");
                string customUrl = this.GetAttribute(node, "data-custom-url");

                // parse stable compatibility
                WikiCompatibilityStatus status = this.GetStatusAttribute(node, "data-status") ?? WikiCompatibilityStatus.Ok;
                ISemanticVersion unofficialVersion = this.GetSemanticVersionAttribute(node, "data-unofficial-version");
                string summary = node.Descendants().FirstOrDefault(p => p.HasClass("data-summary"))?.InnerText.Trim();

                // parse beta compatibility
                WikiCompatibilityStatus? betaStatus = this.GetStatusAttribute(node, "data-beta-status");
                ISemanticVersion betaUnofficialVersion = betaStatus.HasValue ? this.GetSemanticVersionAttribute(node, "data-beta-unofficial-version") : null;
                string betaSummary = betaStatus.HasValue ? node.Descendants().FirstOrDefault(p => p.HasClass("data-beta-summary"))?.InnerText.Trim() : null;

                // yield model
                yield return new WikiCompatibilityEntry
                {
                    // mod info
                    ID = ids,
                    Name = name,
                    NexusID = nexusID,
                    ChucklefishID = chucklefishID,
                    GitHubRepo = githubRepo,
                    CustomSourceUrl = customSourceUrl,
                    CustomUrl = customUrl,

                    // stable compatibility
                    Status = status,
                    Summary = summary,
                    UnofficialVersion = unofficialVersion,

                    // beta compatibility
                    BetaStatus = betaStatus,
                    BetaSummary = betaSummary,
                    BetaUnofficialVersion = betaUnofficialVersion
                };
            }
        }

        /// <summary>Get a compatibility status attribute value.</summary>
        /// <param name="node">The HTML node.</param>
        /// <param name="attributeName">The attribute name.</param>
        private WikiCompatibilityStatus? GetStatusAttribute(HtmlNode node, string attributeName)
        {
            string raw = node.GetAttributeValue(attributeName, null);
            if (raw == null)
                return null; // not a mod node?
            if (!Enum.TryParse(raw, true, out WikiCompatibilityStatus status))
                throw new InvalidOperationException($"Unknown status '{raw}' when parsing compatibility list.");
            return status;
        }

        /// <summary>Get a semantic version attribute value.</summary>
        /// <param name="node">The HTML node.</param>
        /// <param name="attributeName">The attribute name.</param>
        private ISemanticVersion GetSemanticVersionAttribute(HtmlNode node, string attributeName)
        {
            string raw = node.GetAttributeValue(attributeName, null);
            return SemanticVersion.TryParse(raw, out ISemanticVersion version)
                ? version
                : null;
        }

        /// <summary>Get a nullable integer attribute value.</summary>
        /// <param name="node">The HTML node.</param>
        /// <param name="attributeName">The attribute name.</param>
        private int? GetNullableIntAttribute(HtmlNode node, string attributeName)
        {
            string raw = this.GetAttribute(node, attributeName);
            if (raw != null && int.TryParse(raw, out int value))
                return value;
            return null;
        }

        /// <summary>Get a strings attribute value.</summary>
        /// <param name="node">The HTML node.</param>
        /// <param name="attributeName">The attribute name.</param>
        private string GetAttribute(HtmlNode node, string attributeName)
        {
            string raw = node.GetAttributeValue(attributeName, null);
            if (raw != null)
                raw = HtmlEntity.DeEntitize(raw);
            return raw;
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
