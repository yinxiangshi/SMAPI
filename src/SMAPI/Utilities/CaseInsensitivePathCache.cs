using System;
using System.Collections.Generic;
using System.IO;

namespace StardewModdingAPI.Utilities
{
    /// <summary>Provides an API for case-insensitive relative path lookups within a root directory.</summary>
    internal class CaseInsensitivePathCache
    {
        /*********
        ** Fields
        *********/
        /// <summary>The root directory path for relative paths.</summary>
        private readonly string RootPath;

        /// <summary>A case-insensitive lookup of file paths within the <see cref="RootPath"/>. Each path is listed in both file path and asset name format, so it's usable in both contexts without needing to re-parse paths.</summary>
        private readonly Lazy<Dictionary<string, string>> RelativePathCache;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="rootPath">The root directory path for relative paths.</param>
        public CaseInsensitivePathCache(string rootPath)
        {
            this.RootPath = rootPath;
            this.RelativePathCache = new(this.GetRelativePathCache);
        }

        /// <summary>Get the exact capitalization for a given relative file path.</summary>
        /// <param name="relativePath">The relative path.</param>
        /// <remarks>Returns the resolved path in file path format, else the normalized <paramref name="relativePath"/>.</remarks>
        public string GetFilePath(string relativePath)
        {
            return this.GetImpl(PathUtilities.NormalizePath(relativePath));
        }

        /// <summary>Get the exact capitalization for a given asset name.</summary>
        /// <param name="relativePath">The relative path.</param>
        /// <remarks>Returns the resolved path in asset name format, else the normalized <paramref name="relativePath"/>.</remarks>
        public string GetAssetName(string relativePath)
        {
            return this.GetImpl(PathUtilities.NormalizeAssetName(relativePath));
        }

        /// <summary>Add a relative path that was just created by a SMAPI API.</summary>
        /// <param name="relativePath">The relative path. This must already be normalized in asset name or file path format.</param>
        public void Add(string relativePath)
        {
            // skip if cache isn't created yet (no need to add files manually in that case)
            if (!this.RelativePathCache.IsValueCreated)
                return;

            // skip if already cached
            if (this.RelativePathCache.Value.ContainsKey(relativePath))
                return;

            // make sure path exists
            relativePath = PathUtilities.NormalizePath(relativePath);
            if (!File.Exists(Path.Combine(this.RootPath, relativePath)))
                throw new InvalidOperationException($"Can't add relative path '{relativePath}' to the case-insensitive cache for '{this.RootPath}' because that file doesn't exist.");

            // cache path
            this.CacheRawPath(this.RelativePathCache.Value, relativePath);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the exact capitalization for a given relative path.</summary>
        /// <param name="relativePath">The relative path. This must already be normalized into asset name or file path format (i.e. using <see cref="PathUtilities.NormalizeAssetName"/> or <see cref="PathUtilities.NormalizePath"/> respectively).</param>
        /// <remarks>Returns the resolved path in the same format if found, else returns the path as-is.</remarks>
        private string GetImpl(string relativePath)
        {
            // invalid path
            if (string.IsNullOrWhiteSpace(relativePath))
                return relativePath;

            // already cached
            if (this.RelativePathCache.Value.TryGetValue(relativePath, out string? resolved))
                return resolved;

            // file exists but isn't cached for some reason
            // cache it now so any later references to it are case-insensitive
            if (File.Exists(Path.Combine(this.RootPath, relativePath)))
            {
                this.CacheRawPath(this.RelativePathCache.Value, relativePath);
                return relativePath;
            }

            // no such file, keep capitalization as-is
            return relativePath;
        }

        /// <summary>Get a case-insensitive lookup of file paths (see <see cref="RelativePathCache"/>).</summary>
        private Dictionary<string, string> GetRelativePathCache()
        {
            Dictionary<string, string> cache = new(StringComparer.OrdinalIgnoreCase);

            foreach (string path in Directory.EnumerateFiles(this.RootPath, "*", SearchOption.AllDirectories))
            {
                string relativePath = path.Substring(this.RootPath.Length + 1);

                this.CacheRawPath(cache, relativePath);
            }

            return cache;
        }

        /// <summary>Add a raw relative path to the cache.</summary>
        /// <param name="cache">The cache to update.</param>
        /// <param name="relativePath">The relative path to cache, with its exact filesystem capitalization.</param>
        private void CacheRawPath(IDictionary<string, string> cache, string relativePath)
        {
            string filePath = PathUtilities.NormalizePath(relativePath);
            string assetName = PathUtilities.NormalizeAssetName(relativePath);

            cache[filePath] = filePath;
            cache[assetName] = assetName;
        }
    }
}
