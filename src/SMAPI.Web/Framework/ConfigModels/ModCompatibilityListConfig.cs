namespace StardewModdingAPI.Web.Framework.ConfigModels
{
    /// <summary>The config settings for mod compatibility list.</summary>
    internal class ModCompatibilityListConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The number of minutes data from the wiki should be cached before refetching it.</summary>
        public int CacheMinutes { get; set; }
    }
}
