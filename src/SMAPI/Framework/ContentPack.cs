using System;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Toolkit.Serialisation;
using StardewModdingAPI.Toolkit.Utilities;
using xTile;

namespace StardewModdingAPI.Framework
{
    /// <summary>Manages access to a content pack's metadata and files.</summary>
    internal class ContentPack : IContentPack
    {
        /*********
        ** Properties
        *********/
        /// <summary>Provides an API for loading content assets.</summary>
        private readonly IContentHelper Content;

        /// <summary>Encapsulates SMAPI's JSON file parsing.</summary>
        private readonly JsonHelper JsonHelper;


        /*********
        ** Accessors
        *********/
        /// <summary>The full path to the content pack's folder.</summary>
        public string DirectoryPath { get; }

        /// <summary>The content pack's manifest.</summary>
        public IManifest Manifest { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="directoryPath">The full path to the content pack's folder.</param>
        /// <param name="manifest">The content pack's manifest.</param>
        /// <param name="content">Provides an API for loading content assets.</param>
        /// <param name="jsonHelper">Encapsulates SMAPI's JSON file parsing.</param>
        public ContentPack(string directoryPath, IManifest manifest, IContentHelper content, JsonHelper jsonHelper)
        {
            this.DirectoryPath = directoryPath;
            this.Manifest = manifest;
            this.Content = content;
            this.JsonHelper = jsonHelper;
        }

        /// <summary>Read a JSON file from the content pack folder.</summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="path">The file path relative to the contnet directory.</param>
        /// <returns>Returns the deserialised model, or <c>null</c> if the file doesn't exist or is empty.</returns>
        public TModel ReadJsonFile<TModel>(string path) where TModel : class
        {
            path = Path.Combine(this.DirectoryPath, PathUtilities.NormalisePathSeparators(path));
            return this.JsonHelper.ReadJsonFile<TModel>(path);
        }

        /// <summary>Load content from the content pack folder (if not already cached), and return it. When loading a <c>.png</c> file, this must be called outside the game's draw loop.</summary>
        /// <typeparam name="T">The expected data type. The main supported types are <see cref="Map"/>, <see cref="Texture2D"/>, and dictionaries; other types may be supported by the game's content pipeline.</typeparam>
        /// <param name="key">The local path to a content file relative to the content pack folder.</param>
        /// <exception cref="ArgumentException">The <paramref name="key"/> is empty or contains invalid characters.</exception>
        /// <exception cref="ContentLoadException">The content asset couldn't be loaded (e.g. because it doesn't exist).</exception>
        public T LoadAsset<T>(string key)
        {
            return this.Content.Load<T>(key, ContentSource.ModFolder);
        }

        /// <summary>Get the underlying key in the game's content cache for an asset. This can be used to load custom map tilesheets, but should be avoided when you can use the content API instead. This does not validate whether the asset exists.</summary>
        /// <param name="key">The the local path to a content file relative to the content pack folder.</param>
        /// <exception cref="ArgumentException">The <paramref name="key"/> is empty or contains invalid characters.</exception>
        public string GetActualAssetKey(string key)
        {
            return this.Content.GetActualAssetKey(key, ContentSource.ModFolder);
        }

    }
}
