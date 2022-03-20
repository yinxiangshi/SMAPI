using System;

namespace StardewModdingAPI.Framework.Content
{
    /// <summary>An edit to apply to an asset when it's requested from the content pipeline.</summary>
    internal class AssetEditOperation
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod applying the edit.</summary>
        public IModMetadata Mod { get; }

        /// <summary>Apply the edit to an asset.</summary>
        public Action<IAssetData> ApplyEdit { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The mod applying the edit.</param>
        /// <param name="applyEdit">Apply the edit to an asset.</param>
        public AssetEditOperation(IModMetadata mod, Action<IAssetData> applyEdit)
        {
            this.Mod = mod;
            this.ApplyEdit = applyEdit;
        }
    }
}
