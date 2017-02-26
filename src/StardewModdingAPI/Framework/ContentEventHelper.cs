using System;
using System.Collections.Generic;
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
