namespace StardewModdingAPI.Web.Framework.ConfigModels
{
    /// <summary>The config settings for cache storage.</summary>
    internal class StorageConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The storage mechanism to use.</summary>
        public StorageMode Mode { get; set; }

        /// <summary>The connection string for the storage mechanism, if applicable.</summary>
        public string ConnectionString { get; set; }

        /// <summary>The database name for the storage mechanism, if applicable.</summary>
        public string Database { get; set; }
    }
}
