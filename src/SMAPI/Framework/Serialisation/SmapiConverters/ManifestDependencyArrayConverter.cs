using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI.Framework.Models;

namespace StardewModdingAPI.Framework.Serialisation.SmapiConverters
{
    /// <summary>Handles deserialisation of <see cref="IManifestDependency"/> arrays.</summary>
    internal class ManifestDependencyArrayConverter : JsonConverter
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
            return objectType == typeof(IManifestDependency[]);
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
            List<IManifestDependency> result = new List<IManifestDependency>();
            foreach (JObject obj in JArray.Load(reader).Children<JObject>())
            {
                string uniqueID = obj.ValueIgnoreCase<string>(nameof(IManifestDependency.UniqueID));
                string minVersion = obj.ValueIgnoreCase<string>(nameof(IManifestDependency.MinimumVersion));
                bool required = obj.ValueIgnoreCase<bool?>(nameof(IManifestDependency.IsRequired)) ?? true;
                result.Add(new ManifestDependency(uniqueID, minVersion, required));
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
