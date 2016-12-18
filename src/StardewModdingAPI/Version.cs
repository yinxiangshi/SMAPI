using System;
using Newtonsoft.Json;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI
{
    /// <summary>A semantic version with an optional release tag.</summary>
    [Obsolete("Use " + nameof(SemanticVersion) + " or " + nameof(Manifest) + "." + nameof(Manifest.Version) + " instead")]
    public struct Version : ISemanticVersion
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
        [Obsolete("Use " + nameof(Version) + "." + nameof(Version.ToString) + " instead.")]
        public string VersionString
        {
            get
            {
                Program.DeprecationManager.Warn($"{nameof(Version)}.{nameof(Version.VersionString)}", "1.0", DeprecationLevel.Notice);
                return this.GetSemanticVersion().ToString();
            }
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="major">The major version incremented for major API changes.</param>
        /// <param name="minor">The minor version incremented for backwards-compatible changes.</param>
        /// <param name="patch">The patch version for backwards-compatible bug fixes.</param>
        /// <param name="build">An optional build tag.</param>
        public Version(int major, int minor, int patch, string build)
            : this(major, minor, patch, build, suppressDeprecationWarning: false)
        { }

        /// <summary>Get an integer indicating whether this version precedes (less than 0), supercedes (more than 0), or is equivalent to (0) the specified version.</summary>
        /// <param name="other">The version to compare with this instance.</param>
        public int CompareTo(Version other)
        {
            return this.GetSemanticVersion().CompareTo(other);
        }

        /// <summary>Get whether this version is newer than the specified version.</summary>
        /// <param name="other">The version to compare with this instance.</param>
        [Obsolete("Use " + nameof(ISemanticVersion) + "." + nameof(ISemanticVersion.IsNewerThan) + " instead")]
        public bool IsNewerThan(Version other)
        {
            return this.GetSemanticVersion().IsNewerThan(other);
        }

        /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object. </summary>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="other" /> in the sort order.  Zero This instance occurs in the same position in the sort order as <paramref name="other" />. Greater than zero This instance follows <paramref name="other" /> in the sort order. </returns>
        /// <param name="other">An object to compare with this instance. </param>
        int IComparable<ISemanticVersion>.CompareTo(ISemanticVersion other)
        {
            return this.GetSemanticVersion().CompareTo(other);
        }

        /// <summary>Get whether this version is older than the specified version.</summary>
        /// <param name="other">The version to compare with this instance.</param>
        bool ISemanticVersion.IsOlderThan(ISemanticVersion other)
        {
            return this.GetSemanticVersion().IsOlderThan(other);
        }

        /// <summary>Get whether this version is newer than the specified version.</summary>
        /// <param name="other">The version to compare with this instance.</param>
        bool ISemanticVersion.IsNewerThan(ISemanticVersion other)
        {
            return this.GetSemanticVersion().IsNewerThan(other);
        }

        /// <summary>Get a string representation of the version.</summary>
        public override string ToString()
        {
            return this.GetSemanticVersion().ToString();
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="major">The major version incremented for major API changes.</param>
        /// <param name="minor">The minor version incremented for backwards-compatible changes.</param>
        /// <param name="patch">The patch version for backwards-compatible bug fixes.</param>
        /// <param name="build">An optional build tag.</param>
        /// <param name="suppressDeprecationWarning">Whether to suppress the deprecation warning.</param>
        internal Version(int major, int minor, int patch, string build, bool suppressDeprecationWarning)
        {
            if (!suppressDeprecationWarning)
                Program.DeprecationManager.Warn($"{nameof(Version)}", "1.5", DeprecationLevel.Notice);

            this.MajorVersion = major;
            this.MinorVersion = minor;
            this.PatchVersion = patch;
            this.Build = build;
        }

        /// <summary>Get the equivalent semantic version.</summary>
        /// <remarks>This is a hack so the struct can wrap <see cref="SemanticVersion"/> without a mutable backing field, which would cause a <see cref="StackOverflowException"/> due to recreating the struct value on each change.</remarks>
        private SemanticVersion GetSemanticVersion()
        {
            return new SemanticVersion(this.MajorVersion, this.MinorVersion, this.PatchVersion, this.Build);
        }
    }
}
