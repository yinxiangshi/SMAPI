namespace StardewModdingAPI.Web.Framework.Clients.Chucklefish
{
    /// <summary>Mod metadata from the Chucklefish mod site.</summary>
    internal class ChucklefishMod
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod name.</summary>
        public string Name { get; set; }

        /// <summary>The mod's semantic version number.</summary>
        public string Version { get; set; }

        /// <summary>The mod's web URL.</summary>
        public string Url { get; set; }
    }
}
