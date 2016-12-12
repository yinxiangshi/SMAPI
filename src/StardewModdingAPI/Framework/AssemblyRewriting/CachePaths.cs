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

        /// <summary>The file path containing the assembly metadata.</summary>
        public string Metadata { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="directory">The directory path which contains the assembly.</param>
        /// <param name="assembly">The file path of the assembly file.</param>
        /// <param name="metadata">The file path containing the assembly metadata.</param>
        public CachePaths(string directory, string assembly, string metadata)
        {
            this.Directory = directory;
            this.Assembly = assembly;
            this.Metadata = metadata;
        }
    }
}