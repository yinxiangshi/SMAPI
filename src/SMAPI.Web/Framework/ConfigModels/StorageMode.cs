namespace StardewModdingAPI.Web.Framework.ConfigModels
{
    /// <summary>Indicates a storage mechanism to use.</summary>
    internal enum StorageMode
    {
        /// <summary>Store data in a hosted MongoDB instance.</summary>
        Mongo,

        /// <summary>Store data in an in-memory MongoDB instance. This is useful for testing MongoDB storage locally, but will likely fail when deployed since it needs permission to open a local port.</summary>
        MongoInMemory,

        /// <summary>Store data in-memory. This is suitable for local testing or single-instance servers, but will cause issues when distributed across multiple servers.</summary>
        InMemory
    }
}
