namespace StardewModdingAPI.Framework.Content
{
    /// <summary>A set of operations to apply to an asset for a given <see cref="IAssetEditor"/> or <see cref="IAssetLoader"/> implementation.</summary>
    /// <param name="Mod">The mod applying the changes.</param>
    /// <param name="LoadOperations">The load operations to apply.</param>
    /// <param name="EditOperations">The edit operations to apply.</param>
    internal record AssetOperationGroup(IModMetadata Mod, AssetLoadOperation[] LoadOperations, AssetEditOperation[] EditOperations);
}
