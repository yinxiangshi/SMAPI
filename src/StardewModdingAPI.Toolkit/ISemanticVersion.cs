using System;

namespace StardewModdingAPI.Toolkit
{
    /// <summary>A semantic version with an optional release tag.</summary>
    public interface ISemanticVersion : IComparable<ISemanticVersion>, IEquatable<ISemanticVersion>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The major version incremented for major API changes.</summary>
        int Major { get; }

        /// <summary>The minor version incremented for backwards-compatible changes.</summary>
        int Minor { get; }

        /// <summary>The patch version for backwards-compatible bug fixes.</summary>
        int Patch { get; }

        /// <summary>An optional prerelease tag.</summary>
        string Tag { get; }


        /*********
        ** Accessors
        *********/
        /// <summary>Whether this is a pre-release version.</summary>
        bool IsPrerelease();

        /// <summary>Get whether this version is older than the specified version.</summary>
        /// <param name="other">The version to compare with this instance.</param>
        bool IsOlderThan(ISemanticVersion other);

        /// <summary>Get whether this version is newer than the specified version.</summary>
        /// <param name="other">The version to compare with this instance.</param>
        bool IsNewerThan(ISemanticVersion other);

        /// <summary>Get whether this version is between two specified versions (inclusively).</summary>
        /// <param name="min">The minimum version.</param>
        /// <param name="max">The maximum version.</param>
        bool IsBetween(ISemanticVersion min, ISemanticVersion max);

        /// <summary>Get a string representation of the version.</summary>
        string ToString();
    }
}
