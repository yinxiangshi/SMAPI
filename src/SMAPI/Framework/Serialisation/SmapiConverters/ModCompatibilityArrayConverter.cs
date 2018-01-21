using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI.Framework.Models;

namespace StardewModdingAPI.Framework.Serialisation.SmapiConverters
{
    /// <summary>Handles deserialisation of <see cref="ModCompatibility"/> arrays.</summary>
    internal class ModCompatibilityArrayConverter : JsonConverter
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
            return objectType == typeof(ModCompatibility[]);
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Read the JSON representation of the object.</summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="existingValue">The object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
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
