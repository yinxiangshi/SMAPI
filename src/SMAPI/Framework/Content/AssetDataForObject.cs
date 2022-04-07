#nullable disable

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using xTile;

namespace StardewModdingAPI.Framework.Content
{
    /// <summary>Encapsulates access and changes to content being read from a data file.</summary>
    internal class AssetDataForObject : AssetData<object>, IAssetData
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="locale">The content's locale code, if the content is localized.</param>
        /// <param name="assetName">The asset name being read.</param>
        /// <param name="data">The content data being read.</param>
        /// <param name="getNormalizedPath">Normalizes an asset key to match the cache key.</param>
        public AssetDataForObject(string locale, IAssetName assetName, object data, Func<string, string> getNormalizedPath)
            : base(locale, assetName, data, getNormalizedPath, onDataReplaced: null) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="info">The asset metadata.</param>
        /// <param name="data">The content data being read.</param>
        /// <param name="getNormalizedPath">Normalizes an asset key to match the cache key.</param>
        public AssetDataForObject(IAssetInfo info, object data, Func<string, string> getNormalizedPath)
            : this(info.Locale, info.Name, data, getNormalizedPath) { }

        /// <inheritdoc />
        public IAssetDataForDictionary<TKey, TValue> AsDictionary<TKey, TValue>()
        {
            return new AssetDataForDictionary<TKey, TValue>(this.Locale, this.Name, this.GetData<IDictionary<TKey, TValue>>(), this.GetNormalizedPath, this.ReplaceWith);
        }

        /// <inheritdoc />
        public IAssetDataForImage AsImage()
        {
            return new AssetDataForImage(this.Locale, this.Name, this.GetData<Texture2D>(), this.GetNormalizedPath, this.ReplaceWith);
        }

        /// <inheritdoc />
        public IAssetDataForMap AsMap()
        {
            return new AssetDataForMap(this.Locale, this.Name, this.GetData<Map>(), this.GetNormalizedPath, this.ReplaceWith);
        }

        /// <inheritdoc />
        public TData GetData<TData>()
        {
            if (this.Data is not TData data)
                throw new InvalidCastException($"The content data of type {this.Data.GetType().FullName} can't be converted to the requested {typeof(TData).FullName}.");
            return data;
        }
    }
}
