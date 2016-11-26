using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;

namespace StardewModdingAPI.Framework
{
    /// <summary>Preprocesses and loads mod assemblies.</summary>
    internal class ModAssemblyLoader
    {
        /*********
        ** Properties
        *********/
        /// <summary>The directory in which to cache data.</summary>
        private readonly string CacheDirPath;

        /// <summary>Encapsulates monitoring and logging for a given module.</summary>
        private readonly IMonitor Monitor;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="cacheDirPath">The cache directory.</param>
        /// <param name="targetPlatform">The current game platform.</param>
        /// <param name="monitor">Encapsulates monitoring and logging for a given module.</param>
        public ModAssemblyLoader(string cacheDirPath, Platform targetPlatform, IMonitor monitor)
        {
            this.CacheDirPath = cacheDirPath;
            this.Monitor = monitor;
        }

        /// <summary>Preprocess an assembly and cache the modified version.</summary>
        /// <param name="assemblyPath">The assembly file path.</param>
        public void ProcessAssembly(string assemblyPath)
        {
            // read assembly data
            byte[] assemblyBytes = File.ReadAllBytes(assemblyPath);
            byte[] hash = MD5.Create().ComputeHash(assemblyBytes);

            // check cache
            CachePaths cachePaths = this.GetCacheInfo(assemblyPath);
            bool canUseCache = File.Exists(cachePaths.Assembly) && File.Exists(cachePaths.Hash) && hash.SequenceEqual(File.ReadAllBytes(cachePaths.Hash));

            // process assembly if not cached
            if (!canUseCache)
            {
                this.Monitor.Log($"Preprocessing new assembly {assemblyPath}...");

                // read assembly definition
                AssemblyDefinition definition;
                using (Stream readStream = new MemoryStream(assemblyBytes))
                    definition = AssemblyDefinition.ReadAssembly(readStream);

                // write cache
                using (MemoryStream outStream = new MemoryStream())
                {
                    definition.Write(outStream);
                    byte[] outBytes = outStream.ToArray();
                    Directory.CreateDirectory(cachePaths.Directory);
                    File.WriteAllBytes(cachePaths.Assembly, outBytes);
                    File.WriteAllBytes(cachePaths.Hash, hash);
                }
            }
        }

        /// <summary>Load a preprocessed assembly.</summary>
        /// <param name="assemblyPath">The assembly file path.</param>
        public Assembly LoadCachedAssembly(string assemblyPath)
        {
            CachePaths cachePaths = this.GetCacheInfo(assemblyPath);
            if (!File.Exists(cachePaths.Assembly))
                throw new InvalidOperationException($"The assembly {assemblyPath} doesn't exist in the preprocessed cache.");
            return Assembly.UnsafeLoadFrom(cachePaths.Assembly);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the cache details for an assembly.</summary>
        /// <param name="assemblyPath">The assembly file path.</param>
        private CachePaths GetCacheInfo(string assemblyPath)
        {
            string key = Path.GetFileNameWithoutExtension(assemblyPath);
            string dirPath = Path.Combine(this.CacheDirPath, new DirectoryInfo(Path.GetDirectoryName(assemblyPath)).Name);
            string cacheAssemblyPath = Path.Combine(dirPath, $"{key}.dll");
            string cacheHashPath = Path.Combine(dirPath, $"{key}.hash");
            return new CachePaths(dirPath, cacheAssemblyPath, cacheHashPath);
        }

        /*********
        ** Private objects
        *********/
        /// <summary>Contains the paths for an assembly's cached data.</summary>
        private struct CachePaths
        {
            /*********
            ** Accessors
            *********/
            /// <summary>The directory path which contains the assembly.</summary>
            public string Directory { get; }

            /// <summary>The file path of the assembly file.</summary>
            public string Assembly { get; }

            /// <summary>The file path containing the MD5 hash for the assembly.</summary>
            public string Hash { get; }


            /*********
            ** Public methods
            *********/
            /// <summary>Construct an instance.</summary>
            /// <param name="directory">The directory path which contains the assembly.</param>
            /// <param name="assembly">The file path of the assembly file.</param>
            /// <param name="hash">The file path containing the MD5 hash for the assembly.</param>
            public CachePaths(string directory, string assembly, string hash)
            {
                this.Directory = directory;
                this.Assembly = assembly;
                this.Hash = hash;
            }
        }
    }
}
