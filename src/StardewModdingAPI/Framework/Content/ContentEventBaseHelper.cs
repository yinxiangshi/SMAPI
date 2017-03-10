using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace StardewModdingAPI.Framework.Content
{
    /// <summary>Base implementation for a content helper which encapsulates access and changes to content being read from a data file.</summary>
    /// <typeparam name="TValue">The interface value type.</typeparam>
    internal class ContentEventBaseHelper<TValue> : EventArgs, IContentEventData<TValue>
    {
        /*********
        ** Properties
        *********/
        /// <summary>Normalises an asset key to match the cache key.</summary>
        protected readonly Func<string, string> GetNormalisedPath;


        /*********
        ** Accessors
        *********/
        /// <summary>The content's locale code, if the content is localised.</summary>
        public string Locale { get; }

        /// <summary>The normalised asset name being read. The format may change between platforms; see <see cref="IsAssetName"/> to compare with a known path.</summary>
        public string AssetName { get; }

        /// <summary>The content data being read.</summary>
        public TValue Data { get; protected set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="locale">The content's locale code, if the content is localised.</param>
        /// <param name="assetName">The normalised asset name being read.</param>
        /// <param name="data">The content data being read.</param>
        /// <param name="getNormalisedPath">Normalises an asset key to match the cache key.</param>
        public ContentEventBaseHelper(string locale, string assetName, TValue data, Func<string, string> getNormalisedPath)
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

        /// <summary>Replace the entire content value with the given value. This is generally not recommended, since it may break compatibility with other mods or different versions of the game.</summary>
        /// <param name="value">The new content value.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is null.</exception>
        /// <exception cref="InvalidCastException">The <paramref name="value"/>'s type is not compatible with the loaded asset's type.</exception>
        public void ReplaceWith(TValue value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Can't set a loaded asset to a null value.");
            if (!this.Data.GetType().IsInstanceOfType(value))
                throw new InvalidCastException($"Can't replace loaded asset of type {this.GetFriendlyTypeName(this.Data.GetType())} with value of type {this.GetFriendlyTypeName(value.GetType())}. The new type must be compatible to prevent game errors.");

            this.Data = value;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Get a human-readable type name.</summary>
        /// <param name="type">The type to name.</param>
        protected string GetFriendlyTypeName(Type type)
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
