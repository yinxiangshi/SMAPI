using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewModdingAPI.Framework
{
    /// <summary>Encapsulates access and changes to content being read from a data file.</summary>
    internal class ContentEventHelper : EventArgs, IContentEventHelper
    {
        /*********
        ** Properties
        *********/
        /// <summary>Normalises an asset key to match the cache key.</summary>
        private readonly Func<string, string> GetNormalisedPath;


        /*********
        ** Accessors
        *********/
        /// <summary>The content's locale code, if the content is localised.</summary>
        public string Locale { get; }

        /// <summary>The normalised asset name being read. The format may change between platforms; see <see cref="IsAssetName"/> to compare with a known path.</summary>
        public string AssetName { get; }

        /// <summary>The content data being read.</summary>
        public object Data { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="locale">The content's locale code, if the content is localised.</param>
        /// <param name="assetName">The normalised asset name being read.</param>
        /// <param name="data">The content data being read.</param>
        /// <param name="getNormalisedPath">Normalises an asset key to match the cache key.</param>
        public ContentEventHelper(string locale, string assetName, object data, Func<string, string> getNormalisedPath)
        {
            this.Locale = locale;
            this.AssetName = assetName;
            this.Data = data;
            this.GetNormalisedPath = getNormalisedPath;
        }

        /// <summary>Get whether the asset name being loaded matches a given name after normalisation.</summary>
        /// <param name="path">The expected asset path, relative to the game's content folder and without the .xnb extension or locale suffix (like 'Data\ObjectInformation').</param>
        public bool IsAssetName(string path)
        {
            path = this.GetNormalisedPath(path);
            return this.AssetName.Equals(path, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>Get the data as a given type.</summary>
        /// <typeparam name="TData">The expected data type.</typeparam>
        /// <exception cref="InvalidCastException">The data can't be converted to <typeparamref name="TData"/>.</exception>
        public TData GetData<TData>()
        {
            if (!(this.Data is TData))
                throw new InvalidCastException($"The content data of type {this.Data.GetType().FullName} can't be converted to the requested {typeof(TData).FullName}.");
            return (TData)this.Data;
        }

        /// <summary>Add or replace an entry in the dictionary data.</summary>
        /// <typeparam name="TKey">The entry key type.</typeparam>
        /// <typeparam name="TValue">The entry value type.</typeparam>
        /// <param name="key">The entry key.</param>
        /// <param name="value">The entry value.</param>
        /// <exception cref="InvalidOperationException">The content being read isn't a dictionary.</exception>
        public void SetDictionaryEntry<TKey, TValue>(TKey key, TValue value)
        {
            IDictionary<TKey, TValue> data = this.GetData<Dictionary<TKey, TValue>>();
            data[key] = value;
        }

        /// <summary>Add or replace an entry in the dictionary data.</summary>
        /// <typeparam name="TKey">The entry key type.</typeparam>
        /// <typeparam name="TValue">The entry value type.</typeparam>
        /// <param name="key">The entry key.</param>
        /// <param name="value">A callback which accepts the current value and returns the new value.</param>
        /// <exception cref="InvalidOperationException">The content being read isn't a dictionary.</exception>
        public void SetDictionaryEntry<TKey, TValue>(TKey key, Func<TValue, TValue> value)
        {
            IDictionary<TKey, TValue> data = this.GetData<Dictionary<TKey, TValue>>();
            data[key] = value(data[key]);
        }

        /// <summary>Overwrite part of the image.</summary>
        /// <param name="source">The image to patch into the content.</param>
        /// <param name="sourceArea">The part of the <paramref name="source"/> to copy (or <c>null</c> to take the whole texture). This must be within the bounds of the <paramref name="source"/> texture.</param>
        /// <param name="targetArea">The part of the content to patch (or <c>null</c> to patch the whole texture). The original content within this area will be erased. This must be within the bounds of the existing spritesheet.</param>
        /// <param name="patchMode">Indicates how an image should be patched.</param>
        /// <exception cref="ArgumentNullException">One of the arguments is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="targetArea"/> is outside the bounds of the spritesheet.</exception>
        /// <exception cref="InvalidOperationException">The content being read isn't an image.</exception>
        public void PatchImage(Texture2D source, Rectangle? sourceArea = null, Rectangle? targetArea = null, PatchMode patchMode = PatchMode.Replace)
        {
            // get texture
            Texture2D target = this.GetData<Texture2D>();

            // get areas
            sourceArea = sourceArea ?? new Rectangle(0, 0, source.Width, source.Height);
            targetArea = targetArea ?? new Rectangle(0, 0, Math.Min(sourceArea.Value.Width, target.Width), Math.Min(sourceArea.Value.Height, target.Height));

            // validate
            if (source == null)
                throw new ArgumentNullException(nameof(source), "Can't patch from a null source texture.");
            if (sourceArea.Value.X < 0 || sourceArea.Value.Y < 0 || sourceArea.Value.Right > source.Width || sourceArea.Value.Bottom > source.Height)
                throw new ArgumentOutOfRangeException(nameof(sourceArea), "The source area is outside the bounds of the source texture.");
            if (targetArea.Value.X < 0 || targetArea.Value.Y < 0 || targetArea.Value.Right > target.Width || targetArea.Value.Bottom > target.Height)
                throw new ArgumentOutOfRangeException(nameof(targetArea), "The target area is outside the bounds of the target texture.");
            if (sourceArea.Value.Width != targetArea.Value.Width || sourceArea.Value.Height != targetArea.Value.Height)
                throw new InvalidOperationException("The source and target areas must be the same size.");

            // get source data
            int pixelCount = sourceArea.Value.Width * sourceArea.Value.Height;
            Color[] sourceData = new Color[pixelCount];
            source.GetData(0, sourceArea, sourceData, 0, pixelCount);

            // merge data in overlay mode
            if (patchMode == PatchMode.Overlay)
            {
                Color[] newData = new Color[targetArea.Value.Width * targetArea.Value.Height];
                target.GetData(0, targetArea, newData, 0, newData.Length);
                for (int i = 0; i < sourceData.Length; i++)
                {
                    Color pixel = sourceData[i];
                    if (pixel.A != 0) // not transparent
                        newData[i] = pixel;
                }
                sourceData = newData;
            }

            // patch target texture
            target.SetData(0, targetArea, sourceData, 0, pixelCount);
        }

        /// <summary>Replace the entire content value with the given value. This is generally not recommended, since it may break compatibility with other mods or different versions of the game.</summary>
        /// <param name="value">The new content value.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is null.</exception>
        /// <exception cref="InvalidCastException">The <paramref name="value"/>'s type is not compatible with the loaded asset's type.</exception>
        public void ReplaceWith(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Can't set a loaded asset to a null value.");
            if (!this.Data.GetType().IsInstanceOfType(value))
                throw new InvalidCastException($"Can't replace loaded asset of type {this.GetFriendlyTypeName(this.Data.GetType())} with value of type {this.GetFriendlyTypeName(value.GetType())}. The new type must be compatible to prevent game errors.");

            this.Data = value;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a human-readable type name.</summary>
        /// <param name="type">The type to name.</param>
        private string GetFriendlyTypeName(Type type)
        {
            // dictionary
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type[] genericArgs = type.GetGenericArguments();
                return $"Dictionary<{this.GetFriendlyTypeName(genericArgs[0])}, {this.GetFriendlyTypeName(genericArgs[1])}>";
            }

            // texture
            if (type == typeof(Texture2D))
                return type.Name;

            // native type
            if (type == typeof(int))
                return "int";
            if (type == typeof(string))
                return "string";

            // default
            return type.FullName;
        }
    }
}
