using System;

namespace StardewModdingAPI
{
    /// <summary>Encapsulates access and changes to content being read from a data file.</summary>
    public interface IContentEventHelper
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The normalised asset path being read. The format may change between platforms; see <see cref="IsPath"/> to compare with a known path.</summary>
        string Path { get; }

        /// <summary>The content data being read.</summary>
        object Data { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the asset path being loaded matches a given path after normalisation.</summary>
        /// <param name="path">The expected asset path, relative to the game folder and without the .xnb extension (like 'Data\ObjectInformation').</param>
        /// <param name="matchLocalisedVersion">Whether to match a localised version of the asset file (like 'Data\ObjectInformation.ja-JP').</param>
        bool IsPath(string path, bool matchLocalisedVersion = true);

        /// <summary>Get the data as a given type.</summary>
        /// <typeparam name="TData">The expected data type.</typeparam>
        /// <exception cref="InvalidCastException">The data can't be converted to <typeparamref name="TData"/>.</exception>
        TData GetData<TData>();

        /// <summary>Add or replace an entry in the dictionary data.</summary>
        /// <typeparam name="TKey">The entry key type.</typeparam>
        /// <typeparam name="TValue">The entry value type.</typeparam>
        /// <param name="key">The entry key.</param>
        /// <param name="value">The entry value.</param>
        /// <exception cref="InvalidOperationException">The content being read isn't a dictionary.</exception>
        void SetDictionaryEntry<TKey, TValue>(TKey key, TValue value);

        /// <summary>Add or replace an entry in the dictionary data.</summary>
        /// <typeparam name="TKey">The entry key type.</typeparam>
        /// <typeparam name="TValue">The entry value type.</typeparam>
        /// <param name="key">The entry key.</param>
        /// <param name="value">A callback which accepts the current value and returns the new value.</param>
        /// <exception cref="InvalidOperationException">The content being read isn't a dictionary.</exception>
        void SetDictionaryEntry<TKey, TValue>(TKey key, Func<TValue, TValue> value);

        /// <summary>Replace the entire content value with the given value. This is generally not recommended, since it may break compatibility with other mods or different versions of the game.</summary>
        /// <param name="value">The new content value.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is null.</exception>
        /// <exception cref="InvalidCastException">The <paramref name="value"/>'s type is not compatible with the loaded asset's type.</exception>
        void ReplaceWith(object value);
    }
}
