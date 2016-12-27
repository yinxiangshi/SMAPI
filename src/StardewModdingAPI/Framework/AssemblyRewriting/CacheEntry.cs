using System.IO;

namespace StardewModdingAPI.Framework.AssemblyRewriting
{
    /// <summary>Represents cached metadata for a rewritten assembly.</summary>
    internal class CacheEntry
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The MD5 hash for the original assembly.</summary>
        public readonly string Hash;

        /// <summary>The SMAPI version used to rewrite the assembly.</summary>
        public readonly string ApiVersion;

        /// <summary>Whether to use the cached assembly instead of the original assembly.</summary>
        public readonly bool UseCachedAssembly;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="hash">The MD5 hash for the original assembly.</param>
        /// <param name="apiVersion">The SMAPI version used to rewrite the assembly.</param>
        /// <param name="useCachedAssembly">Whether to use the cached assembly instead of the original assembly.</param>
        public CacheEntry(string hash, string apiVersion, bool useCachedAssembly)
        {
            this.Hash = hash;
            this.ApiVersion = apiVersion;
            this.UseCachedAssembly = useCachedAssembly;
        }

        /// <summary>Get whether the cache entry is up-to-date for the given assembly hash.</summary>
        /// <param name="paths">The paths for the cached assembly.</param>
        /// <param name="hash">The MD5 hash of the original assembly.</param>
        /// <param name="currentVersion">The current SMAPI version.</param>
        public bool IsUpToDate(CachePaths paths, string hash, ISemanticVersion currentVersion)
        {
            return hash == this.Hash
                && this.ApiVersion == currentVersion.ToString()
                && (!this.UseCachedAssembly || File.Exists(paths.Assembly));
        }
    }
}