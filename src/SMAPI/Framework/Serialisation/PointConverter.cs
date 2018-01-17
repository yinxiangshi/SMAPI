using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI.Framework.Exceptions;

namespace StardewModdingAPI.Framework.Serialisation
{
    /// <summary>Handles deserialisation of <see cref="PointConverter"/> for crossplatform compatibility.</summary>
    internal class PointConverter : JsonConverter
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
            return objectType == typeof(Point);
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="existingValue">The object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // point
            //    Linux/Mac: { "X": 1, "Y": 2 }
            //    Windows:   "1, 2"
            JToken token = JToken.Load(reader);
            switch (token.Type)
            {
                case JTokenType.Object:
                    {
                        JObject obj = (JObject)token;
                        int x = obj.Value<int>(nameof(Point.X));
                        int y = obj.Value<int>(nameof(Point.Y));
                        return new Point(x, y);
                    }

                case JTokenType.String:
                    {
                        string str = token.Value<string>();
                        if (string.IsNullOrWhiteSpace(str))
                            return null;

                        string[] parts = str.Split(',');
                        if (parts.Length != 2)
                            throw new SParseException($"Can't parse {typeof(Point).Name} from {token.Path}, invalid value '{str}'.");

                        int x = Convert.ToInt32(parts[0]);
                        int y = Convert.ToInt32(parts[1]);
                        return new Point(x, y);
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
