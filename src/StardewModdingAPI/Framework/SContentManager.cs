using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using StardewModdingAPI.AssemblyRewriters;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework.Reflection;
using StardewValley;

namespace StardewModdingAPI.Framework
{
    /// <summary>SMAPI's implementation of the game's content manager which lets it raise content events.</summary>
    internal class SContentManager : LocalizedContentManager
    {
        /*********
        ** Accessors
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
        private readonly Func<string, string> NormaliseKeyForPlatform;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="serviceProvider">The service provider to use to locate services.</param>
        /// <param name="rootDirectory">The root directory to search for content.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public SContentManager(IServiceProvider serviceProvider, string rootDirectory, IMonitor monitor)
            : this(serviceProvider, rootDirectory, Thread.CurrentThread.CurrentUICulture, null, monitor) { }

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
            this.Monitor = monitor;
            IReflectionHelper reflection = new ReflectionHelper();

            // get underlying asset cache
            this.Cache = reflection
                .GetPrivateField<Dictionary<string, object>>(this, "loadedAssets")
                .GetValue();

            // get asset key normalisation logic
            if (Constants.TargetPlatform == Platform.Windows)
            {
                IPrivateMethod method = reflection.GetPrivateMethod(typeof(TitleContainer), "GetCleanPath");
                this.NormaliseKeyForPlatform = path => method.Invoke<string>(path);
            }
            else
                this.NormaliseKeyForPlatform = key => key.Replace('\\', '/'); // based on MonoGame's ContentManager.Load<T> logic
        }

        /// <summary>Load an asset that has been processed by the content pipeline.</summary>
        /// <typeparam name="T">The type of asset to load.</typeparam>
        /// <param name="assetName">The asset path relative to the loader root directory, not including the <c>.xnb</c> extension.</param>
        public override T Load<T>(string assetName)
        {
            // normalise key so can override the cache value later
            assetName = this.NormaliseKey(assetName);

            // skip if no event handlers or already loaded
            if (!ContentEvents.HasAssetLoadingListeners() || this.Cache.ContainsKey(assetName))
                return base.Load<T>(assetName);

            // intercept load
            T data = base.Load<T>(assetName);
            IContentEventHelper helper = new ContentEventHelper(assetName, data, this.NormaliseKeyForPlatform);
            ContentEvents.InvokeAssetLoading(this.Monitor, helper);
            this.Cache[assetName] = helper.Data;
            return (T)helper.Data;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Normalise an asset key so it's consistent with the underlying cache.</summary>
        /// <param name="key">The asset key.</param>
        private string NormaliseKey(string key)
        {
            // ensure key format is consistent
            string[] parts = key.Split(SContentManager.PossiblePathSeparators, StringSplitOptions.RemoveEmptyEntries);
            key = string.Join(SContentManager.PreferredPathSeparator, parts);

            // apply platform normalisation logic
            return this.NormaliseKeyForPlatform(key);
        }
    }
}
