namespace StardewModdingAPI.Framework.Content
{
    /// <summary>A set of operations to apply to an asset for a given <see cref="IAssetEditor"/> or <see cref="IAssetLoader"/> implementation.</summary>
    internal class AssetOperationGroup
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod applying the changes.</summary>
        public IModMetadata Mod { get; }

        /// <summary>The load operations to apply.</summary>
        public AssetLoadOperation[] LoadOperations { get; }

        /// <summary>The edit operations to apply.</summary>
        public AssetEditOperation[] EditOperations { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The mod applying the changes.</param>
        /// <param name="loadOperations">The load operations to apply.</param>
        /// <param name="editOperations">The edit operations to apply.</param>
        public AssetOperationGroup(IModMetadata mod, AssetLoadOperation[] loadOperations, AssetEditOperation[] editOperations)
        {
            this.Mod = mod;
            this.LoadOperations = loadOperations;
            this.EditOperations = editOperations;
        }
    }
}
