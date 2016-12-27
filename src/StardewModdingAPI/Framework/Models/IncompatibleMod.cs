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

        /// <summary>The most recent incompatible mod version.</summary>
        public string Version { get; set; }

        /// <summary>The URL the user can check for an official updated version.</summary>
        public string UpdateUrl { get; set; }

        /// <summary>The URL the user can check for an unofficial updated version.</summary>
        public string UnofficialUpdateUrl { get; set; }

        /// <summary>A regular expression matching version strings to consider compatible, even if they technically precede <see cref="Version"/>.</summary>
        public string ForceCompatibleVersion { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the specified version is compatible according to this metadata.</summary>
        /// <param name="version">The current version of the matching mod.</param>
        public bool IsCompatible(ISemanticVersion version)
        {
            ISemanticVersion incompatibleVersion = new SemanticVersion(this.Version);

            // allow newer versions
            if (version.IsNewerThan(incompatibleVersion))
                return true;

            // allow versions matching override
            return !string.IsNullOrWhiteSpace(this.ForceCompatibleVersion) && Regex.IsMatch(version.ToString(), this.ForceCompatibleVersion, RegexOptions.IgnoreCase);
        }
    }
}