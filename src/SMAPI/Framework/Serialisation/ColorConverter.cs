using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI.Framework.Exceptions;

namespace StardewModdingAPI.Framework.Serialisation
{
    /// <summary>Handles deserialisation of <see cref="Color"/> for crossplatform compatibility.</summary>
    internal class ColorConverter : JsonConverter
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
            return objectType == typeof(Color);
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="existingValue">The object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //    Linux/Mac: { "B": 76, "G": 51, "R": 25, "A": 102 }
            //    Windows:   "26, 51, 76, 102"
            JToken token = JToken.Load(reader);
            switch (token.Type)
            {
                case JTokenType.Object:
                    {
                        JObject obj = (JObject)token;
                        int r = obj.Value<int>(nameof(Color.R));
                        int g = obj.Value<int>(nameof(Color.G));
                        int b = obj.Value<int>(nameof(Color.B));
                        int a = obj.Value<int>(nameof(Color.A));
                        return new Color(r, g, b, a);
                    }

                case JTokenType.String:
                    {
                        string str = token.Value<string>();
                        if (string.IsNullOrWhiteSpace(str))
                            return null;

                        string[] parts = str.Split(',');
                        if (parts.Length != 4)
                            throw new SParseException($"Can't parse {typeof(Color).Name} from {token.Path}, invalid value '{str}'.");

                        int r = Convert.ToInt32(parts[0]);
                        int g = Convert.ToInt32(parts[1]);
                        int b = Convert.ToInt32(parts[2]);
                        int a = Convert.ToInt32(parts[3]);
                        return new Color(r, g, b, a);
                    }

                default:
                    throw new SParseException($"Can't parse {typeof(Point).Name} from {token.Path}, must be an object or string.");
            }
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
