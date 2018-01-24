using Newtonsoft.Json;

namespace StardewModdingAPI.Framework
{
    /// <summary>An implementation of <see cref="ISemanticVersion"/> that hamdles the legacy <see cref="IManifest"/> version format.</summary>
    internal class LegacyManifestVersion : SemanticVersion
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="majorVersion">The major version incremented for major API changes.</param>
        /// <param name="minorVersion">The minor version incremented for backwards-compatible changes.</param>
        /// <param name="patchVersion">The patch version for backwards-compatible bug fixes.</param>
        /// <param name="build">An optional build tag.</param>
        [JsonConstructor]
        public LegacyManifestVersion(int majorVersion, int minorVersion, int patchVersion, string build = null)
            : base(
                majorVersion,
                minorVersion,
                patchVersion,
                build != "0" ? build : null // '0' from incorrect examples in old SMAPI documentation
            )
        { }
    }
}
