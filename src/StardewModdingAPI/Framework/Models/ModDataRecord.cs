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
