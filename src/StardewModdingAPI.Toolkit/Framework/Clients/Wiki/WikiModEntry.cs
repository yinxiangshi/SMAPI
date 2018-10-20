namespace StardewModdingAPI.Toolkit.Framework.Clients.Wiki
{
    /// <summary>A mod entry in the wiki list.</summary>
    public class WikiModEntry
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod's unique ID. A mod may have multiple current IDs in rare cases (e.g. due to parallel releases or unofficial updates).</summary>
        public string[] ID { get; set; }

        /// <summary>The mod's display name.</summary>
        public string Name { get; set; }

        /// <summary>The mod's alternative names, if any.</summary>
        public string AlternateNames { get; set; }

        /// <summary>The mod's author name.</summary>
        public string Author { get; set; }

        /// <summary>The mod's alternative author names, if any.</summary>
        public string AlternateAuthors { get; set; }

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

        /// <summary>The game or SMAPI version which broke this mod (if applicable).</summary>
        public string BrokeIn { get; set; }

        /// <summary>The mod's compatibility with the latest stable version of the game.</summary>
        public WikiCompatibilityInfo Compatibility { get; set; }

        /// <summary>The mod's compatibility with the latest beta version of the game (if any).</summary>
        public WikiCompatibilityInfo BetaCompatibility { get; set; }

        /// <summary>Whether a Stardew Valley or SMAPI beta which affects mod compatibility is in progress. If this is true, <see cref="BetaCompatibility"/> should be used for beta versions of SMAPI instead of <see cref="Compatibility"/>.</summary>
        public bool HasBetaInfo => this.BetaCompatibility != null;

        /// <summary>The link anchor for the mod entry in the wiki compatibility list.</summary>
        public string Anchor { get; set; }
    }
}
