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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The download's display name.</param>
        /// <param name="description">The download's description.</param>
        /// <param name="version">The download's file version.</param>
        public GenericModDownload(string name, string? description, string? version)
        {
            this.Name = name;
            this.Description = description;
            this.Version = version;
        }

        /// <summary>
        ///   Return true if the subkey matches this download.  A subkey matches if it appears as
        ///   a substring in the name or description.
        /// </summary>
        /// <param name="subkey">the subkey</param>
        /// <returns><c>true</c> if <paramref name="subkey"/> matches this download, otherwise <c>false</c></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool MatchesSubkey(string subkey) {
            return this.Name.Contains(subkey, StringComparison.OrdinalIgnoreCase) == true
                || this.Description?.Contains(subkey, StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}
