namespace StardewModdingAPI.Framework.AssemblyRewriting
{
    /// <summary>Metadata about a preprocessed assembly.</summary>
    internal class RewriteResult
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The original assembly path.</summary>
        public readonly string OriginalAssemblyPath;

        /// <summary>The cache paths.</summary>
        public readonly CachePaths CachePaths;

        /// <summary>The rewritten assembly bytes.</summary>
        public readonly byte[] AssemblyBytes;

        /// <summary>The MD5 hash for the original assembly.</summary>
        public readonly string Hash;

        /// <summary>Whether to use the cached assembly instead of the original assembly.</summary>
        public readonly bool UseCachedAssembly;

        /// <summary>Whether this data is newer than the cache.</summary>
        public readonly bool IsNewerThanCache;



        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="originalAssemblyPath"></param>
        /// <param name="cachePaths">The cache paths.</param>
        /// <param name="assemblyBytes">The rewritten assembly bytes.</param>
        /// <param name="hash">The MD5 hash for the original assembly.</param>
        /// <param name="useCachedAssembly">Whether to use the cached assembly instead of the original assembly.</param>
        /// <param name="isNewerThanCache">Whether this data is newer than the cache.</param>
        public RewriteResult(string originalAssemblyPath, CachePaths cachePaths, byte[] assemblyBytes, string hash, bool useCachedAssembly, bool isNewerThanCache)
        {
            this.OriginalAssemblyPath = originalAssemblyPath;
            this.CachePaths = cachePaths;
            this.Hash = hash;
            this.AssemblyBytes = assemblyBytes;
            this.UseCachedAssembly = useCachedAssembly;
            this.IsNewerThanCache = isNewerThanCache;
        }
    }
}
