namespace StardewModdingAPI.Framework.AssemblyRewriting
{
    /// <summary>Contains the paths for an assembly's cached data.</summary>
    internal struct CachePaths
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