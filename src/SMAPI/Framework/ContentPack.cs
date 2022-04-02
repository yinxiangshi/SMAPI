using System;
using System.IO;
using StardewModdingAPI.Framework.ModHelpers;
using StardewModdingAPI.Toolkit.Serialization;
using StardewModdingAPI.Utilities;

namespace StardewModdingAPI.Framework
{
    /// <summary>Manages access to a content pack's metadata and files.</summary>
    internal class ContentPack : IContentPack
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates SMAPI's JSON file parsing.</summary>
        private readonly JsonHelper JsonHelper;

        /// <summary>A case-insensitive lookup of relative paths within the <see cref="DirectoryPath"/>.</summary>
        private readonly CaseInsensitivePathCache RelativePathCache;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public string DirectoryPath { get; }

        /// <inheritdoc />
        public IManifest Manifest { get; }

        /// <inheritdoc />
        public ITranslationHelper Translation => this.TranslationImpl;

        /// <inheritdoc />
        public IModContentHelper ModContent { get; }

        /// <summary>The underlying translation helper.</summary>
        internal TranslationHelper TranslationImpl { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="directoryPath">The full path to the content pack's folder.</param>
        /// <param name="manifest">The content pack's manifest.</param>
        /// <param name="content">Provides an API for loading content assets from the content pack's folder.</param>
        /// <param name="translation">Provides translations stored in the content pack's <c>i18n</c> folder.</param>
        /// <param name="jsonHelper">Encapsulates SMAPI's JSON file parsing.</param>
        /// <param name="relativePathCache">A case-insensitive lookup of relative paths within the <paramref name="directoryPath"/>.</param>
        public ContentPack(string directoryPath, IManifest manifest, IModContentHelper content, TranslationHelper translation, JsonHelper jsonHelper, CaseInsensitivePathCache relativePathCache)
        {
            this.DirectoryPath = directoryPath;
            this.Manifest = manifest;
            this.ModContent = content;
            this.TranslationImpl = translation;
            this.JsonHelper = jsonHelper;
            this.RelativePathCache = relativePathCache;
        }

        /// <inheritdoc />
        public bool HasFile(string path)
        {
            path = PathUtilities.NormalizePath(path);

            return this.GetFile(path).Exists;
        }

        /// <inheritdoc />
        public TModel ReadJsonFile<TModel>(string path) where TModel : class
        {
            path = PathUtilities.NormalizePath(path);

            FileInfo file = this.GetFile(path);
            return file.Exists && this.JsonHelper.ReadJsonFileIfExists(file.FullName, out TModel model)
                ? model
                : null;
        }

        /// <inheritdoc />
        public void WriteJsonFile<TModel>(string path, TModel data) where TModel : class
        {
            path = PathUtilities.NormalizePath(path);

            FileInfo file = this.GetFile(path, out path);
            this.JsonHelper.WriteJsonFile(file.FullName, data);

            this.RelativePathCache.Add(path);
        }

        /// <inheritdoc />
        [Obsolete]
        public T LoadAsset<T>(string key)
        {
            return this.ModContent.Load<T>(key);
        }

        /// <inheritdoc />
        [Obsolete]
        public string GetActualAssetKey(string key)
        {
            return this.ModContent.GetInternalAssetName(key)?.Name;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the underlying file info.</summary>
        /// <param name="relativePath">The normalized file path relative to the content pack directory.</param>
        private FileInfo GetFile(string relativePath)
        {
            return this.GetFile(relativePath, out _);
        }

        /// <summary>Get the underlying file info.</summary>
        /// <param name="relativePath">The normalized file path relative to the content pack directory.</param>
        /// <param name="actualRelativePath">The relative path after case-insensitive matching.</param>
        private FileInfo GetFile(string relativePath, out string actualRelativePath)
        {
            if (!PathUtilities.IsSafeRelativePath(relativePath))
                throw new InvalidOperationException($"You must call {nameof(IContentPack)} methods with a relative path.");

            actualRelativePath = this.RelativePathCache.GetFilePath(relativePath);

            return new FileInfo(Path.Combine(this.DirectoryPath, actualRelativePath));
        }
    }
}
