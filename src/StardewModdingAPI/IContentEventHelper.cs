using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewModdingAPI
{
    /// <summary>Encapsulates access and changes to content being read from a data file.</summary>
    public interface IContentEventHelper
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The content's locale code, if the content is localised.</summary>
        string Locale { get; }

        /// <summary>The normalised asset name being read. The format may change between platforms; see <see cref="IsAssetName"/> to compare with a known path.</summary>
        string AssetName { get; }

        /// <summary>The content data being read.</summary>
        object Data { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the asset name being loaded matches a given name after normalisation.</summary>
        /// <param name="path">The expected asset path, relative to the game's content folder and without the .xnb extension or locale suffix (like 'Data\ObjectInformation').</param>
        bool IsAssetName(string path);

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

        /// <summary>Overwrite part of the image.</summary>
        /// <param name="source">The image to patch into the content.</param>
        /// <param name="sourceArea">The part of the <paramref name="source"/> to copy (or <c>null</c> to take the whole texture). This must be within the bounds of the <paramref name="source"/> texture.</param>
        /// <param name="targetArea">The part of the content to patch (or <c>null</c> to patch the whole texture). The original content within this area will be erased. This must be within the bounds of the existing spritesheet.</param>
        /// <param name="patchMode">Indicates how an image should be patched.</param>
        /// <exception cref="ArgumentNullException">One of the arguments is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="targetArea"/> is outside the bounds of the spritesheet.</exception>
        /// <exception cref="InvalidOperationException">The content being read isn't an image.</exception>
        void PatchImage(Texture2D source, Rectangle? sourceArea = null, Rectangle? targetArea = null, PatchMode patchMode = PatchMode.Replace);

        /// <summary>Replace the entire content value with the given value. This is generally not recommended, since it may break compatibility with other mods or different versions of the game.</summary>
        /// <param name="value">The new content value.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is null.</exception>
        /// <exception cref="InvalidCastException">The <paramref name="value"/>'s type is not compatible with the loaded asset's type.</exception>
        void ReplaceWith(object value);
    }
}
