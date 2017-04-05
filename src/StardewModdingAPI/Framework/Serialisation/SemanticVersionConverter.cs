using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StardewModdingAPI.Framework.Serialisation
{
    /// <summary>Overrides how SMAPI reads and writes <see cref="ISemanticVersion"/>.</summary>
    internal class SemanticVersionConverter : JsonConverter
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
            return objectType == typeof(ISemanticVersion);
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="existingValue">The object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            int major = obj.Value<int>("MajorVersion");
            int minor = obj.Value<int>("MinorVersion");
            int patch = obj.Value<int>("PatchVersion");
            string build = obj.Value<string>("Build");
            return new SemanticVersion(major, minor, patch, build);
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
