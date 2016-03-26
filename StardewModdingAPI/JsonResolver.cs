using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using StardewModdingAPI.Inheritance;

namespace StardewModdingAPI
{
    class JsonResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (objectType == typeof(Rectangle) || objectType == typeof(Rectangle?))
            {
                Console.WriteLine("FOUND A RECT");
                JsonContract contract = base.CreateObjectContract(objectType);
                contract.Converter = new RectangleConverter();
                return contract;
            }
            if (objectType == typeof(StardewValley.Object))
            {
                Log.Verbose("FOUND AN OBJECT");
                JsonContract contract = base.CreateObjectContract(objectType);
                contract.Converter = new ObjectConverter();
                return contract;

            }
            return base.CreateContract(objectType);
        }
    }

    public class ObjectConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Log.Verbose("TRYING TO WRITE");
            var obj = (StardewValley.Object)value;
            Log.Verbose("TRYING TO WRITE");

            var jObject = GetObject(obj);
            Log.Verbose("TRYING TO WRITE");

            try
            {
                Log.Verbose(jObject.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            Console.ReadKey();

            jObject.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            return GetObject(jObject);
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        protected static JObject GetObject(StardewValley.Object o)
        {
            try
            {
                var parentSheetIndex = o.parentSheetIndex;
                var stack = o.stack;
                var isRecipe = o.isRecipe;
                var price = o.price;
                var quality = o.quality;

                var oo = new SBareObject(parentSheetIndex, stack, isRecipe, price, quality);
                Log.Success(JsonConvert.SerializeObject(oo));
                return JObject.FromObject(oo);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Console.ReadKey();
            }
            return null;
        }

        protected static StardewValley.Object GetObject(JObject jObject)
        {
            int? parentSheetIndex = GetTokenValue<object>(jObject, "parentSheetIndex") as int?;
            int? stack = GetTokenValue<object>(jObject, "parentSheetIndex") as int?;
            bool? isRecipe = GetTokenValue<object>(jObject, "parentSheetIndex") as bool?;
            int? price = GetTokenValue<object>(jObject, "parentSheetIndex") as int?;
            int? quality = GetTokenValue<object>(jObject, "parentSheetIndex") as int?;

            return new StardewValley.Object(parentSheetIndex ?? 0, stack ?? 0, isRecipe ?? false, price ?? -1, quality ?? 0);
        }

        protected static StardewValley.Object GetObject(JToken jToken)
        {
            var jObject = JObject.FromObject(jToken);

            return GetObject(jObject);
        }

        protected static T GetTokenValue<T>(JObject jObject, string tokenName) where T : class
        {
            JToken jToken;
            jObject.TryGetValue(tokenName, StringComparison.InvariantCultureIgnoreCase, out jToken);
            return jToken as T;
        }
    }

    public class RectangleConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var rectangle = (Rectangle)value;

            var jObject = GetObject(rectangle);

            jObject.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Console.WriteLine(reader.ReadAsString());
            var jObject = JObject.Load(reader);

            return GetRectangle(jObject);
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        protected static JObject GetObject(Rectangle rectangle)
        {
            var x = rectangle.X;
            var y = rectangle.Y;
            var width = rectangle.Width;
            var height = rectangle.Height;

            return JObject.FromObject(new { x, y, width, height });
        }

        protected static Rectangle GetRectangle(JObject jObject)
        {
            var x = GetTokenValue(jObject, "x") ?? 0;
            var y = GetTokenValue(jObject, "y") ?? 0;
            var width = GetTokenValue(jObject, "width") ?? 0;
            var height = GetTokenValue(jObject, "height") ?? 0;

            return new Rectangle(x, y, width, height);
        }

        protected static Rectangle GetRectangle(JToken jToken)
        {
            var jObject = JObject.FromObject(jToken);

            return GetRectangle(jObject);
        }

        protected static int? GetTokenValue(JObject jObject, string tokenName)
        {
            JToken jToken;
            return jObject.TryGetValue(tokenName, StringComparison.InvariantCultureIgnoreCase, out jToken) ? (int)jToken : (int?)null;
        }
    }

    public class RectangleListConverter : RectangleConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var rectangleList = (IList<Rectangle>)value;

            var jArray = new JArray();

            foreach (var rectangle in rectangleList)
            {
                jArray.Add(GetObject(rectangle));
            }

            jArray.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var rectangleList = new List<Rectangle>();

            var jArray = JArray.Load(reader);

            foreach (var jToken in jArray)
            {
                rectangleList.Add(GetRectangle(jToken));
            }

            return rectangleList;
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}
