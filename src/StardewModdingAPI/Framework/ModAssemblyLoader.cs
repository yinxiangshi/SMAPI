using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;
using Newtonsoft.Json;
using StardewModdingAPI.AssemblyRewriters;
using StardewModdingAPI.Framework.AssemblyRewriting;

namespace StardewModdingAPI.Framework
{
    /// <summary>Preprocesses and loads mod assemblies.</summary>
    internal class ModAssemblyLoader
    {
        /*********
        ** Properties
        *********/
        /// <summary>The name of the directory containing a mod's cached data.</summary>
        private readonly string CacheDirName;

        /// <summary>Metadata for mapping assemblies to the current <see cref="Platform"/>.</summary>
        private readonly PlatformAssemblyMap AssemblyMap;

        /// <summary>Rewrites assembly types to match the current platform.</summary>
        private readonly AssemblyTypeRewriter AssemblyTypeRewriter;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="cacheDirName">The name of the directory containing a mod's cached data.</param>
        /// <param name="targetPlatform">The current game platform.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public ModAssemblyLoader(string cacheDirName, Platform targetPlatform, IMonitor monitor)
        {
            this.CacheDirName = cacheDirName;
            this.Monitor = monitor;
            this.AssemblyMap = Constants.GetAssemblyMap(targetPlatform);
            this.AssemblyTypeRewriter = new AssemblyTypeRewriter(this.AssemblyMap, monitor);
        }

        /// <summary>Preprocess an assembly unless the cache is up to date.</summary>
        /// <param name="assemblyPath">The assembly file path.</param>
        /// <returns>Returns the rewrite metadata for the preprocessed assembly.</returns>
        public RewriteResult ProcessAssemblyUnlessCached(string assemblyPath)
        {
            // read assembly data
            byte[] assemblyBytes = File.ReadAllBytes(assemblyPath);
            string hash = string.Join("", MD5.Create().ComputeHash(assemblyBytes).Select(p => p.ToString("X2")));

            // get cached result if current
            CachePaths cachePaths = this.GetCachePaths(assemblyPath);
            {
                CacheEntry cacheEntry = File.Exists(cachePaths.Metadata) ? JsonConvert.DeserializeObject<CacheEntry>(File.ReadAllText(cachePaths.Metadata)) : null;
                if (cacheEntry != null && cacheEntry.IsUpToDate(cachePaths, hash, Constants.Version))
                    return new RewriteResult(assemblyPath, cachePaths, assemblyBytes, cacheEntry.Hash, cacheEntry.UseCachedAssembly, isNewerThanCache: false); // no rewrite needed
            }
            this.Monitor.Log($"Preprocessing {Path.GetFileName(assemblyPath)} for compatibility...", LogLevel.Trace);

            // rewrite assembly
            AssemblyDefinition assembly;
            using (Stream readStream = new MemoryStream(assemblyBytes))
                assembly = AssemblyDefinition.ReadAssembly(readStream);
            bool modified = this.AssemblyTypeRewriter.RewriteAssembly(assembly);
            using (MemoryStream outStream = new MemoryStream())
            {
                assembly.Write(outStream);
                byte[] outBytes = outStream.ToArray();
                return new RewriteResult(assemblyPath, cachePaths, outBytes, hash, useCachedAssembly: modified, isNewerThanCache: true);
            }
        }

        /// <summary>Write rewritten assembly metadata to the cache for a mod.</summary>
        /// <param name="results">The rewrite results.</param>
        /// <param name="forceCacheAssemblies">Whether to write all assemblies to the cache, even if they weren't modified.</param>
        /// <exception cref="InvalidOperationException">There are no results to write, or the results are not all for the same directory.</exception>
        public void WriteCache(IEnumerable<RewriteResult> results, bool forceCacheAssemblies)
        {
            results = results.ToArray();

            // get cache directory
            if (!results.Any())
                throw new InvalidOperationException("There are no assemblies to cache.");
            if (results.Select(p => p.CachePaths.Directory).Distinct().Count() > 1)
                throw new InvalidOperationException("The assemblies can't be cached together because they have different source directories.");
            string cacheDir = results.Select(p => p.CachePaths.Directory).First();

            // reset cache
            if (Directory.Exists(cacheDir))
                Directory.Delete(cacheDir, recursive: true);
            Directory.CreateDirectory(cacheDir);

            // cache all results
            foreach (RewriteResult result in results)
            {
                CacheEntry cacheEntry = new CacheEntry(result.Hash, Constants.Version.ToString(), forceCacheAssemblies || result.UseCachedAssembly);
                File.WriteAllText(result.CachePaths.Metadata, JsonConvert.SerializeObject(cacheEntry));
                if (forceCacheAssemblies || result.UseCachedAssembly)
                    File.WriteAllBytes(result.CachePaths.Assembly, result.AssemblyBytes);
            }
        }

        /// <summary>Resolve an assembly from its name.</summary>
        /// <param name="name">The assembly name.</param>
        /// <remarks>
        /// This implementation returns the first loaded assembly which matches the short form of
        /// the assembly name, to resolve assembly resolution issues when rewriting
        /// assemblies (especially with Mono). Since this is meant to be called on <see cref="AppDomain.AssemblyResolve"/>,
        /// the implicit assumption is that loading the exact assembly failed.
        /// </remarks>
        public Assembly ResolveAssembly(string name)
        {
            string shortName = name.Split(new[] { ',' }, 2).First(); // get simple name (without version and culture)
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(p => p.GetName().Name == shortName);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the cache details for an assembly.</summary>
        /// <param name="assemblyPath">The assembly file path.</param>
        private CachePaths GetCachePaths(string assemblyPath)
        {
            string fileName = Path.GetFileName(assemblyPath);
            string dirPath = Path.Combine(Path.GetDirectoryName(assemblyPath), this.CacheDirName);
            string cacheAssemblyPath = Path.Combine(dirPath, fileName);
            string metadataPath = Path.Combine(dirPath, $"{fileName}.json");
            return new CachePaths(dirPath, cacheAssemblyPath, metadataPath);
        }
    }
}
