using System;

namespace StardewModdingAPI.Framework.Content
{
    /// <summary>An operation which provides the initial instance of an asset when it's requested from the content pipeline.</summary>
    internal class AssetLoadOperation
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod loading the asset.</summary>
        public IModMetadata Mod { get; }

        /// <summary>The content pack on whose behalf the asset is being loaded, if any.</summary>
        public IModMetadata OnBehalfOf { get; }

        /// <summary>Load the initial value for an asset.</summary>
        public Func<IAssetInfo, object> GetData { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The mod applying the edit.</param>
        /// <param name="onBehalfOf">The content pack on whose behalf the asset is being loaded, if any.</param>
        /// <param name="getData">Load the initial value for an asset.</param>
        public AssetLoadOperation(IModMetadata mod, IModMetadata onBehalfOf, Func<IAssetInfo, object> getData)
        {
            this.Mod = mod;
            this.OnBehalfOf = onBehalfOf;
            this.GetData = getData;
        }
    }
}
