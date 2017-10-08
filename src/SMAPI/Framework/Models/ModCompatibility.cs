using System;

namespace StardewModdingAPI.Framework.Models
{
    /// <summary>Specifies the compatibility of a given mod version range.</summary>
    internal class ModCompatibility
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The lowest version in the range, or <c>null</c> for all past versions.</summary>
        public ISemanticVersion LowerVersion { get; }

        /// <summary>The highest version in the range, or <c>null</c> for all future versions.</summary>
        public ISemanticVersion UpperVersion { get; }

        /// <summary>The mod compatibility.</summary>
        public ModStatus Status { get; }

        /// <summary>The reason phrase to show in log output, or <c>null</c> to use the default value.</summary>
        /// <example>For example, "this version is incompatible with the latest version of the game".</example>
        public string ReasonPhrase { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="versionRange">A version range, which consists of two version strings separated by a '~' character. Either side can be left blank for an unbounded range.</param>
        /// <param name="status">The mod compatibility.</param>
        /// <param name="reasonPhrase">The reason phrase to show in log output, or <c>null</c> to use the default value.</param>
        public ModCompatibility(string versionRange, ModStatus status, string reasonPhrase)
        {
            // extract version strings
            string[] versions = versionRange.Split('~');
            if (versions.Length != 2)
                throw new FormatException($"Could not parse '{versionRange}' as a version range. It must have two version strings separated by a '~' character (either side can be left blank for an unbounded range).");

            // initialise
            this.LowerVersion = !string.IsNullOrWhiteSpace(versions[0]) ? new SemanticVersion(versions[0]) : null;
            this.UpperVersion = !string.IsNullOrWhiteSpace(versions[1]) ? new SemanticVersion(versions[1]) : null;
            this.Status = status;
            this.ReasonPhrase = reasonPhrase;
        }

        /// <summary>Get whether a given version is contained within this compatibility range.</summary>
        /// <param name="version">The version to check.</param>
        public bool MatchesVersion(ISemanticVersion version)
        {
            return
                (this.LowerVersion == null || !version.IsOlderThan(this.LowerVersion))
                && (this.UpperVersion == null || !version.IsNewerThan(this.UpperVersion));
        }
    }
}
