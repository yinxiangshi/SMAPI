using System;
using System.Collections;
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

        public static string ToSingular(this IEnumerable<Object> enumerable, string split = ", ")
        {
            string result = string.Join(split, enumerable);
            return result;
        }

        public static bool IsInt32(this object o)
        {
            int i;
            return Int32.TryParse(o.ToString(), out i);
        }

        public static Int32 AsInt32(this object o)
        {
            return Int32.Parse(o.ToString());
        }

        public static bool IsBool(this object o)
        {
            bool b;
            return Boolean.TryParse(o.ToString(), out b);
        }

        public static bool AsBool(this object o)
        {
            return Boolean.Parse(o.ToString());
        }

        public static int GetHash(this IEnumerable enumerable)
        {
            string s = string.Empty;
            foreach (var v in enumerable)
            {
                s += v.GetHashCode();
            }
            return s.GetHashCode();
        } 
    }
}