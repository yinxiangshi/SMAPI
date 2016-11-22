using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;

namespace StardewModdingAPI.Framework
{
    /// <summary>Loads mod assemblies.</summary>
    internal class ModAssemblyLoader
    {
        /*********
        ** Properties
        *********/
        /// <summary>The directory in which to cache data.</summary>
        private readonly string CacheDirPath;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="cacheDirPath">The cache directory.</param>
        public ModAssemblyLoader(string cacheDirPath)
        {
            this.CacheDirPath = cacheDirPath;
        }

        /// <summary>Read an assembly from the given path.</summary>
        /// <param name="assemblyPath">The assembly file path.</param>
        public Assembly ProcessAssembly(string assemblyPath)
        {
            // read assembly data
            byte[] assemblyBytes = File.ReadAllBytes(assemblyPath);
            byte[] hash = MD5.Create().ComputeHash(assemblyBytes);

            // get cache data
            string key = Path.GetFileNameWithoutExtension(assemblyPath);
            string cachePath = Path.Combine(this.CacheDirPath, $"{key}.dll");
            string cacheHashPath = Path.Combine(this.CacheDirPath, $"{key}-hash.txt");
            bool canUseCache = File.Exists(cachePath) && File.Exists(cacheHashPath) && hash.SequenceEqual(File.ReadAllBytes(cacheHashPath));

            // process assembly if not cached
            if (!canUseCache)
            {
                // read assembly definition
                AssemblyDefinition definition;
                using (Stream readStream = new MemoryStream(assemblyBytes))
                    definition = AssemblyDefinition.ReadAssembly(readStream);

                // write cache
                using (MemoryStream outStream = new MemoryStream())
                {
                    definition.Write(outStream);
                    byte[] outBytes = outStream.ToArray();
                    File.WriteAllBytes(cachePath, outBytes);
                    File.WriteAllBytes(cacheHashPath, hash);
                }
            }

            // load assembly
            return Assembly.UnsafeLoadFrom(cachePath);
        }
    }
}
