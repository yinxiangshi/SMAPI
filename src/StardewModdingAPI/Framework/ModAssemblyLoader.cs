using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;
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
        /// <summary>The directory in which to cache data.</summary>
        private readonly string CacheDirPath;

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
        /// <param name="cacheDirPath">The cache directory.</param>
        /// <param name="targetPlatform">The current game platform.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public ModAssemblyLoader(string cacheDirPath, Platform targetPlatform, IMonitor monitor)
        {
            this.CacheDirPath = cacheDirPath;
            this.Monitor = monitor;
            this.AssemblyMap = Constants.GetAssemblyMap(targetPlatform);
            this.AssemblyTypeRewriter = new AssemblyTypeRewriter(this.AssemblyMap, monitor);
        }

        /// <summary>Preprocess an assembly and cache the modified version.</summary>
        /// <param name="assemblyPath">The assembly file path.</param>
        public void ProcessAssembly(string assemblyPath)
        {
            // read assembly data
            string assemblyFileName = Path.GetFileName(assemblyPath);
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            byte[] assemblyBytes = File.ReadAllBytes(assemblyPath);
            string hash = $"SMAPI {Constants.Version}|" + string.Join("", MD5.Create().ComputeHash(assemblyBytes).Select(p => p.ToString("X2")));

            // check cache
            CachePaths cachePaths = this.GetCacheInfo(assemblyPath);
            bool canUseCache = File.Exists(cachePaths.Assembly) && File.Exists(cachePaths.Hash) && hash == File.ReadAllText(cachePaths.Hash);

            // process assembly if not cached
            if (!canUseCache)
            {
                this.Monitor.Log($"Loading {assemblyFileName} for the first time; preprocessing...");

                // read assembly definition
                AssemblyDefinition assembly;
                using (Stream readStream = new MemoryStream(assemblyBytes))
                    assembly = AssemblyDefinition.ReadAssembly(readStream);

                // rewrite assembly to match platform
                this.AssemblyTypeRewriter.RewriteAssembly(assembly);

                // write cache
                using (MemoryStream outStream = new MemoryStream())
                {
                    // get assembly bytes
                    assembly.Write(outStream);
                    byte[] outBytes = outStream.ToArray();

                    // write assembly data
                    Directory.CreateDirectory(cachePaths.Directory);
                    File.WriteAllBytes(cachePaths.Assembly, outBytes);
                    File.WriteAllText(cachePaths.Hash, hash);

                    // copy any mdb/pdb files
                    foreach (string path in Directory.GetFiles(assemblyDir, "*.mdb").Concat(Directory.GetFiles(assemblyDir, "*.pdb")))
                    {
                        string filename = Path.GetFileName(path);
                        File.Copy(path, Path.Combine(cachePaths.Directory, filename), overwrite: true);
                    }
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
            return Assembly.UnsafeLoadFrom(cachePaths.Assembly); // unsafe load allows DLLs downloaded from the Internet without the user needing to 'unblock' them
        }

        /// <summary>Resolve an assembly from its name.</summary>
        /// <param name="shortName">The short assembly name.</param>
        public Assembly ResolveAssembly(string shortName)
        {
            return this.AssemblyMap.Targets.FirstOrDefault(p => p.GetName().Name == shortName);
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
    }
}
