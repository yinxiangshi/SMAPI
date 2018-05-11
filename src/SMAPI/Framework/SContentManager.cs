using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Framework.Content;
using StardewModdingAPI.Framework.Exceptions;
using StardewModdingAPI.Framework.Reflection;
using StardewModdingAPI.Framework.Utilities;
using StardewValley;

namespace StardewModdingAPI.Framework
{
    /// <summary>A minimal content manager which defers to SMAPI's core content logic.</summary>
    internal class SContentManager : LocalizedContentManager
    {
        /*********
        ** Properties
        *********/
        /// <summary>The central coordinator which manages content managers.</summary>
        private readonly ContentCoordinator Coordinator;

        /// <summary>The underlying asset cache.</summary>
        private readonly ContentCache Cache;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>A lookup which indicates whether the asset is localisable (i.e. the filename contains the locale), if previously loaded.</summary>
        private readonly IDictionary<string, bool> IsLocalisableLookup;

        /// <summary>The language enum values indexed by locale code.</summary>
        private readonly IDictionary<string, LocalizedContentManager.LanguageCode> LanguageCodes;

        /// <summary>The assets currently being intercepted by <see cref="IAssetLoader"/> instances. This is used to prevent infinite loops when a loader loads a new asset.</summary>
        private readonly ContextHash<string> AssetsBeingLoaded = new ContextHash<string>();

        /// <summary>The path prefix for assets in mod folders.</summary>
        private readonly string ModContentPrefix;

        /// <summary>A callback to invoke when the content manager is being disposed.</summary>
        private readonly Action<SContentManager> OnDisposing;

        /// <summary>Interceptors which provide the initial versions of matching assets.</summary>
        private IDictionary<IModMetadata, IList<IAssetLoader>> Loaders => this.Coordinator.Loaders;

        /// <summary>Interceptors which edit matching assets after they're loaded.</summary>
        private IDictionary<IModMetadata, IList<IAssetEditor>> Editors => this.Coordinator.Editors;

        /// <summary>Whether the content coordinator has been disposed.</summary>
        private bool IsDisposed;


        /*********
        ** Accessors
        *********/
        /// <summary>A name for the mod manager. Not guaranteed to be unique.</summary>
        public string Name { get; }

        /// <summary>Whether this content manager is wrapped around a mod folder.</summary>
        public bool IsModFolder { get; }

        /// <summary>The current language as a constant.</summary>
        public LocalizedContentManager.LanguageCode Language => this.GetCurrentLanguage();

        /// <summary>The absolute path to the <see cref="ContentManager.RootDirectory"/>.</summary>
        public string FullRootDirectory => Path.Combine(Constants.ExecutionPath, this.RootDirectory);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">A name for the mod manager. Not guaranteed to be unique.</param>
        /// <param name="serviceProvider">The service provider to use to locate services.</param>
        /// <param name="rootDirectory">The root directory to search for content.</param>
        /// <param name="currentCulture">The current culture for which to localise content.</param>
        /// <param name="coordinator">The central coordinator which manages content managers.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="isModFolder">Whether this content manager is wrapped around a mod folder.</param>
        /// <param name="onDisposing">A callback to invoke when the content manager is being disposed.</param>
        public SContentManager(string name, IServiceProvider serviceProvider, string rootDirectory, CultureInfo currentCulture, ContentCoordinator coordinator, IMonitor monitor, Reflector reflection, Action<SContentManager> onDisposing, bool isModFolder)
                : base(serviceProvider, rootDirectory, currentCulture)
        {
            // init
            this.Name = name;
            this.IsModFolder = isModFolder;
            this.Coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
            this.Cache = new ContentCache(this, reflection);
            this.Monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            this.ModContentPrefix = this.GetAssetNameFromFilePath(Constants.ModPath, ContentSource.GameContent);
            this.OnDisposing = onDisposing;

            // get asset data
            this.LanguageCodes = this.GetKeyLocales().ToDictionary(p => p.Value, p => p.Key, StringComparer.InvariantCultureIgnoreCase);
            this.IsLocalisableLookup = reflection.GetField<IDictionary<string, bool>>(this, "_localizedAsset").GetValue();

        }

