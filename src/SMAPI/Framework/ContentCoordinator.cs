using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI.Framework.Content;
using StardewModdingAPI.Framework.Reflection;
using StardewModdingAPI.Metadata;
using StardewValley;

namespace StardewModdingAPI.Framework
{
    /// <summary>The central logic for creating content managers, invalidating caches, and propagating asset changes.</summary>
    internal class ContentCoordinator : IDisposable
    {
        /*********
        ** Properties
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Provides metadata for core game assets.</summary>
        private readonly CoreAssetPropagator CoreAssets;

        /// <summary>Simplifies access to private code.</summary>
        private readonly Reflector Reflection;

        /// <summary>The loaded content managers (including the <see cref="MainContentManager"/>).</summary>
        private readonly IList<SContentManager> ContentManagers = new List<SContentManager>();

        /// <summary>Whether the content coordinator has been disposed.</summary>
        private bool IsDisposed;


        /*********
        ** Accessors
        *********/
        /// <summary>The primary content manager used for most assets.</summary>
        public SContentManager MainContentManager { get; private set; }

        /// <summary>The current language as a constant.</summary>
        public LocalizedContentManager.LanguageCode Language => this.MainContentManager.Language;

        /// <summary>Interceptors which provide the initial versions of matching assets.</summary>
        public IDictionary<IModMetadata, IList<IAssetLoader>> Loaders { get; } = new Dictionary<IModMetadata, IList<IAssetLoader>>();

        /// <summary>Interceptors which edit matching assets after they're loaded.</summary>
        public IDictionary<IModMetadata, IList<IAssetEditor>> Editors { get; } = new Dictionary<IModMetadata, IList<IAssetEditor>>();

