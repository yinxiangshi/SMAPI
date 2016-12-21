using System;
using System.Text.RegularExpressions;

namespace StardewModdingAPI
{
    /// <summary>A semantic version with an optional release tag.</summary>
    public class SemanticVersion : ISemanticVersion
    {
        /*********
        ** Properties
        *********/
        /// <summary>A regular expression matching a semantic version string.</summary>
        /// <remarks>Derived from https://github.com/maxhauser/semver.</remarks>
        private static readonly Regex Regex = new Regex(@"^(?<major>\d+)(\.(?<minor>\d+))?(\.(?<patch>\d+))?(?<build>.*)$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);


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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="major">The major version incremented for major API changes.</param>
        /// <param name="minor">The minor version incremented for backwards-compatible changes.</param>
        /// <param name="patch">The patch version for backwards-compatible bug fixes.</param>
        /// <param name="build">An optional build tag.</param>
        public SemanticVersion(int major, int minor, int patch, string build = null)
        {
            this.MajorVersion = major;
            this.MinorVersion = minor;
            this.PatchVersion = patch;
            this.Build = build;
        }

        /// <summary>Get an integer indicating whether this version precedes (less than 0), supercedes (more than 0), or is equivalent to (0) the specified version.</summary>
        /// <param name="other">The version to compare with this instance.</param>
        /// <remarks>The implementation is defined by Semantic Version 2.0 (http://semver.org/).</remarks>
        public int CompareTo(ISemanticVersion other)
        {
            const int same = 0;
            const int curNewer = 1;
            const int curOlder = -1;

            // compare stable versions
            if (this.MajorVersion != other.MajorVersion)
                return this.MajorVersion.CompareTo(other.MajorVersion);
            if (this.MinorVersion != other.MinorVersion)
                return this.MinorVersion.CompareTo(other.MinorVersion);
            if (this.PatchVersion != other.PatchVersion)
                return this.PatchVersion.CompareTo(other.PatchVersion);
            if (this.Build == other.Build)
                return same;

            // stable supercedes pre-release
            bool curIsStable = string.IsNullOrWhiteSpace(this.Build);
            bool otherIsStable = string.IsNullOrWhiteSpace(other.Build);
            if (curIsStable)
                return curNewer;
            if (otherIsStable)
                return curOlder;

            // compare two pre-release tag values
            string[] curParts = this.Build.Split('.');
            string[] otherParts = other.Build.Split('.');
            for (int i = 0; i < curParts.Length; i++)
            {
                // longer prerelease tag supercedes if otherwise equal
                if (otherParts.Length <= i)
                    return curNewer;

                // compare if different
                if (curParts[i] != otherParts[i])
                {
                    // compare numerically if possible
                    {
                        int curNum, otherNum;
                        if (int.TryParse(curParts[i], out curNum) && int.TryParse(otherParts[i], out otherNum))
                            return curNum.CompareTo(otherNum);
                    }

                    // else compare lexically
                    return string.Compare(curParts[i], otherParts[i], StringComparison.OrdinalIgnoreCase);
                }
            }

            // fallback (this should never happen)
            return string.Compare(this.ToString(), other.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>Get whether this version is older than the specified version.</summary>
        /// <param name="other">The version to compare with this instance.</param>
        public bool IsOlderThan(ISemanticVersion other)
        {
            return this.CompareTo(other) < 0;
        }

        /// <summary>Get whether this version is newer than the specified version.</summary>
        /// <param name="other">The version to compare with this instance.</param>
        public bool IsNewerThan(ISemanticVersion other)
        {
            return this.CompareTo(other) > 0;
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
        /// <summary>Construct an instance.</summary>
        /// <param name="version">The semantic version string.</param>
        internal SemanticVersion(string version)
        {
            var match = SemanticVersion.Regex.Match(version);
            if (!match.Success)
                throw new FormatException($"The input '{version}' is not a semantic version.");

            this.MajorVersion = int.Parse(match.Groups["major"].Value);
            this.MinorVersion = match.Groups["minor"].Success ? int.Parse(match.Groups["minor"].Value) : 0;
            this.PatchVersion = match.Groups["patch"].Success ? int.Parse(match.Groups["patch"].Value) : 0;
            this.Build = (match.Groups["build"].Success ? match.Groups["build"].Value : "").Trim(' ', '-', '.');
        }

        /// <summary>Get a normalised build tag.</summary>
        /// <param name="tag">The tag to normalise.</param>
        private string GetNormalisedTag(string tag)
        {
            tag = tag?.Trim().Trim('-', '.');
            if (string.IsNullOrWhiteSpace(tag) || tag == "0")
                return null;
            return tag;
        }
    }
}