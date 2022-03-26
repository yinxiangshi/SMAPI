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

        /// <summary>The content pack on whose behalf the edit is being applied, if any.</summary>
        public IModMetadata OnBehalfOf { get; }

        /// <summary>Apply the edit to an asset.</summary>
        public Action<IAssetData> ApplyEdit { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The mod applying the edit.</param>
        /// <param name="onBehalfOf">The content pack on whose behalf the edit is being applied, if any.</param>
        /// <param name="applyEdit">Apply the edit to an asset.</param>
        public AssetEditOperation(IModMetadata mod, IModMetadata onBehalfOf, Action<IAssetData> applyEdit)
        {
            this.Mod = mod;
            this.OnBehalfOf = onBehalfOf;
            this.ApplyEdit = applyEdit;
        }
    }
}
