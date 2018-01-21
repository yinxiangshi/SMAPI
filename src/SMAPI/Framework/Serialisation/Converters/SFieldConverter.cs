using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI.Framework.Exceptions;
using StardewModdingAPI.Framework.Models;

namespace StardewModdingAPI.Framework.Serialisation.Converters
{
    /// <summary>Overrides how SMAPI reads and writes <see cref="ISemanticVersion"/> and <see cref="IManifestDependency"/> fields.</summary>
    internal class SFieldConverter : JsonConverter
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether this converter can write JSON.</summary>
        public override bool CanWrite => false;


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether this instance can convert the specified object type.</summary>
        /// <param name="objectType">The object type.</param>
        public override bool CanConvert(Type objectType)
        {
            return
                objectType == typeof(ISemanticVersion)
                || objectType == typeof(IManifestDependency[])
                || objectType == typeof(ModDataID)
                || objectType == typeof(ModCompatibility[]);
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="existingValue">The object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // semantic version
            if (objectType == typeof(ISemanticVersion))
            {
                JToken token = JToken.Load(reader);
                switch (token.Type)
                {
                    case JTokenType.Object:
                        {
                            JObject obj = (JObject)token;
                            int major = obj.Value<int>(nameof(ISemanticVersion.MajorVersion));
                            int minor = obj.Value<int>(nameof(ISemanticVersion.MinorVersion));
                            int patch = obj.Value<int>(nameof(ISemanticVersion.PatchVersion));
                            string build = obj.Value<string>(nameof(ISemanticVersion.Build));
                            return new LegacyManifestVersion(major, minor, patch, build);
                        }

                    case JTokenType.String:
                        {
                            string str = token.Value<string>();
                            if (string.IsNullOrWhiteSpace(str))
                                return null;
                            if (!SemanticVersion.TryParse(str, out ISemanticVersion version))
                                throw new SParseException($"Can't parse semantic version from invalid value '{str}', should be formatted like 1.2, 1.2.30, or 1.2.30-beta.");
                            return version;
                        }

                    default:
                        throw new SParseException($"Can't parse semantic version from {token.Type}, must be an object or string.");
                }
            }

            // manifest dependencies
            if (objectType == typeof(IManifestDependency[]))
            {
                List<IManifestDependency> result = new List<IManifestDependency>();
                foreach (JObject obj in JArray.Load(reader).Children<JObject>())
                {
                    string uniqueID = obj.Value<string>(nameof(IManifestDependency.UniqueID));
                    string minVersion = obj.Value<string>(nameof(IManifestDependency.MinimumVersion));
                    bool required = obj.Value<bool?>(nameof(IManifestDependency.IsRequired)) ?? true;
                    result.Add(new ManifestDependency(uniqueID, minVersion, required));
                }
                return result.ToArray();
            }

            // mod data ID
            if (objectType == typeof(ModDataID))
            {
                JToken token = JToken.Load(reader);
                return new ModDataID(token.Value<string>());
            }

            // mod compatibility records
            if (objectType == typeof(ModCompatibility[]))
            {
                List<ModCompatibility> result = new List<ModCompatibility>();
                foreach (JProperty property in JObject.Load(reader).Properties())
                {
                    string range = property.Name;
                    ModStatus status = (ModStatus)Enum.Parse(typeof(ModStatus), property.Value.Value<string>(nameof(ModCompatibility.Status)));
                    string reasonPhrase = property.Value.Value<string>(nameof(ModCompatibility.ReasonPhrase));

                    result.Add(new ModCompatibility(range, status, reasonPhrase));
                }
                return result.ToArray();
            }

            // unknown
            throw new NotSupportedException($"Unknown type '{objectType?.FullName}'.");
        }

        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The JSON writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("This converter does not write JSON.");
        }
    }
}
