using System;
using System.Collections.Generic;

namespace StardewModdingAPI
{
    /// <summary>Encapsulates access and changes to dictionary content being read from a data file.</summary>
    public interface IContentEventHelperForDictionary<TKey, TValue>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The content's locale code, if the content is localised.</summary>
        string Locale { get; }

        /// <summary>The normalised asset name being read. The format may change between platforms; see <see cref="IsAssetName"/> to compare with a known path.</summary>
        string AssetName { get; }

        /// <summary>The content data being read.</summary>
        IDictionary<TKey, TValue> Data { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the asset name being loaded matches a given name after normalisation.</summary>
        /// <param name="path">The expected asset path, relative to the game's content folder and without the .xnb extension or locale suffix (like 'Data\ObjectInformation').</param>
        bool IsAssetName(string path);

        /// <summary>Add or replace an entry in the dictionary data.</summary>
        /// <param name="key">The entry key.</param>
        /// <param name="value">The entry value.</param>
        void SetEntry(TKey key, TValue value);

        /// <summary>Add or replace an entry in the dictionary data.</summary>
        /// <param name="key">The entry key.</param>
        /// <param name="value">A callback which accepts the current value and returns the new value.</param>
        void SetEntry(TKey key, Func<TValue, TValue> value);

        /// <summary>Replace the entire content value with the given value. This is generally not recommended, since it may break compatibility with other mods or different versions of the game.</summary>
        /// <param name="value">The new content value.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is null.</exception>
        void ReplaceWith(IDictionary<TKey, TValue> value);
    }
}
