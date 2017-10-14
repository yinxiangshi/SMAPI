using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StardewModdingAPI.Framework.Serialisation;

namespace StardewModdingAPI.Framework.Models
{
    /// <summary>Metadata about a mod from SMAPI's internal data.</summary>
    internal class ModDataRecord
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique mod identifier.</summary>
        [JsonConverter(typeof(SFieldConverter))]
        public ModDataID ID { get; set; }

        /// <summary>A value to inject into <see cref="IManifest.UpdateKeys"/> field if it's not already set.</summary>
        public string[] UpdateKeys { get; set; }

        /// <summary>The URL where the player can get an unofficial or alternative version of the mod if the official version isn't compatible.</summary>
        public string AlternativeUrl { get; set; }

        /// <summary>The compatibility of given mod versions (if any).</summary>
        [JsonConverter(typeof(SFieldConverter))]
        public ModCompatibility[] Compatibility { get; set; } = new ModCompatibility[0];

        /// <summary>Map local versions to a semantic version for update checks.</summary>
        public IDictionary<string, string> MapLocalVersions { get; set; } = new Dictionary<string, string>();

        /// <summary>Map remote versions to a semantic version for update checks.</summary>
        public IDictionary<string, string> MapRemoteVersions { get; set; } = new Dictionary<string, string>();


        /*********
        ** Public methods
        *********/
        /// <summary>Get the compatibility record for a given version, if any.</summary>
        /// <param name="version">The mod version to check.</param>
        public ModCompatibility GetCompatibility(ISemanticVersion version)
        {
            return this.Compatibility.FirstOrDefault(p => p.MatchesVersion(version));
        }

        /// <summary>Get a semantic local version for update checks.</summary>
        /// <param name="version">The local version to normalise.</param>
        public string GetLocalVersionForUpdateChecks(string version)
        {
            return this.MapLocalVersions != null && this.MapLocalVersions.TryGetValue(version, out string newVersion)
                ? newVersion
                : version;
        }

        /// <summary>Get a semantic remote version for update checks.</summary>
        /// <param name="version">The remote version to normalise.</param>
        public string GetRemoteVersionForUpdateChecks(string version)
        {
            return this.MapRemoteVersions != null && this.MapRemoteVersions.TryGetValue(version, out string newVersion)
                ? newVersion
                : version;
        }
    }
}