        /// <summary>Load an asset that has been processed by the content pipeline.</summary>
        /// <typeparam name="T">The type of asset to load.</typeparam>
        /// <param name="assetName">The asset path relative to the loader root directory, not including the <c>.xnb</c> extension.</param>
        public override T Load<T>(string assetName)
        {
            return this.Load<T>(assetName, LocalizedContentManager.CurrentLanguageCode);
        }

        /// <summary>Load an asset that has been processed by the content pipeline.</summary>
        /// <typeparam name="T">The type of asset to load.</typeparam>
        /// <param name="assetName">The asset path relative to the loader root directory, not including the <c>.xnb</c> extension.</param>
        /// <param name="language">The language code for which to load content.</param>
        public override T Load<T>(string assetName, LanguageCode language)
        {
            // normalise asset key
            this.AssertValidAssetKeyFormat(assetName);
            assetName = this.NormaliseAssetName(assetName);

            // load game content
            if (!this.IsModFolder && !assetName.StartsWith(this.ModContentPrefix))
                return this.LoadImpl<T>(assetName, language);

            // load mod content
            SContentLoadException GetContentError(string reasonPhrase) => new SContentLoadException($"Failed loading content asset '{assetName}': {reasonPhrase}");
            try
            {
                // try cache
                if (this.IsLoaded(assetName))
                    return this.LoadImpl<T>(assetName, language);

                // get file
                FileInfo file = this.GetModFile(assetName);
                if (!file.Exists)
                    throw GetContentError("the specified path doesn't exist.");

                // load content
                switch (file.Extension.ToLower())
                {
                    // XNB file
                    case ".xnb":
                        return this.LoadImpl<T>(assetName, language);

                    // unpacked map
                    case ".tbin":
                        throw GetContentError($"can't read unpacked map file '{assetName}' directly from the underlying content manager. It must be loaded through the mod's {typeof(IModHelper)}.{nameof(IModHelper.Content)} helper.");

                    // unpacked image
                    case ".png":
                        // validate
                        if (typeof(T) != typeof(Texture2D))
                            throw GetContentError($"can't read file with extension '{file.Extension}' as type '{typeof(T)}'; must be type '{typeof(Texture2D)}'.");

                        // fetch & cache
                        using (FileStream stream = File.OpenRead(file.FullName))
                        {
                            Texture2D texture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, stream);
                            texture = this.PremultiplyTransparency(texture);
                            this.Inject(assetName, texture);
                            return (T)(object)texture;
                        }

                    default:
                        throw GetContentError($"unknown file extension '{file.Extension}'; must be one of '.png', '.tbin', or '.xnb'.");
                }
            }
            catch (Exception ex) when (!(ex is SContentLoadException))
            {
                if (ex.GetInnermostException() is DllNotFoundException dllEx && dllEx.Message == "libgdiplus.dylib")
                    throw GetContentError("couldn't find libgdiplus, which is needed to load mod images. Make sure Mono is installed and you're running the game through the normal launcher.");
                throw new SContentLoadException($"The content manager failed loading content asset '{assetName}'.", ex);
            }
        }

        /// <summary>Load the base asset without localisation.</summary>
        /// <typeparam name="T">The type of asset to load.</typeparam>
        /// <param name="assetName">The asset path relative to the loader root directory, not including the <c>.xnb</c> extension.</param>
        public override T LoadBase<T>(string assetName)
        {
            return this.Load<T>(assetName, LanguageCode.en);
        }

        /// <summary>Inject an asset into the cache.</summary>
        /// <typeparam name="T">The type of asset to inject.</typeparam>
        /// <param name="assetName">The asset path relative to the loader root directory, not including the <c>.xnb</c> extension.</param>
        /// <param name="value">The asset value.</param>
        public void Inject<T>(string assetName, T value)
        {
            assetName = this.NormaliseAssetName(assetName);
            this.Cache[assetName] = value;
        }

        /// <summary>Create a new content manager for temporary use.</summary>
        public override LocalizedContentManager CreateTemporary()
        {
            return this.Coordinator.CreateContentManager("(temporary)", isModFolder: false);
        }

