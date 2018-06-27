using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StardewModdingAPI.Toolkit.Framework.Clients.Wiki;
using StardewModdingAPI.Toolkit.Framework.ModData;

namespace StardewModdingAPI.Toolkit
{
    /// <summary>A convenience wrapper for the various tools.</summary>
    public class ModToolkit
    {
        /*********
        ** Properties
        *********/
        /// <summary>The default HTTP user agent for the toolkit.</summary>
        private readonly string UserAgent;

        /// <summary>Maps vendor keys (like <c>Nexus</c>) to their mod URL template (where <c>{0}</c> is the mod ID). This doesn't affect update checks, which defer to the remote web API.</summary>
        private readonly IDictionary<string, string> VendorModUrls = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            ["Chucklefish"] = "https://community.playstarbound.com/resources/{0}",
            ["GitHub"] = "https://github.com/{0}/releases",
            ["Nexus"] = "https://www.nexusmods.com/stardewvalley/mods/{0}"
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public ModToolkit()
        {
            ISemanticVersion version = new SemanticVersion(this.GetType().Assembly.GetName().Version);
            this.UserAgent = $"SMAPI Mod Handler Toolkit/{version}";
        }

        /// <summary>Extract mod metadata from the wiki compatibility list.</summary>
        public async Task<WikiCompatibilityEntry[]> GetWikiCompatibilityListAsync()
        {
            var client = new WikiCompatibilityClient(this.UserAgent);
            return await client.FetchAsync();
        }

        /// <summary>Get SMAPI's internal mod database.</summary>
        /// <param name="metadataPath">The file path for the SMAPI metadata file.</param>
        /// <param name="getUpdateUrl">Get an update URL for an update key (if valid).</param>
        public ModDatabase GetModDatabase(string metadataPath, Func<string, string> getUpdateUrl)
        {
            MetadataModel metadata = JsonConvert.DeserializeObject<MetadataModel>(File.ReadAllText(metadataPath));
            ModDataRecord[] records = metadata.ModData.Select(pair => new ModDataRecord(pair.Key, pair.Value)).ToArray();
            return new ModDatabase(records, getUpdateUrl);
        }

        /// <summary>Get an update URL for an update key (if valid).</summary>
        /// <param name="updateKey">The update key.</param>
        internal string GetUpdateUrl(string updateKey)
        {
            string[] parts = updateKey.Split(new[] { ':' }, 2);
            if (parts.Length != 2)
                return null;

            string vendorKey = parts[0].Trim();
            string modID = parts[1].Trim();

            if (this.VendorModUrls.TryGetValue(vendorKey, out string urlTemplate))
                return string.Format(urlTemplate, modID);

            return null;
        }
    }
}
