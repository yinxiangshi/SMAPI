using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public static string ToSingular<T>(this IEnumerable<T> ienum, string split = ", ") // where T : class
        {
            //Apparently Keys[] won't split normally :l
            if (typeof(T) == typeof(Keys))
            {
                return string.Join(split, ienum.ToArray());
            }
            return string.Join(split, ienum);
        }

        public static bool IsInt32(this object o)
        {
            int i;
            return int.TryParse(o.ToString(), out i);
        }

        public static int AsInt32(this object o)
        {
            return int.Parse(o.ToString());
        }

        public static int GetHash(this IEnumerable enumerable)
        {
            var hash = 0;
            foreach (var v in enumerable)
            {
                hash ^= v.GetHashCode();
            }
            return hash;
        }

        public static FieldInfo GetBaseFieldInfo(this Type t, string name)
        {
            return t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static T GetBaseFieldValue<T>(this Type t, object o, string name) where T : class
        {
            return t.GetBaseFieldInfo(name).GetValue(o) as T;
        }

        public static void SetBaseFieldValue<T>(this Type t, object o, string name, object newValue) where T : class
        {
            t.GetBaseFieldInfo(name).SetValue(o, newValue as T);
        }

        public static string RemoveNumerics(this string st)
        {
            var s = st;
            foreach (var c in s)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    s = s.Replace(c.ToString(), "");
                }
            }
            return s;
        }
    }
}