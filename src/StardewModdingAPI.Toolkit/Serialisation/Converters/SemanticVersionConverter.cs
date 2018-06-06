using Newtonsoft.Json.Linq;
using StardewModdingAPI.Toolkit.Serialisation.Models;

namespace StardewModdingAPI.Toolkit.Serialisation.Converters
{
    /// <summary>Handles deserialisation of <see cref="SemanticVersion"/>.</summary>
    internal class SemanticVersionConverter : SimpleReadOnlyConverter<SemanticVersion>
    {
        /*********
        ** Protected methods
        *********/
        /// <summary>Read a JSON object.</summary>
        /// <param name="obj">The JSON object to read.</param>
        /// <param name="path">The path to the current JSON node.</param>
        protected override SemanticVersion ReadObject(JObject obj, string path)
        {
            int major = obj.ValueIgnoreCase<int>("MajorVersion");
            int minor = obj.ValueIgnoreCase<int>("MinorVersion");
            int patch = obj.ValueIgnoreCase<int>("PatchVersion");
            string build = obj.ValueIgnoreCase<string>("Build");
            return new LegacyManifestVersion(major, minor, patch, build);
        }

        /// <summary>Read a JSON string.</summary>
        /// <param name="str">The JSON string value.</param>
        /// <param name="path">The path to the current JSON node.</param>
        protected override SemanticVersion ReadString(string str, string path)
        {
            if (string.IsNullOrWhiteSpace(str))
                return null;
            if (!SemanticVersion.TryParse(str, out ISemanticVersion version))
                throw new SParseException($"Can't parse semantic version from invalid value '{str}', should be formatted like 1.2, 1.2.30, or 1.2.30-beta (path: {path}).");
            return (SemanticVersion)version;
        }
    }
}
