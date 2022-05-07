namespace StardewModdingAPI.Toolkit.Utilities.PathLookups
{
    /// <summary>An API for relative path lookups within a root directory with minimal preprocessing.</summary>
    internal class MinimalPathLookup : IFilePathLookup
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A singleton instance for reuse.</summary>
        public static readonly MinimalPathLookup Instance = new();


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public string GetFilePath(string relativePath)
        {
            return PathUtilities.NormalizePath(relativePath);
        }

        /// <inheritdoc />
        public string GetAssetName(string relativePath)
        {
            return PathUtilities.NormalizeAssetName(relativePath);
        }

        /// <inheritdoc />
        public void Add(string relativePath) { }
    }
}
