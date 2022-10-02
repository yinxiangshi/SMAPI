namespace StardewModdingAPI.Web.Framework
{
    /// <summary>Generic metadata about a file download on a mod page.</summary>
    internal interface IModDownload
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The download's display name.</summary>
        string Name { get; }

        /// <summary>The download's description.</summary>
        string? Description { get; }

        /// <summary>The download's file version.</summary>
        string? Version { get; }

        /// <summary>This download's URL (if it has a URL that is different from the containing mod page's URL).</summary>
        string? Url { get; }

        /// <summary>Return <see langword="true"/> iff the subkey matches this download</summary>
        /// <param name="subkey">the subkey</param>
        /// <returns><see langword="true"/> if <paramref name="subkey"/> matches this download, otherwise <see langword="false"/></returns>
        bool MatchesSubkey(string subkey);
    }
}
