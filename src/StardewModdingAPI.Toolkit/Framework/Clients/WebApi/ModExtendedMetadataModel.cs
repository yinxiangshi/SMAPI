using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StardewModdingAPI.Toolkit.Framework.Clients.Wiki;
using StardewModdingAPI.Toolkit.Framework.ModData;

namespace StardewModdingAPI.Toolkit.Framework.Clients.WebApi
{
    /// <summary>Extended metadata about a mod.</summary>
    public class ModExtendedMetadataModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod's unique ID. A mod may have multiple current IDs in rare cases (e.g. due to parallel releases or unofficial updates).</summary>
        public string[] ID { get; set; } = new string[0];

        /// <summary>The mod's display name.</summary>
        public string Name { get; set; }

        /// <summary>The mod ID on Nexus.</summary>
        public int? NexusID { get; set; }

        /// <summary>The mod ID in the Chucklefish mod repo.</summary>
        public int? ChucklefishID { get; set; }

        /// <summary>The GitHub repository in the form 'owner/repo'.</summary>
        public string GitHubRepo { get; set; }

        /// <summary>The URL to a non-GitHub source repo.</summary>
        public string CustomSourceUrl { get; set; }

        /// <summary>The custom mod page URL (if applicable).</summary>
        public string CustomUrl { get; set; }

        /// <summary>The compatibility status.</summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public WikiCompatibilityStatus? CompatibilityStatus { get; set; }

        /// <summary>The human-readable summary of the compatibility status or workaround, without HTML formatitng.</summary>
        public string CompatibilitySummary { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public ModExtendedMetadataModel() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="wiki">The mod metadata from the wiki (if available).</param>
        /// <param name="db">The mod metadata from SMAPI's internal DB (if available).</param>
        public ModExtendedMetadataModel(WikiCompatibilityEntry wiki, ModDataRecord db)
        {
            // wiki data
            if (wiki != null)
            {
                this.ID = wiki.ID;
                this.Name = wiki.Name;
                this.NexusID = wiki.NexusID;
                this.ChucklefishID = wiki.ChucklefishID;
                this.GitHubRepo = wiki.GitHubRepo;
                this.CustomSourceUrl = wiki.CustomSourceUrl;
                this.CustomUrl = wiki.CustomUrl;
                this.CompatibilityStatus = wiki.Status;
                this.CompatibilitySummary = wiki.Summary;
            }

            // internal DB data
            if (db != null)
            {
                this.ID = this.ID.Union(db.FormerIDs).ToArray();
                this.Name = this.Name ?? db.DisplayName;
            }
        }
    }
}
