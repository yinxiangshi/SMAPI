using System;
using System.Text.RegularExpressions;

namespace StardewModdingAPI.Common
{
    /// <summary>A low-level implementation of a semantic version with an optional release tag.</summary>
    /// <remarks>The implementation is defined by Semantic Version 2.0 (http://semver.org/).</remarks>
    internal class SemanticVersionImpl
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The major version incremented for major API changes.</summary>
        public int Major { get; }

        /// <summary>The minor version incremented for backwards-compatible changes.</summary>
        public int Minor { get; }

        /// <summary>The patch version for backwards-compatible bug fixes.</summary>
        public int Patch { get; }

        /// <summary>An optional prerelease tag.</summary>
        public string Tag { get; }

        /// <summary>A regular expression matching a semantic version string.</summary>
        /// <remarks>
        /// This pattern is derived from the BNF documentation in the <a href="https://github.com/mojombo/semver">semver repo</a>,
        /// with three important deviations intended to support Stardew Valley mod conventions:
        /// - allows short-form "x.y" versions;
        /// - allows hyphens in prerelease tags as synonyms for dots (like "-unofficial-update.3");
        /// - doesn't allow '+build' suffixes.
        /// </remarks>
        internal static readonly Regex Regex = new Regex(@"^(?>(?<major>0|[1-9]\d*))\.(?>(?<minor>0|[1-9]\d*))(?>(?:\.(?<patch>0|[1-9]\d*))?)(?:-(?<prerelease>(?>[a-z0-9]+[\-\.]?)+))?$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="major">The major version incremented for major API changes.</param>
        /// <param name="minor">The minor version incremented for backwards-compatible changes.</param>
        /// <param name="patch">The patch version for backwards-compatible bug fixes.</param>
        /// <param name="tag">An optional prerelease tag.</param>
        public SemanticVersionImpl(int major, int minor, int patch, string tag = null)
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
            this.Tag = this.GetNormalisedTag(tag);
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="version">The assembly version.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="version"/> is null.</exception>
        public SemanticVersionImpl(Version version)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version), "The input version can't be null.");

            this.Major = version.Major;
            this.Minor = version.Minor;
            this.Patch = version.Build;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="version">The semantic version string.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="version"/> is null.</exception>
        /// <exception cref="FormatException">The <paramref name="version"/> is not a valid semantic version.</exception>
        public SemanticVersionImpl(string version)
        {
            // parse
            if (version == null)
                throw new ArgumentNullException(nameof(version), "The input version string can't be null.");
            var match = SemanticVersionImpl.Regex.Match(version.Trim());
            if (!match.Success)
                throw new FormatException($"The input '{version}' isn't a valid semantic version.");

            // initialise
            this.Major = int.Parse(match.Groups["major"].Value);
            this.Minor = match.Groups["minor"].Success ? int.Parse(match.Groups["minor"].Value) : 0;
            this.Patch = match.Groups["patch"].Success ? int.Parse(match.Groups["patch"].Value) : 0;
            this.Tag = match.Groups["prerelease"].Success ? this.GetNormalisedTag(match.Groups["prerelease"].Value) : null;
        }

        /// <summary>Get an integer indicating whether this version precedes (less than 0), supercedes (more than 0), or is equivalent to (0) the specified version.</summary>
        /// <param name="other">The version to compare with this instance.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="other"/> value is null.</exception>
        public int CompareTo(SemanticVersionImpl other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            return this.CompareTo(other.Major, other.Minor, other.Patch, other.Tag);
        }


        /// <summary>Get an integer indicating whether this version precedes (less than 0), supercedes (more than 0), or is equivalent to (0) the specified version.</summary>
        /// <param name="otherMajor">The major version to compare with this instance.</param>
        /// <param name="otherMinor">The minor version to compare with this instance.</param>
        /// <param name="otherPatch">The patch version to compare with this instance.</param>
        /// <param name="otherTag">The prerelease tag to compare with this instance.</param>
        public int CompareTo(int otherMajor, int otherMinor, int otherPatch, string otherTag)
        {
            const int same = 0;
            const int curNewer = 1;
            const int curOlder = -1;

            // compare stable versions
            if (this.Major != otherMajor)
                return this.Major.CompareTo(otherMajor);
            if (this.Minor != otherMinor)
                return this.Minor.CompareTo(otherMinor);
            if (this.Patch != otherPatch)
                return this.Patch.CompareTo(otherPatch);
            if (this.Tag == otherTag)
                return same;

            // stable supercedes pre-release
            bool curIsStable = string.IsNullOrWhiteSpace(this.Tag);
            bool otherIsStable = string.IsNullOrWhiteSpace(otherTag);
            if (curIsStable)
                return curNewer;
            if (otherIsStable)
                return curOlder;

            // compare two pre-release tag values
            string[] curParts = this.Tag.Split('.', '-');
            string[] otherParts = otherTag.Split('.', '-');
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
                        if (int.TryParse(curParts[i], out int curNum) && int.TryParse(otherParts[i], out int otherNum))
                            return curNum.CompareTo(otherNum);
                    }

                    // else compare lexically
                    return string.Compare(curParts[i], otherParts[i], StringComparison.OrdinalIgnoreCase);
                }
            }

            // fallback (this should never happen)
            return string.Compare(this.ToString(), new SemanticVersionImpl(otherMajor, otherMinor, otherPatch, otherTag).ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>Get a string representation of the version.</summary>
        public override string ToString()
        {
            // version
            string result = this.Patch != 0
                ? $"{this.Major}.{this.Minor}.{this.Patch}"
                : $"{this.Major}.{this.Minor}";

            // tag
            string tag = this.Tag;
            if (tag != null)
                result += $"-{tag}";
            return result;
        }

        /// <summary>Parse a version string without throwing an exception if it fails.</summary>
        /// <param name="version">The version string.</param>
        /// <param name="parsed">The parsed representation.</param>
        /// <returns>Returns whether parsing the version succeeded.</returns>
        internal static bool TryParse(string version, out SemanticVersionImpl parsed)
        {
            try
            {
                parsed = new SemanticVersionImpl(version);
                return true;
            }
            catch
            {
                parsed = null;
                return false;
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a normalised build tag.</summary>
        /// <param name="tag">The tag to normalise.</param>
        private string GetNormalisedTag(string tag)
        {
            tag = tag?.Trim();
            return !string.IsNullOrWhiteSpace(tag) ? tag : null;
        }
    }
}
