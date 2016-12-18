using System;

namespace StardewModdingAPI
{
    /// <summary>A semantic version with an optional release tag.</summary>
    public interface ISemanticVersion : IComparable<ISemanticVersion>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The major version incremented for major API changes.</summary>
        int MajorVersion { get; }

        /// <summary>The minor version incremented for backwards-compatible changes.</summary>
        int MinorVersion { get; }

        /// <summary>The patch version for backwards-compatible bug fixes.</summary>
        int PatchVersion { get; }

        /// <summary>An optional build tag.</summary>
        string Build { get; }


        /*********
        ** Accessors
        *********/
        /// <summary>Get whether this version is older than the specified version.</summary>
        /// <param name="other">The version to compare with this instance.</param>
        bool IsOlderThan(ISemanticVersion other);

        /// <summary>Get whether this version is newer than the specified version.</summary>
        /// <param name="other">The version to compare with this instance.</param>
        bool IsNewerThan(ISemanticVersion other);
    }
}