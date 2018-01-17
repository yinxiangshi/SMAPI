using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI.Framework.Exceptions;

namespace StardewModdingAPI.Framework.Serialisation
{
    /// <summary>Handles deserialisation of <see cref="Rectangle"/> for crossplatform compatibility.</summary>
    internal class RectangleConverter : JsonConverter
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
            return objectType == typeof(Rectangle);
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="existingValue">The object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //    Linux/Mac: { "X": 1, "Y": 2, "Width": 3, "Height": 4 }
            //    Windows:   "{X:1 Y:2 Width:3 Height:4}"
            JToken token = JToken.Load(reader);
            switch (token.Type)
            {
                case JTokenType.Object:
                    {
                        JObject obj = (JObject)token;
                        int x = obj.Value<int>(nameof(Rectangle.X));
                        int y = obj.Value<int>(nameof(Rectangle.Y));
                        int width = obj.Value<int>(nameof(Rectangle.Width));
                        int height = obj.Value<int>(nameof(Rectangle.Height));
                        return new Rectangle(x, y, width, height);
                    }

                case JTokenType.String:
                    {
                        string str = token.Value<string>();
                        if (string.IsNullOrWhiteSpace(str))
                            return Rectangle.Empty;

                        var match = Regex.Match(str, @"^\{X:(?<x>\d+) Y:(?<y>\d+) Width:(?<width>\d+) Height:(?<height>\d+)\}$");
                        if (!match.Success)
                            throw new SParseException($"Can't parse {typeof(Rectangle).Name} from {reader.Path}, invalid string format.");

                        int x = Convert.ToInt32(match.Groups["x"].Value);
                        int y = Convert.ToInt32(match.Groups["y"].Value);
                        int width = Convert.ToInt32(match.Groups["width"].Value);
                        int height = Convert.ToInt32(match.Groups["height"].Value);

                        return new Rectangle(x, y, width, height);
                    }

                default:
                    throw new SParseException($"Can't parse {typeof(Rectangle).Name} from {reader.Path}, must be an object or string.");
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
