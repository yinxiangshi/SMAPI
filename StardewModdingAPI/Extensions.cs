using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewModdingAPI
{
    public static class Extensions
    {
        public static Random Random = new Random();

        public static bool IsKeyDown(this Keys key)
        {
            return Keyboard.GetState().IsKeyDown(key);
        }

        public static Color RandomColour()
        {
            return new Color(Random.Next(0, 255), Random.Next(0, 255), Random.Next(0, 255));
        }

        public static string ToSingular(this IEnumerable<Object> enumerable)
        {
            string result = string.Join(", ", enumerable);
            return result;
        }

        public static bool IsInt32(this string s)
        {
            int i;
            return Int32.TryParse(s, out i);
        }

        public static Int32 AsInt32(this string s)
        {
            return Int32.Parse(s);
        }
    }
}