        /// <summary>Normalise path separators in a file path. For asset keys, see <see cref="NormaliseAssetName"/> instead.</summary>
        /// <param name="path">The file path to normalise.</param>
        [Pure]
        public string NormalisePathSeparators(string path)
        {
            return this.Cache.NormalisePathSeparators(path);
        }

        /// <summary>Normalise an asset name so it's consistent with the underlying cache.</summary>
        /// <param name="assetName">The asset key.</param>
        [Pure]
        public string NormaliseAssetName(string assetName)
        {
            return this.Cache.NormaliseKey(assetName);
        }

        /// <summary>Assert that the given key has a valid format.</summary>
        /// <param name="key">The asset key to check.</param>
        /// <exception cref="SContentLoadException">The asset key is empty or contains invalid characters.</exception>
        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "Parameter is only used for assertion checks by design.")]
        public void AssertValidAssetKeyFormat(string key)
        {
            // NOTE: the game checks for ContentLoadException to handle invalid keys, so avoid
            // throwing other types like ArgumentException here.
            if (string.IsNullOrWhiteSpace(key))
                throw new SContentLoadException("The asset key or local path is empty.");
            if (key.Intersect(Path.GetInvalidPathChars()).Any())
                throw new SContentLoadException("The asset key or local path contains invalid characters.");
        }

        /// <summary>Convert an absolute file path into an appropriate asset name.</summary>
        /// <param name="absolutePath">The absolute path to the file.</param>
        /// <param name="relativeTo">The folder to which to get a relative path.</param>
        public string GetAssetNameFromFilePath(string absolutePath, ContentSource relativeTo)
        {
#if SMAPI_FOR_WINDOWS
            // XNA doesn't allow absolute asset paths, so get a path relative to the source folder
            string sourcePath = relativeTo == ContentSource.GameContent ? this.Coordinator.FullRootDirectory : this.FullRootDirectory;
            return this.GetRelativePath(sourcePath, absolutePath);
#else
            // MonoGame is weird about relative paths on Mac, but allows absolute paths
            return absolutePath;
#endif
        }

        /****
        ** Content loading
        ****/
        /// <summary>Get the current content locale.</summary>
        public string GetLocale()
        {
            return this.GetLocale(this.GetCurrentLanguage());
        }

        /// <summary>The locale for a language.</summary>
        /// <param name="language">The language.</param>
        public string GetLocale(LocalizedContentManager.LanguageCode language)
        {
            return this.LanguageCodeString(language);
        }

        /// <summary>Get whether the content manager has already loaded and cached the given asset.</summary>
        /// <param name="assetName">The asset path relative to the loader root directory, not including the <c>.xnb</c> extension.</param>
        public bool IsLoaded(string assetName)
        {
            assetName = this.Cache.NormaliseKey(assetName);
            return this.IsNormalisedKeyLoaded(assetName);
        }

        /// <summary>Get the cached asset keys.</summary>
        public IEnumerable<string> GetAssetKeys()
        {
            return this.Cache.Keys
                .Select(this.GetAssetName)
                .Distinct();
        }

        /****
        ** Cache invalidation
        ****/
        /// <summary>Purge matched assets from the cache.</summary>
        /// <param name="predicate">Matches the asset keys to invalidate.</param>
        /// <param name="dispose">Whether to dispose invalidated assets. This should only be <c>true</c> when they're being invalidated as part of a dispose, to avoid crashing the game.</param>
        /// <returns>Returns the number of invalidated assets.</returns>
        public IEnumerable<string> InvalidateCache(Func<string, Type, bool> predicate, bool dispose = false)
        {
            HashSet<string> removeAssetNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            this.Cache.Remove((key, type) =>
            {
                this.ParseCacheKey(key, out string assetName, out _);
                if (removeAssetNames.Contains(assetName) || predicate(assetName, type))
                {
                    removeAssetNames.Add(assetName);
                    return true;
                }
                return false;
            });

            return removeAssetNames;
        }

        /// <summary>Dispose held resources.</summary>
        /// <param name="isDisposing">Whether the content manager is being disposed (rather than finalized).</param>
        protected override void Dispose(bool isDisposing)
        {
            if (this.IsDisposed)
                return;
            this.IsDisposed = true;

            this.OnDisposing(this);
            base.Dispose(isDisposing);
        }

        /// <inheritdoc />
        public override void Unload()
        {
            if (this.IsDisposed)
                return; // base logic doesn't allow unloading twice, which happens due to SMAPI and the game both unloading

            base.Unload();
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Asset name/key handling
        ****/
        /// <summary>Get a directory or file path relative to the content root.</summary>
        /// <param name="sourcePath">The source file path.</param>
        /// <param name="targetPath">The target file path.</param>
        private string GetRelativePath(string sourcePath, string targetPath)
        {
            return PathUtilities.GetRelativePath(sourcePath, targetPath);
        }

        /// <summary>Get the locale codes (like <c>ja-JP</c>) used in asset keys.</summary>
        private IDictionary<LocalizedContentManager.LanguageCode, string> GetKeyLocales()
        {
            // create locale => code map
            IDictionary<LocalizedContentManager.LanguageCode, string> map = new Dictionary<LocalizedContentManager.LanguageCode, string>();
            foreach (LocalizedContentManager.LanguageCode code in Enum.GetValues(typeof(LocalizedContentManager.LanguageCode)))
                map[code] = this.GetLocale(code);

            return map;
        }

        /// <summary>Get the asset name from a cache key.</summary>
        /// <param name="cacheKey">The input cache key.</param>
        private string GetAssetName(string cacheKey)
        {
            this.ParseCacheKey(cacheKey, out string assetName, out string _);
            return assetName;
        }

        /// <summary>Parse a cache key into its component parts.</summary>
        /// <param name="cacheKey">The input cache key.</param>
        /// <param name="assetName">The original asset name.</param>
        /// <param name="localeCode">The asset locale code (or <c>null</c> if not localised).</param>
        private void ParseCacheKey(string cacheKey, out string assetName, out string localeCode)
        {
            // handle localised key
            if (!string.IsNullOrWhiteSpace(cacheKey))
            {
                int lastSepIndex = cacheKey.LastIndexOf(".", StringComparison.InvariantCulture);
                if (lastSepIndex >= 0)
                {
                    string suffix = cacheKey.Substring(lastSepIndex + 1, cacheKey.Length - lastSepIndex - 1);
                    if (this.LanguageCodes.ContainsKey(suffix))
                    {
                        assetName = cacheKey.Substring(0, lastSepIndex);
                        localeCode = cacheKey.Substring(lastSepIndex + 1, cacheKey.Length - lastSepIndex - 1);
                        return;
                    }
                }
            }

            // handle simple key
            assetName = cacheKey;
            localeCode = null;
        }

        /****
        ** Cache handling
        ****/
        /// <summary>Get whether an asset has already been loaded.</summary>
        /// <param name="normalisedAssetName">The normalised asset name.</param>
        private bool IsNormalisedKeyLoaded(string normalisedAssetName)
        {
            // default English
            if (this.Language == LocalizedContentManager.LanguageCode.en)
                return this.Cache.ContainsKey(normalisedAssetName);

            // translated
            string localeKey = $"{normalisedAssetName}.{this.GetLocale(this.GetCurrentLanguage())}";
            if (this.IsLocalisableLookup.TryGetValue(localeKey, out bool localisable))
            {
                return localisable
                    ? this.Cache.ContainsKey(localeKey)
                    : this.Cache.ContainsKey(normalisedAssetName);
            }

            // not loaded yet
            return false;
        }

        /****
        ** Content loading
        ****/
        /// <summary>Load an asset name without heuristics to support mod content.</summary>
        /// <typeparam name="T">The type of asset to load.</typeparam>
        /// <param name="assetName">The asset path relative to the loader root directory, not including the <c>.xnb</c> extension.</param>
        /// <param name="language">The language code for which to load content.</param>
        private T LoadImpl<T>(string assetName, LocalizedContentManager.LanguageCode language)
        {
            // skip if already loaded
            if (this.IsNormalisedKeyLoaded(assetName))
                return base.Load<T>(assetName, language);

            // load asset
            T data;
            if (this.AssetsBeingLoaded.Contains(assetName))
            {
                this.Monitor.Log($"Broke loop while loading asset '{assetName}'.", LogLevel.Warn);
                this.Monitor.Log($"Bypassing mod loaders for this asset. Stack trace:\n{Environment.StackTrace}", LogLevel.Trace);
                data = base.Load<T>(assetName, language);
            }
            else
            {
                data = this.AssetsBeingLoaded.Track(assetName, () =>
                {
                    string locale = this.GetLocale(language);
                    IAssetInfo info = new AssetInfo(locale, assetName, typeof(T), this.NormaliseAssetName);
                    IAssetData asset =
                        this.ApplyLoader<T>(info)
                        ?? new AssetDataForObject(info, base.Load<T>(assetName, language), this.NormaliseAssetName);
                    asset = this.ApplyEditors<T>(info, asset);
                    return (T)asset.Data;
                });
            }

            // update cache & return data
            this.Inject(assetName, data);
            return data;
        }

        /// <summary>Get a file from the mod folder.</summary>
        /// <param name="path">The asset path relative to the content folder.</param>
        private FileInfo GetModFile(string path)
        {
            // try exact match
            FileInfo file = new FileInfo(Path.Combine(this.FullRootDirectory, path));

            // try with default extension
            if (!file.Exists && file.Extension.ToLower() != ".xnb")
            {
                FileInfo result = new FileInfo(file.FullName + ".xnb");
                if (result.Exists)
                    file = result;
            }

            return file;
        }

        /// <summary>Load the initial asset from the registered <see cref="Loaders"/>.</summary>
        /// <param name="info">The basic asset metadata.</param>
        /// <returns>Returns the loaded asset metadata, or <c>null</c> if no loader matched.</returns>
        private IAssetData ApplyLoader<T>(IAssetInfo info)
        {
            // find matching loaders
            var loaders = this.GetInterceptors(this.Loaders)
                .Where(entry =>
                {
                    try
                    {
                        return entry.Value.CanLoad<T>(info);
                    }
                    catch (Exception ex)
                    {
                        entry.Key.LogAsMod($"Mod failed when checking whether it could load asset '{info.AssetName}', and will be ignored. Error details:\n{ex.GetLogSummary()}", LogLevel.Error);
                        return false;
                    }
                })
                .ToArray();

            // validate loaders
            if (!loaders.Any())
                return null;
            if (loaders.Length > 1)
            {
                string[] loaderNames = loaders.Select(p => p.Key.DisplayName).ToArray();
                this.Monitor.Log($"Multiple mods want to provide the '{info.AssetName}' asset ({string.Join(", ", loaderNames)}), but an asset can't be loaded multiple times. SMAPI will use the default asset instead; uninstall one of the mods to fix this. (Message for modders: you should usually use {typeof(IAssetEditor)} instead to avoid conflicts.)", LogLevel.Warn);
                return null;
            }

            // fetch asset from loader
            IModMetadata mod = loaders[0].Key;
            IAssetLoader loader = loaders[0].Value;
            T data;
            try
            {
                data = loader.Load<T>(info);
                this.Monitor.Log($"{mod.DisplayName} loaded asset '{info.AssetName}'.", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                mod.LogAsMod($"Mod crashed when loading asset '{info.AssetName}'. SMAPI will use the default asset instead. Error details:\n{ex.GetLogSummary()}", LogLevel.Error);
                return null;
            }

            // validate asset
            if (data == null)
            {
                mod.LogAsMod($"Mod incorrectly set asset '{info.AssetName}' to a null value; ignoring override.", LogLevel.Error);
                return null;
            }

            // return matched asset
            return new AssetDataForObject(info, data, this.NormaliseAssetName);
        }

        /// <summary>Apply any <see cref="Editors"/> to a loaded asset.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="info">The basic asset metadata.</param>
        /// <param name="asset">The loaded asset.</param>
        private IAssetData ApplyEditors<T>(IAssetInfo info, IAssetData asset)
        {
            IAssetData GetNewData(object data) => new AssetDataForObject(info, data, this.NormaliseAssetName);

            // edit asset
            foreach (var entry in this.GetInterceptors(this.Editors))
            {
                // check for match
                IModMetadata mod = entry.Key;
                IAssetEditor editor = entry.Value;
                try
                {
                    if (!editor.CanEdit<T>(info))
                        continue;
                }
                catch (Exception ex)
                {
                    mod.LogAsMod($"Mod crashed when checking whether it could edit asset '{info.AssetName}', and will be ignored. Error details:\n{ex.GetLogSummary()}", LogLevel.Error);
                    continue;
                }

                // try edit
                object prevAsset = asset.Data;
                try
                {
                    editor.Edit<T>(asset);
                    this.Monitor.Log($"{mod.DisplayName} intercepted {info.AssetName}.", LogLevel.Trace);
                }
                catch (Exception ex)
                {
                    mod.LogAsMod($"Mod crashed when editing asset '{info.AssetName}', which may cause errors in-game. Error details:\n{ex.GetLogSummary()}", LogLevel.Error);
                }

                // validate edit
                if (asset.Data == null)
                {
                    mod.LogAsMod($"Mod incorrectly set asset '{info.AssetName}' to a null value; ignoring override.", LogLevel.Warn);
                    asset = GetNewData(prevAsset);
                }
                else if (!(asset.Data is T))
                {
                    mod.LogAsMod($"Mod incorrectly set asset '{asset.AssetName}' to incompatible type '{asset.Data.GetType()}', expected '{typeof(T)}'; ignoring override.", LogLevel.Warn);
                    asset = GetNewData(prevAsset);
                }
            }

            // return result
            return asset;
        }

        /// <summary>Get all registered interceptors from a list.</summary>
        private IEnumerable<KeyValuePair<IModMetadata, T>> GetInterceptors<T>(IDictionary<IModMetadata, IList<T>> entries)
        {
            foreach (var entry in entries)
            {
                IModMetadata mod = entry.Key;
                IList<T> interceptors = entry.Value;

                // registered editors
                foreach (T interceptor in interceptors)
                    yield return new KeyValuePair<IModMetadata, T>(mod, interceptor);
            }
        }

        /// <summary>Premultiply a texture's alpha values to avoid transparency issues in the game. This is only possible if the game isn't currently drawing.</summary>
        /// <param name="texture">The texture to premultiply.</param>
        /// <returns>Returns a premultiplied texture.</returns>
        /// <remarks>Based on <a href="https://gist.github.com/Layoric/6255384">code by Layoric</a>.</remarks>
        private Texture2D PremultiplyTransparency(Texture2D texture)
        {
            // validate
            if (Context.IsInDrawLoop)
                throw new NotSupportedException("Can't load a PNG file while the game is drawing to the screen. Make sure you load content outside the draw loop.");

            // process texture
            SpriteBatch spriteBatch = Game1.spriteBatch;
            GraphicsDevice gpu = Game1.graphics.GraphicsDevice;
            using (RenderTarget2D renderTarget = new RenderTarget2D(Game1.graphics.GraphicsDevice, texture.Width, texture.Height))
            {
                // create blank render target to premultiply
                gpu.SetRenderTarget(renderTarget);
                gpu.Clear(Color.Black);

                // multiply each color by the source alpha, and write just the color values into the final texture
                spriteBatch.Begin(SpriteSortMode.Immediate, new BlendState
                {
                    ColorDestinationBlend = Blend.Zero,
                    ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue,
                    AlphaDestinationBlend = Blend.Zero,
                    AlphaSourceBlend = Blend.SourceAlpha,
                    ColorSourceBlend = Blend.SourceAlpha
                });
                spriteBatch.Draw(texture, texture.Bounds, Color.White);
                spriteBatch.End();

                // copy the alpha values from the source texture into the final one without multiplying them
                spriteBatch.Begin(SpriteSortMode.Immediate, new BlendState
                {
                    ColorWriteChannels = ColorWriteChannels.Alpha,
                    AlphaDestinationBlend = Blend.Zero,
                    ColorDestinationBlend = Blend.Zero,
                    AlphaSourceBlend = Blend.One,
                    ColorSourceBlend = Blend.One
                });
                spriteBatch.Draw(texture, texture.Bounds, Color.White);
                spriteBatch.End();

                // release GPU
                gpu.SetRenderTarget(null);

                // extract premultiplied data
                Color[] data = new Color[texture.Width * texture.Height];
                renderTarget.GetData(data);

                // unset texture from GPU to regain control
                gpu.Textures[0] = null;

                // update texture with premultiplied data
                texture.SetData(data);
            }

            return texture;
        }
    }
}
