using System;
using System.Collections.Generic;

namespace StardewModdingAPI.Framework.Content
{
    /// <summary>Encapsulates access and changes to dictionary content being read from a data file.</summary>
    internal class ContentEventHelperForDictionary<TKey, TValue> : ContentEventBaseHelper<IDictionary<TKey, TValue>>, IContentEventHelperForDictionary<TKey, TValue>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="locale">The content's locale code, if the content is localised.</param>
        /// <param name="assetName">The normalised asset name being read.</param>
        /// <param name="data">The content data being read.</param>
        /// <param name="getNormalisedPath">Normalises an asset key to match the cache key.</param>
        public ContentEventHelperForDictionary(string locale, string assetName, IDictionary<TKey, TValue> data, Func<string, string> getNormalisedPath)
            : base(locale, assetName, data, getNormalisedPath) { }

        /// <summary>Add or replace an entry in the dictionary data.</summary>
        /// <param name="key">The entry key.</param>
        /// <param name="value">The entry value.</param>
        public void SetEntry(TKey key, TValue value)
        {
            this.Data[key] = value;
        }

        /// <summary>Add or replace an entry in the dictionary data.</summary>
        /// <param name="key">The entry key.</param>
        /// <param name="value">A callback which accepts the current value and returns the new value.</param>
        public void SetEntry(TKey key, Func<TValue, TValue> value)
        {
            this.Data[key] = value(this.Data[key]);
        }
    }
}
