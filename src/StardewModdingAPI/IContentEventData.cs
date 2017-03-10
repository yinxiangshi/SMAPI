using System;

namespace StardewModdingAPI
{
    /// <summary>Generic metadata and methods for a content asset being loaded.</summary>
    /// <typeparam name="TValue">The expected data type.</typeparam>
    public interface IContentEventData<TValue>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The content's locale code, if the content is localised.</summary>
        string Locale { get; }

        /// <summary>The normalised asset name being read. The format may change between platforms; see <see cref="IsAssetName"/> to compare with a known path.</summary>
        string AssetName { get; }

        /// <summary>The content data being read.</summary>
        TValue Data { get; }

        /// <summary>The content data type.</summary>
        Type DataType { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the asset name being loaded matches a given name after normalisation.</summary>
        /// <param name="path">The expected asset path, relative to the game's content folder and without the .xnb extension or locale suffix (like 'Data\ObjectInformation').</param>
        bool IsAssetName(string path);

        /// <summary>Replace the entire content value with the given value. This is generally not recommended, since it may break compatibility with other mods or different versions of the game.</summary>
        /// <param name="value">The new content value.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is null.</exception>
        /// <exception cref="InvalidCastException">The <paramref name="value"/>'s type is not compatible with the loaded asset's type.</exception>
        void ReplaceWith(TValue value);
    }
}
