using System.Text.RegularExpressions;

namespace StardewModdingAPI.Framework.Models
{
    /// <summary>Contains abstract metadata about an incompatible mod.</summary>
    internal class IncompatibleMod
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique mod ID.</summary>
        public string ID { get; set; }

        /// <summary>The mod name.</summary>
        public string Name { get; set; }

        /// <summary>The oldest incompatible mod version, or <c>null</c> for all past versions.</summary>
        public string LowerVersion { get; set; }

        /// <summary>The most recent incompatible mod version.</summary>
        public string UpperVersion { get; set; }

        /// <summary>The URL the user can check for an official updated version.</summary>
        public string UpdateUrl { get; set; }

        /// <summary>The URL the user can check for an unofficial updated version.</summary>
        public string UnofficialUpdateUrl { get; set; }

        /// <summary>A regular expression matching version strings to consider compatible, even if they technically precede <see cref="UpperVersion"/>.</summary>
        public string ForceCompatibleVersion { get; set; }

        /// <summary>The reason phrase to show in the warning, or <c>null</c> to use the default value.</summary>
        /// <example>"this version is incompatible with the latest version of the game"</example>
        public string ReasonPhrase { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the specified version is compatible according to this metadata.</summary>
        /// <param name="version">The current version of the matching mod.</param>
        public bool IsCompatible(ISemanticVersion version)
        {
            ISemanticVersion lowerVersion = this.LowerVersion != null ? new SemanticVersion(this.LowerVersion) : null;
            ISemanticVersion upperVersion = new SemanticVersion(this.UpperVersion);

            // ignore versions not in range
            if (lowerVersion != null && version.IsOlderThan(lowerVersion))
                return true;
            if (version.IsNewerThan(upperVersion))
                return true;

            // allow versions matching override
            return !string.IsNullOrWhiteSpace(this.ForceCompatibleVersion) && Regex.IsMatch(version.ToString(), this.ForceCompatibleVersion, RegexOptions.IgnoreCase);
        }
    }
}
