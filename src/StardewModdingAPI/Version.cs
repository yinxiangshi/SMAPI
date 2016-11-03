using System;
using Newtonsoft.Json;

namespace StardewModdingAPI
{
    /// <summary>A semantic version with an optional release tag.</summary>
    public struct Version
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The major version incremented for major API changes.</summary>
        public int MajorVersion { get; set; }

        /// <summary>The minor version incremented for backwards-compatible changes.</summary>
        public int MinorVersion { get; set; }

        /// <summary>The patch version for backwards-compatible bug fixes.</summary>
        public int PatchVersion { get; set; }

        /// <summary>An optional build tag.</summary>
        public string Build { get; set; }

        /// <summary>Obsolete.</summary>
        [JsonIgnore]
        [Obsolete("Use `Version.ToString()` instead.")]
        public string VersionString => this.ToString();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="major">The major version incremented for major API changes.</param>
        /// <param name="minor">The minor version incremented for backwards-compatible changes.</param>
        /// <param name="patch">The patch version for backwards-compatible bug fixes.</param>
        /// <param name="build">An optional build tag.</param>
        public Version(int major, int minor, int patch, string build)
        {
            this.MajorVersion = major;
            this.MinorVersion = minor;
            this.PatchVersion = patch;
            this.Build = build;
        }

        /// <summary>Get a string representation of the version.</summary>
        public override string ToString()
        {
            // version
            string result = this.PatchVersion != 0
                ? $"{this.MajorVersion}.{this.MinorVersion}.{this.PatchVersion}"
                : $"{this.MajorVersion}.{this.MinorVersion}";

            // tag
            string tag = this.GetNormalisedTag(this.Build);
            if (tag != null)
                result += $"-{tag}";
            return result;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a normalised build tag.</summary>
        /// <param name="tag">The tag to normalise.</param>
        private string GetNormalisedTag(string tag)
        {
            tag = tag?.Trim().Trim('-');
            if (string.IsNullOrWhiteSpace(tag) || tag == "0")
                return null;
            return tag;
        }
    }
}
