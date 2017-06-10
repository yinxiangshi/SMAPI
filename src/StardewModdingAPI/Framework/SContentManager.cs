using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI.AssemblyRewriters;
using StardewModdingAPI.Framework.Content;
using StardewModdingAPI.Framework.Reflection;
using StardewValley;

namespace StardewModdingAPI.Framework
{
    /// <summary>SMAPI's implementation of the game's content manager which lets it raise content events.</summary>
    internal class SContentManager : LocalizedContentManager
    {
        /*********
        ** Properties
        *********/
        /// <summary>The possible directory separator characters in an asset key.</summary>
        private static readonly char[] PossiblePathSeparators = new[] { '/', '\\', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }.Distinct().ToArray();

        /// <summary>The preferred directory separator chaeacter in an asset key.</summary>
        private static readonly string PreferredPathSeparator = Path.DirectorySeparatorChar.ToString();

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The underlying content manager's asset cache.</summary>
        private readonly IDictionary<string, object> Cache;

        /// <summary>Applies platform-specific asset key normalisation so it's consistent with the underlying cache.</summary>
        private readonly Func<string, string> NormaliseAssetNameForPlatform;

        /// <summary>The private <see cref="LocalizedContentManager"/> method which generates the locale portion of an asset name.</summary>
        private readonly IPrivateMethod GetKeyLocale;


        /*********
        ** Accessors
        *********/
        /// <summary>Implementations which change assets after they're loaded.</summary>
        internal IDictionary<IModMetadata, IList<IAssetEditor>> Editors { get; } = new Dictionary<IModMetadata, IList<IAssetEditor>>();

        /// <summary>The absolute path to the <see cref="ContentManager.RootDirectory"/>.</summary>
        public string FullRootDirectory => Path.Combine(Constants.ExecutionPath, this.RootDirectory);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="serviceProvider">The service provider to use to locate services.</param>
        /// <param name="rootDirectory">The root directory to search for content.</param>
        /// <param name="currentCulture">The current culture for which to localise content.</param>
        /// <param name="languageCodeOverride">The current language code for which to localise content.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public SContentManager(IServiceProvider serviceProvider, string rootDirectory, CultureInfo currentCulture, string languageCodeOverride, IMonitor monitor)
            : base(serviceProvider, rootDirectory, currentCulture, languageCodeOverride)
        {
            // initialise
            IReflectionHelper reflection = new ReflectionHelper();
            this.Monitor = monitor;

            // get underlying fields for interception
            this.Cache = reflection.GetPrivateField<Dictionary<string, object>>(this, "loadedAssets").GetValue();
            this.GetKeyLocale = reflection.GetPrivateMethod(this, "languageCode");

            // get asset key normalisation logic
            if (Constants.TargetPlatform == Platform.Windows)
            {
                IPrivateMethod method = reflection.GetPrivateMethod(typeof(TitleContainer), "GetCleanPath");
                this.NormaliseAssetNameForPlatform = path => method.Invoke<string>(path);
            }
            else
                this.NormaliseAssetNameForPlatform = key => key.Replace('\\', '/'); // based on MonoGame's ContentManager.Load<T> logic
        }

        /// <summary>Normalise path separators in a file path. For asset keys, see <see cref="NormaliseAssetName"/> instead.</summary>
        /// <param name="path">The file path to normalise.</param>
        public string NormalisePathSeparators(string path)
        {
            string[] parts = path.Split(SContentManager.PossiblePathSeparators, StringSplitOptions.RemoveEmptyEntries);
            string normalised = string.Join(SContentManager.PreferredPathSeparator, parts);
            if (path.StartsWith(SContentManager.PreferredPathSeparator))
                normalised = SContentManager.PreferredPathSeparator + normalised; // keep root slash
            return normalised;
        }

        /// <summary>Normalise an asset name so it's consistent with the underlying cache.</summary>
        /// <param name="assetName">The asset key.</param>
        public string NormaliseAssetName(string assetName)
        {
            assetName = this.NormalisePathSeparators(assetName);
            if (assetName.EndsWith(".xnb", StringComparison.InvariantCultureIgnoreCase))
                return assetName.Substring(0, assetName.Length - 4);
            return this.NormaliseAssetNameForPlatform(assetName);
        }

        /// <summary>Get whether the content manager has already loaded and cached the given asset.</summary>
        /// <param name="assetName">The asset path relative to the loader root directory, not including the <c>.xnb</c> extension.</param>
        public bool IsLoaded(string assetName)
        {
            assetName = this.NormaliseAssetName(assetName);
            return this.IsNormalisedKeyLoaded(assetName);
        }

        /// <summary>Load an asset that has been processed by the content pipeline.</summary>
        /// <typeparam name="T">The type of asset to load.</typeparam>
        /// <param name="assetName">The asset path relative to the loader root directory, not including the <c>.xnb</c> extension.</param>
        public override T Load<T>(string assetName)
        {
            assetName = this.NormaliseAssetName(assetName);

            // skip if already loaded
            if (this.IsNormalisedKeyLoaded(assetName))
                return base.Load<T>(assetName);

            // load asset
            T asset = this.GetAssetWithInterceptors(this.GetLocale(), assetName, () => base.Load<T>(assetName));
            this.Cache[assetName] = asset;
            return asset;
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

        /// <summary>Get the current content locale.</summary>
        public string GetLocale()
        {
            return this.GetKeyLocale.Invoke<string>();
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Get whether an asset has already been loaded.</summary>
        /// <param name="normalisedAssetName">The normalised asset name.</param>
        private bool IsNormalisedKeyLoaded(string normalisedAssetName)
        {
            return this.Cache.ContainsKey(normalisedAssetName)
                || this.Cache.ContainsKey($"{normalisedAssetName}.{this.GetKeyLocale.Invoke<string>()}"); // translated asset
        }

        /// <summary>Read an asset with support for asset interceptors.</summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="locale">The current content locale.</param>
        /// <param name="normalisedKey">The normalised asset path relative to the loader root directory, not including the <c>.xnb</c> extension.</param>
        /// <param name="getData">Get the asset from the underlying content manager.</param>
        private T GetAssetWithInterceptors<T>(string locale, string normalisedKey, Func<T> getData)
        {
            // get metadata
            IAssetInfo info = new AssetInfo(locale, normalisedKey, typeof(T), this.NormaliseAssetName);

            // load asset
            T asset = getData();

            // edit asset
            IAssetData data = new AssetDataForObject(info.Locale, info.AssetName, asset, this.NormaliseAssetName);
            foreach (var modEditors in this.Editors)
            {
                IModMetadata mod = modEditors.Key;
                foreach (IAssetEditor editor in modEditors.Value)
                {
                    if (!editor.CanEdit<T>(info))
                        continue;

                    this.Monitor.Log($"{mod.DisplayName} intercepted {info.AssetName}.", LogLevel.Trace);
                    editor.Edit<T>(data);
                    if (data.Data == null)
                        throw new InvalidOperationException($"{mod.DisplayName} incorrectly set asset '{normalisedKey}' to a null value.");
                    if (!(data.Data is T))
                        throw new InvalidOperationException($"{mod.DisplayName} incorrectly set asset '{normalisedKey}' to incompatible type '{data.Data.GetType()}', expected '{typeof(T)}'.");
                }
            }

            // return result
            return (T)data.Data;
        }
    }
}
