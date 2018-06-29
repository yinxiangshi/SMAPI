using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using StardewModdingAPI.Toolkit;
using StardewModdingAPI.Toolkit.Framework.Clients.WebApi;
using StardewModdingAPI.Toolkit.Framework.ModData;
using StardewModdingAPI.Web.Framework.Clients.Chucklefish;
using StardewModdingAPI.Web.Framework.Clients.GitHub;
using StardewModdingAPI.Web.Framework.Clients.Nexus;
using StardewModdingAPI.Web.Framework.ConfigModels;
using StardewModdingAPI.Web.Framework.ModRepositories;

namespace StardewModdingAPI.Web.Controllers
{
    /// <summary>Provides an API to perform mod update checks.</summary>
    [Produces("application/json")]
    [Route("api/v{version:semanticVersion}/mods")]
    internal class ModsApiController : Controller
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod repositories which provide mod metadata.</summary>
        private readonly IDictionary<string, IModRepository> Repositories;

        /// <summary>The cache in which to store mod metadata.</summary>
        private readonly IMemoryCache Cache;

        /// <summary>The number of minutes successful update checks should be cached before refetching them.</summary>
        private readonly int SuccessCacheMinutes;

        /// <summary>The number of minutes failed update checks should be cached before refetching them.</summary>
        private readonly int ErrorCacheMinutes;

        /// <summary>A regex which matches SMAPI-style semantic version.</summary>
        private readonly string VersionRegex;

