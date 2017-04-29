using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewModdingAPI.Framework
{
    /// <summary>Provides an API for loading content assets.</summary>
    internal class ContentHelper : IContentHelper
    {
        /*********
        ** Properties
        *********/
        /// <summary>SMAPI's underlying content manager.</summary>
        private readonly SContentManager ContentManager;

        /// <summary>The absolute path to the mod folder.</summary>
        private readonly string ModFolderPath;

        /// <summary>The path to the mod's folder, relative to the game's content folder (e.g. "../Mods/ModName").</summary>
        private readonly string RelativeContentFolder;

        /// <summary>The friendly mod name for use in errors.</summary>
        private readonly string ModName;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentManager">SMAPI's underlying content manager.</param>
        /// <param name="modFolderPath">The absolute path to the mod folder.</param>
        /// <param name="modName">The friendly mod name for use in errors.</param>
        public ContentHelper(SContentManager contentManager, string modFolderPath, string modName)
        {
            this.ContentManager = contentManager;
            this.ModFolderPath = modFolderPath;
            this.ModName = modName;
            this.RelativeContentFolder = this.GetRelativePath(contentManager.FullRootDirectory, modFolderPath);
        }

        /// <summary>Fetch and cache content from the game content or mod folder (if not already cached), and return it.</summary>
        /// <typeparam name="T">The expected data type. The main supported types are <see cref="Texture2D"/> and dictionaries; other types may be supported by the game's content pipeline.</typeparam>
        /// <param name="key">The asset key to fetch (if the <paramref name="source"/> is <see cref="ContentSource.GameContent"/>), or the local path to an XNB file relative to the mod folder.</param>
        /// <param name="source">Where to search for a matching content asset.</param>
        /// <exception cref="ArgumentException">The <paramref name="key"/> is empty or contains invalid characters.</exception>
        /// <exception cref="ContentLoadException">The content asset couldn't be loaded (e.g. because it doesn't exist).</exception>
        public T Load<T>(string key, ContentSource source)
        {
            // validate
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("The asset key or local path is empty.");
            if (key.Intersect(Path.GetInvalidPathChars()).Any())
                throw new ArgumentException("The asset key or local path contains invalid characters.");

            // load content
            switch (source)
            {
                case ContentSource.GameContent:
                    return this.LoadFromGameContent<T>(key, key, source);

                case ContentSource.ModFolder:
                    // find content file
                    FileInfo file = new FileInfo(Path.Combine(this.ModFolderPath, key));
                    if (!file.Exists && file.Extension == "")
                        file = new FileInfo(Path.Combine(this.ModFolderPath, key + ".xnb"));
                    if (!file.Exists)
                        throw new ContentLoadException($"There is no file at path '{file.FullName}'.");

                    // get content-relative path
                    string contentPath = Path.Combine(this.RelativeContentFolder, key);
                    if (contentPath.EndsWith(".xnb"))
                        contentPath = contentPath.Substring(0, contentPath.Length - 4);

                    // load content
                    switch (file.Extension.ToLower())
                    {
                        case ".xnb":
                            return this.LoadFromGameContent<T>(contentPath, key, source);

                        case ".png":
                            // validate
                            if (typeof(T) != typeof(Texture2D))
                                throw new ContentLoadException($"Can't read file with extension '{file.Extension}' as type '{typeof(T)}'; must be type '{typeof(Texture2D)}'.");

                            // try cache
                            if (this.ContentManager.IsLoaded(contentPath))
                                return this.LoadFromGameContent<T>(contentPath, key, source);

                            // fetch & cache
                            using (FileStream stream = File.OpenRead(file.FullName))
                            {
                                Texture2D texture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, stream);
                                this.ContentManager.Inject(contentPath, texture);
                                return (T)(object)texture;
                            }

                        default:
                            throw new ContentLoadException($"Unknown file extension '{file.Extension}'; must be '.xnb' or '.png'.");
                    }

                default:
                    throw new NotSupportedException($"Unknown content source '{source}'.");
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Load a content asset through the underlying content manager, and throw a friendly error if it fails.</summary>
        /// <typeparam name="T">The expected data type.</typeparam>
        /// <param name="assetKey">The content key.</param>
        /// <param name="friendlyKey">The friendly content key to show in errors.</param>
        /// <param name="source">The content source for use in errors.</param>
        /// <exception cref="ContentLoadException">The content couldn't be loaded.</exception>
        private T LoadFromGameContent<T>(string assetKey, string friendlyKey, ContentSource source)
        {
            try
            {
                return this.ContentManager.Load<T>(assetKey);
            }
            catch (Exception ex)
            {
                throw new ContentLoadException($"{this.ModName} failed loading content asset '{friendlyKey}' from {source}.", ex);
            }
        }

        /// <summary>Get a directory path relative to a given root.</summary>
        /// <param name="rootPath">The root path from which the path should be relative.</param>
        /// <param name="targetPath">The target file path.</param>
        private string GetRelativePath(string rootPath, string targetPath)
        {
            // convert to URIs
            Uri from = new Uri(rootPath + "/");
            Uri to = new Uri(targetPath + "/");
            if (from.Scheme != to.Scheme)
                throw new InvalidOperationException($"Can't get path for '{targetPath}' relative to '{rootPath}'.");

            // get relative path
            return Uri.UnescapeDataString(from.MakeRelativeUri(to).ToString())
                .Replace(Path.DirectorySeparatorChar == '/' ? '\\' : '/', Path.DirectorySeparatorChar); // use correct separator for platform
        }
    }
}
