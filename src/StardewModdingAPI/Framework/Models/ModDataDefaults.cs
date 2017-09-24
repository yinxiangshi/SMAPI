namespace StardewModdingAPI.Framework.Models
{
    /// <summary>Default values for support fields to inject into the manifest.</summary>
    internal class ModDataDefaults
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod's unique ID in the Chucklefish mod site (if any), used for update checks.</summary>
        public string ChucklefishID { get; set; }

        /// <summary>The mod's unique ID in Nexus Mods (if any), used for update checks.</summary>
        public string NexusID { get; set; }

        /// <summary>The mod's organisation and project name on GitHub (if any), used for update checks.</summary>
        public string GitHubProject { get; set; }
    }
}
