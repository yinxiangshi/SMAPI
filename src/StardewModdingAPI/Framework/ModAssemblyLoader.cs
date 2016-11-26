using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;
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

        /// <summary>Rewrites assembly types to match the current platform.</summary>
        private readonly AssemblyTypeRewriter AssemblyTypeRewriter;

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
            this.AssemblyTypeRewriter = this.GetAssemblyRewriter(targetPlatform);
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
                AssemblyDefinition assembly;
                using (Stream readStream = new MemoryStream(assemblyBytes))
                    assembly = AssemblyDefinition.ReadAssembly(readStream);

                // rewrite assembly to match platform
                this.AssemblyTypeRewriter.RewriteAssembly(assembly);

                // write cache
                using (MemoryStream outStream = new MemoryStream())
                {
                    assembly.Write(outStream);
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

        /// <summary>Get an assembly rewriter for the target platform.</summary>
        /// <param name="targetPlatform">The target game platform.</param>
        private AssemblyTypeRewriter GetAssemblyRewriter(Platform targetPlatform)
        {
            // get assembly changes needed for platform
            string[] removeAssemblyReferences;
            Assembly[] targetAssemblies;
            switch (targetPlatform)
            {
                case Platform.Mono:
                    removeAssemblyReferences = new[]
                    {
                        "Stardew Valley",
                        "Microsoft.Xna.Framework",
                        "Microsoft.Xna.Framework.Game",
                        "Microsoft.Xna.Framework.Graphics"
                    };
                    targetAssemblies = new[]
                    {
                        typeof(StardewValley.Game1).Assembly,
                        typeof(Microsoft.Xna.Framework.Vector2).Assembly
                    };
                    break;

                case Platform.Windows:
                    removeAssemblyReferences = new[]
                    {
                        "StardewValley",
                        "MonoGame.Framework"
                    };
                    targetAssemblies = new[]
                    {
                        typeof(StardewValley.Game1).Assembly,
                        typeof(Microsoft.Xna.Framework.Vector2).Assembly,
                        typeof(Microsoft.Xna.Framework.Game).Assembly,
                        typeof(Microsoft.Xna.Framework.Graphics.SpriteBatch).Assembly
                    };
                    break;

                default:
                    throw new InvalidOperationException($"Unknown target platform '{targetPlatform}'.");
            }

            return new AssemblyTypeRewriter(targetAssemblies, removeAssemblyReferences);
        }
    }
}
