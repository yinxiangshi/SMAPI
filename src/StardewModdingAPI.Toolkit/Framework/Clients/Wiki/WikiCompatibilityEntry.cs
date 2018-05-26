namespace StardewModdingAPI.Toolkit.Framework.Clients.Wiki
{
    /// <summary>An entry in the mod compatibility list.</summary>
    public class WikiCompatibilityEntry
    {
        /// <summary>The mod's unique ID. A mod may have multiple current IDs in rare cases (e.g. due to parallel releases or unofficial updates).</summary>
        public string[] ID { get; set; }

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

        /// <summary>The version of the latest unofficial update, if applicable.</summary>
        public ISemanticVersion UnofficialVersion { get; set; }

        /// <summary>The compatibility status.</summary>
        public WikiCompatibilityStatus Status { get; set; }
    }
}