        /// <summary>The internal mod metadata list.</summary>
        private readonly ModDatabase ModDatabase;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="environment">The web hosting environment.</param>
        /// <param name="cache">The cache in which to store mod metadata.</param>
        /// <param name="configProvider">The config settings for mod update checks.</param>
        /// <param name="chucklefish">The Chucklefish API client.</param>
        /// <param name="github">The GitHub API client.</param>
        /// <param name="nexus">The Nexus API client.</param>
        public ModsApiController(IHostingEnvironment environment, IMemoryCache cache, IOptions<ModUpdateCheckConfig> configProvider, IChucklefishClient chucklefish, IGitHubClient github, INexusClient nexus)
        {
            this.ModDatabase = new ModToolkit().GetModDatabase(Path.Combine(environment.WebRootPath, "StardewModdingAPI.metadata.json"));
            ModUpdateCheckConfig config = configProvider.Value;

            this.Cache = cache;
            this.SuccessCacheMinutes = config.SuccessCacheMinutes;
            this.ErrorCacheMinutes = config.ErrorCacheMinutes;
            this.VersionRegex = config.SemanticVersionRegex;
            this.Repositories =
                new IModRepository[]
                {
                    new ChucklefishRepository(config.ChucklefishKey, chucklefish),
                    new GitHubRepository(config.GitHubKey, github),
                    new NexusRepository(config.NexusKey, nexus)
                }
                .ToDictionary(p => p.VendorKey, StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>Fetch version metadata for the given mods.</summary>
        /// <param name="model">The mod search criteria.</param>
        [HttpPost]
        public async Task<IDictionary<string, ModEntryModel>> PostAsync([FromBody] ModSearchModel model)
        {
            ModSearchEntryModel[] searchMods = this.GetSearchMods(model).ToArray();
            IDictionary<string, ModEntryModel> mods = new Dictionary<string, ModEntryModel>(StringComparer.CurrentCultureIgnoreCase);
            foreach (ModSearchEntryModel mod in searchMods)
            {
                if (string.IsNullOrWhiteSpace(mod.ID))
                    continue;

                // resolve update keys
                var updateKeys = new HashSet<string>(mod.UpdateKeys ?? new string[0], StringComparer.InvariantCultureIgnoreCase);
                ModDataRecord record = this.ModDatabase.Get(mod.ID);
                if (record?.Fields != null)
                {
                    string defaultUpdateKey = record.Fields.FirstOrDefault(p => p.Key == ModDataFieldKey.UpdateKey && p.IsDefault)?.Value;
                    if (!string.IsNullOrWhiteSpace(defaultUpdateKey))
                        updateKeys.Add(defaultUpdateKey);
                }

                // get latest versions
                ModEntryModel result = new ModEntryModel { ID = mod.ID };
                IList<string> errors = new List<string>();
                foreach (string updateKey in updateKeys)
                {
                    // fetch data
                    ModInfoModel data = await this.GetInfoForUpdateKeyAsync(updateKey);
                    if (data.Error != null)
                    {
                        errors.Add(data.Error);
                        continue;
                    }

                    // handle main version
                    if (data.Version != null)
                    {
                        if (!SemanticVersion.TryParse(data.Version, out ISemanticVersion version))
                        {
                            errors.Add($"The update key '{updateKey}' matches a mod with invalid semantic version '{data.Version}'.");
                            continue;
                        }

                        if (result.Version == null || version.IsNewerThan(new SemanticVersion(result.Version)))
                        {
                            result.Name = data.Name;
                            result.Url = data.Url;
                            result.Version = version.ToString();
                        }
                    }

                    // handle optional version
                    if (data.PreviewVersion != null)
                    {
                        if (!SemanticVersion.TryParse(data.PreviewVersion, out ISemanticVersion version))
                        {
                            errors.Add($"The update key '{updateKey}' matches a mod with invalid optional semantic version '{data.PreviewVersion}'.");
                            continue;
                        }

                        if (result.PreviewVersion == null || version.IsNewerThan(new SemanticVersion(data.PreviewVersion)))
                        {
                            result.Name = result.Name ?? data.Name;
                            result.PreviewUrl = data.Url;
                            result.PreviewVersion = version.ToString();
                        }
                    }
                }

                // fallback to preview if latest is invalid
                if (result.Version == null && result.PreviewVersion != null)
                {
                    result.Version = result.PreviewVersion;
                    result.Url = result.PreviewUrl;
                    result.PreviewVersion = null;
                    result.PreviewUrl = null;
                }

                // special cases
                if (mod.ID == "Pathoschild.SMAPI")
                {
                    result.Name = "SMAPI";
                    result.Url = "https://smapi.io/";
                    if (result.PreviewUrl != null)
                        result.PreviewUrl = "https://smapi.io/";
                }

                // add result
                result.Errors = errors.ToArray();
                mods[mod.ID] = result;
            }

            return mods;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse a namespaced mod ID.</summary>
        /// <param name="raw">The raw mod ID to parse.</param>
        /// <param name="vendorKey">The parsed vendor key.</param>
        /// <param name="modID">The parsed mod ID.</param>
        /// <returns>Returns whether the value could be parsed.</returns>
        private bool TryParseModKey(string raw, out string vendorKey, out string modID)
        {
            // split parts
            string[] parts = raw?.Split(':');
            if (parts == null || parts.Length != 2)
            {
                vendorKey = null;
                modID = null;
                return false;
            }

            // parse
            vendorKey = parts[0].Trim();
            modID = parts[1].Trim();
            return true;
        }

        /// <summary>Get the mods for which the API should return data.</summary>
        /// <param name="model">The search model.</param>
        private IEnumerable<ModSearchEntryModel> GetSearchMods(ModSearchModel model)
        {
            if (model == null)
                yield break;

            // yield standard entries
            if (model.Mods != null)
            {
                foreach (ModSearchEntryModel mod in model.Mods)
                    yield return mod;
            }

            // yield mod update keys if backwards compatible
            if (model.ModKeys != null && model.ModKeys.Any() && this.ShouldBeBackwardsCompatible("2.6-beta.17"))
            {
                foreach (string updateKey in model.ModKeys.Distinct())
                    yield return new ModSearchEntryModel(updateKey, new[] { updateKey });
            }
        }

        /// <summary>Get the mod info for an update key.</summary>
        /// <param name="updateKey">The namespaced update key.</param>
        private async Task<ModInfoModel> GetInfoForUpdateKeyAsync(string updateKey)
        {
            // parse update key
            if (!this.TryParseModKey(updateKey, out string vendorKey, out string modID))
                return new ModInfoModel($"The update key '{updateKey}' isn't in a valid format. It should contain the site key and mod ID like 'Nexus:541'.");

            // get matching repository
            if (!this.Repositories.TryGetValue(vendorKey, out IModRepository repository))
                return new ModInfoModel($"There's no mod site with key '{vendorKey}'. Expected one of [{string.Join(", ", this.Repositories.Keys)}].");

            // fetch mod info
            return await this.Cache.GetOrCreateAsync($"{repository.VendorKey}:{modID}".ToLower(), async entry =>
            {
                ModInfoModel result = await repository.GetModInfoAsync(modID);
                if (result.Error != null)
                {
                    if (result.Version == null)
                        result.Error = $"The update key '{updateKey}' matches a mod with no version number.";
                    else if (!Regex.IsMatch(result.Version, this.VersionRegex, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
                        result.Error = $"The update key '{updateKey}' matches a mod with invalid semantic version '{result.Version}'.";
                }
                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(result.Error == null ? this.SuccessCacheMinutes : this.ErrorCacheMinutes);
                return result;
            });
        }

        /// <summary>Get whether the API should return data in a backwards compatible way.</summary>
        /// <param name="maxVersion">The last version for which data should be backwards compatible.</param>
        private bool ShouldBeBackwardsCompatible(string maxVersion)
        {
            string actualVersion = (string)this.RouteData.Values["version"];
            return !new SemanticVersion(actualVersion).IsNewerThan(new SemanticVersion(maxVersion));
        }
    }
}