        /// <summary>The absolute path to the <see cref="ContentManager.RootDirectory"/>.</summary>
        public string FullRootDirectory { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="serviceProvider">The service provider to use to locate services.</param>
        /// <param name="rootDirectory">The root directory to search for content.</param>
        /// <param name="currentCulture">The current culture for which to localise content.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public ContentCoordinator(IServiceProvider serviceProvider, string rootDirectory, CultureInfo currentCulture, IMonitor monitor, Reflector reflection)
        {
            this.Monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            this.Reflection = reflection;
            this.FullRootDirectory = Path.Combine(Constants.ExecutionPath, rootDirectory);
            this.ContentManagers.Add(
                this.MainContentManager = new SContentManager("Game1.content", serviceProvider, rootDirectory, currentCulture, this, monitor, reflection, this.OnDisposing, isModFolder: false)
            );
            this.CoreAssets = new CoreAssetPropagator(this.MainContentManager.NormaliseAssetName, reflection);
        }

        /// <summary>Get a new content manager which defers loading to the content core.</summary>
        /// <param name="name">A name for the mod manager. Not guaranteed to be unique.</param>
        /// <param name="isModFolder">Whether this content manager is wrapped around a mod folder.</param>
        /// <param name="rootDirectory">The root directory to search for content (or <c>null</c>. for the default)</param>
        public SContentManager CreateContentManager(string name, bool isModFolder, string rootDirectory = null)
        {
            SContentManager manager = new SContentManager(name, this.MainContentManager.ServiceProvider, rootDirectory ?? this.MainContentManager.RootDirectory, this.MainContentManager.CurrentCulture, this, this.Monitor, this.Reflection, this.OnDisposing, isModFolder);
            this.ContentManagers.Add(manager);
            return manager;
        }

        /// <summary>Get the current content locale.</summary>
        public string GetLocale() => this.MainContentManager.GetLocale(LocalizedContentManager.CurrentLanguageCode);

        /// <summary>Convert an absolute file path into a appropriate asset name.</summary>
        /// <param name="absolutePath">The absolute path to the file.</param>
        public string GetAssetNameFromFilePath(string absolutePath) => this.MainContentManager.GetAssetNameFromFilePath(absolutePath, ContentSource.GameContent);

        /// <summary>Purge assets from the cache that match one of the interceptors.</summary>
        /// <param name="editors">The asset editors for which to purge matching assets.</param>
        /// <param name="loaders">The asset loaders for which to purge matching assets.</param>
        /// <returns>Returns the invalidated asset names.</returns>
        public IEnumerable<string> InvalidateCacheFor(IAssetEditor[] editors, IAssetLoader[] loaders)
        {
            if (!editors.Any() && !loaders.Any())
                return new string[0];

            // get CanEdit/Load methods
            MethodInfo canEdit = typeof(IAssetEditor).GetMethod(nameof(IAssetEditor.CanEdit));
            MethodInfo canLoad = typeof(IAssetLoader).GetMethod(nameof(IAssetLoader.CanLoad));
            if (canEdit == null || canLoad == null)
                throw new InvalidOperationException("SMAPI could not access the interceptor methods."); // should never happen

            // invalidate matching keys
            return this.InvalidateCache(asset =>
            {
                // check loaders
                MethodInfo canLoadGeneric = canLoad.MakeGenericMethod(asset.DataType);
                if (loaders.Any(loader => (bool)canLoadGeneric.Invoke(loader, new object[] { asset })))
                    return true;

                // check editors
                MethodInfo canEditGeneric = canEdit.MakeGenericMethod(asset.DataType);
                return editors.Any(editor => (bool)canEditGeneric.Invoke(editor, new object[] { asset }));
            });
        }

        /// <summary>Purge matched assets from the cache.</summary>
        /// <param name="predicate">Matches the asset keys to invalidate.</param>
        /// <param name="dispose">Whether to dispose invalidated assets. This should only be <c>true</c> when they're being invalidated as part of a dispose, to avoid crashing the game.</param>
        /// <returns>Returns the invalidated asset keys.</returns>
        public IEnumerable<string> InvalidateCache(Func<IAssetInfo, bool> predicate, bool dispose = false)
        {
            string locale = this.GetLocale();
            return this.InvalidateCache((assetName, type) =>
            {
                IAssetInfo info = new AssetInfo(locale, assetName, type, this.MainContentManager.NormaliseAssetName);
                return predicate(info);
            });
        }

        /// <summary>Purge matched assets from the cache.</summary>
        /// <param name="predicate">Matches the asset keys to invalidate.</param>
        /// <param name="dispose">Whether to dispose invalidated assets. This should only be <c>true</c> when they're being invalidated as part of a dispose, to avoid crashing the game.</param>
        /// <returns>Returns the invalidated asset names.</returns>
        public IEnumerable<string> InvalidateCache(Func<string, Type, bool> predicate, bool dispose = false)
        {
            // invalidate cache
            HashSet<string> removedAssetNames = new HashSet<string>();
            foreach (SContentManager contentManager in this.ContentManagers)
            {
                foreach (string name in contentManager.InvalidateCache(predicate, dispose))
                    removedAssetNames.Add(name);
            }

            // reload core game assets
            int reloaded = 0;
            foreach (string key in removedAssetNames)
            {
                if (this.CoreAssets.Propagate(this.MainContentManager, key)) // use an intercepted content manager
                    reloaded++;
            }

            // report result
            if (removedAssetNames.Any())
                this.Monitor.Log($"Invalidated {removedAssetNames.Count} asset names: {string.Join(", ", removedAssetNames.OrderBy(p => p, StringComparer.InvariantCultureIgnoreCase))}. Reloaded {reloaded} core assets.", LogLevel.Trace);
            this.Monitor.Log("Invalidated 0 cache entries.", LogLevel.Trace);

            return removedAssetNames;
        }

        /// <summary>Dispose held resources.</summary>
        public void Dispose()
        {
            if (this.IsDisposed)
                return;
            this.IsDisposed = true;

            this.Monitor.Log("Disposing the content coordinator. Content managers will no longer be usable after this point.", LogLevel.Trace);
            foreach (SContentManager contentManager in this.ContentManagers)
                contentManager.Dispose();
            this.ContentManagers.Clear();
            this.MainContentManager = null;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>A callback invoked when a content manager is disposed.</summary>
        /// <param name="contentManager">The content manager being disposed.</param>
        private void OnDisposing(SContentManager contentManager)
        {
            if (this.IsDisposed)
                return;

            this.ContentManagers.Remove(contentManager);
        }
    }
}
