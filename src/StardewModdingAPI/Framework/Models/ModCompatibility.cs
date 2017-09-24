using Newtonsoft.Json;
using StardewModdingAPI.Framework.Serialisation;

namespace StardewModdingAPI.Framework.Models
{
    /// <summary>Metadata about a mod version that SMAPI should assume is compatible or broken, regardless of whether it detects incompatible code.</summary>
    internal class ModCompatibility
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique mod IDs.</summary>
        [JsonConverter(typeof(SFieldConverter))]
        public ModCompatibilityID[] ID { get; set; }

        /// <summary>The mod name.</summary>
        public string Name { get; set; }

        /// <summary>The oldest incompatible mod version, or <c>null</c> for all past versions.</summary>
        [JsonConverter(typeof(SFieldConverter))]
        public ISemanticVersion LowerVersion { get; set; }

        /// <summary>The most recent incompatible mod version.</summary>
        [JsonConverter(typeof(SFieldConverter))]
        public ISemanticVersion UpperVersion { get; set; }

        /// <summary>A label to show to the user instead of <see cref="UpperVersion"/>, when the manifest version differs from the user-facing version.</summary>
        public string UpperVersionLabel { get; set; }

        /// <summary>The URLs the user can check for a newer version.</summary>
        public string[] UpdateUrls { get; set; }

        /// <summary>The reason phrase to show in the warning, or <c>null</c> to use the default value.</summary>
        /// <example>"this version is incompatible with the latest version of the game"</example>
        public string ReasonPhrase { get; set; }

        /// <summary>Indicates how SMAPI should treat the mod.</summary>
        public ModStatus Status { get; set; } = ModStatus.AssumeBroken;
    }
}
