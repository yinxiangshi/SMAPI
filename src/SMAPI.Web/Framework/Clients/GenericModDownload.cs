using System;

namespace StardewModdingAPI.Web.Framework.Clients
{
    /// <summary>Generic metadata about a file download on a mod page.</summary>
    internal class GenericModDownload : IModDownload
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The download's display name.</summary>
        public string Name { get; }

        /// <summary>The download's description.</summary>
        public string? Description { get; }

        /// <summary>The download's file version.</summary>
        public string? Version { get; }

        /// <summary>
        ///   The URL for this download, if it has one distinct from the mod page's URL.
        /// </summary>
        public string? Url { get; }

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The download's display name.</param>
        /// <param name="description">The download's description.</param>
        /// <param name="version">The download's file version.</param>
        /// <param name="url">The download's URL (if different from the mod page's URL).</param>
        public GenericModDownload(string name, string? description, string? version, string? url = null)
        {
            this.Name = name;
            this.Description = description;
            this.Version = version;
            this.Url = url;
        }

        /// <summary>
        ///   Return <see langword="true"/> if the subkey matches this download.  A subkey matches if it appears as
        ///   a substring in the name or description.
        /// </summary>
        /// <param name="subkey">the subkey</param>
        /// <returns><see langword="true"/> if <paramref name="subkey"/> matches this download, otherwise <see langword="false"/></returns>
        public virtual bool MatchesSubkey(string subkey) {
            return this.Name.Contains(subkey, StringComparison.OrdinalIgnoreCase) == true
                || this.Description?.Contains(subkey, StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}
