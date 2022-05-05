namespace StardewModdingAPI.Toolkit.Utilities.PathLookups
{
    /// <summary>An API for relative path lookups within a root directory.</summary>
    internal interface IFilePathLookup
    {
        /// <summary>Get the actual path for a given relative file path.</summary>
        /// <param name="relativePath">The relative path.</param>
        /// <remarks>Returns the resolved path in file path format, else the normalized <paramref name="relativePath"/>.</remarks>
        string GetFilePath(string relativePath);

        /// <summary>Get the actual path for a given asset name.</summary>
        /// <param name="relativePath">The relative path.</param>
        /// <remarks>Returns the resolved path in asset name format, else the normalized <paramref name="relativePath"/>.</remarks>
        string GetAssetName(string relativePath);

        /// <summary>Add a relative path that was just created by a SMAPI API.</summary>
        /// <param name="relativePath">The relative path. This must already be normalized in asset name or file path format.</param>
        void Add(string relativePath);
    }
}
