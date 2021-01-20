using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI.Toolkit.Serialization;
using StardewModdingAPI.Utilities;

namespace StardewModdingAPI.Framework.Serialization
{
    /// <summary>Handles deserialization of <see cref="Keybind"/> and <see cref="KeybindList"/> models.</summary>
    internal class KeybindConverter : JsonConverter
    {
        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public override bool CanRead { get; } = true;

        /// <inheritdoc />
        public override bool CanWrite { get; } = true;


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether this instance can convert the specified object type.</summary>
        /// <param name="objectType">The object type.</param>
        public override bool CanConvert(Type objectType)
        {
            return
                typeof(Keybind).IsAssignableFrom(objectType)
                || typeof(KeybindList).IsAssignableFrom(objectType);
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="existingValue">The object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string path = reader.Path;

            // validate JSON type
            if (reader.TokenType != JsonToken.String)
                throw new SParseException($"Can't parse {nameof(KeybindList)} from {reader.TokenType} node (path: {reader.Path}).");

            // parse raw value
            string str = JToken.Load(reader).Value<string>();
            if (objectType == typeof(Keybind))
            {
                return Keybind.TryParse(str, out Keybind parsed, out string[] errors)
                    ? parsed
                    : throw new SParseException($"Can't parse {nameof(Keybind)} from invalid value '{str}' (path: {path}).\n{string.Join("\n", errors)}");
            }

            if (objectType == typeof(KeybindList))
            {
                return KeybindList.TryParse(str, out KeybindList parsed, out string[] errors)
                    ? parsed
                    : throw new SParseException($"Can't parse {nameof(KeybindList)} from invalid value '{str}' (path: {path}).\n{string.Join("\n", errors)}");
            }

            throw new SParseException($"Can't parse unexpected type {objectType} from {reader.TokenType} node (path: {reader.Path}).");
        }

        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The JSON writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Read a JSON string.</summary>
        /// <param name="str">The JSON string value.</param>
        /// <param name="path">The path to the current JSON node.</param>
        protected KeybindList ReadString(string str, string path)
        {
            return KeybindList.TryParse(str, out KeybindList parsed, out string[] errors)
                ? parsed
                : throw new SParseException($"Can't parse {nameof(KeybindList)} from invalid value '{str}' (path: {path}).\n{string.Join("\n", errors)}");
        }
    }
}
