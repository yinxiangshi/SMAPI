using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewModdingAPI
{
    /// <summary>Provides general utility extensions.</summary>
    public static class Extensions
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A pseudo-random number generator.</summary>
        public static Random Random = new Random();


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the given key is currently being pressed.</summary>
        /// <param name="key">The key to check.</param>
        public static bool IsKeyDown(this Keys key)
        {
            return Keyboard.GetState().IsKeyDown(key);
        }

        /// <summary>Get a random color.</summary>
        public static Color RandomColour()
        {
            return new Color(Extensions.Random.Next(0, 255), Extensions.Random.Next(0, 255), Extensions.Random.Next(0, 255));
        }

        /// <summary>Concatenate an enumeration into a delimiter-separated string.</summary>
        /// <param name="ienum">The values to concatenate.</param>
        /// <param name="split">The value separator.</param>
        [Obsolete("The usage of ToSingular has changed. Please update your call to use ToSingular<T>")]
        public static string ToSingular(this IEnumerable ienum, string split = ", ")
        {
            Log.Error("The usage of ToSingular has changed. Please update your call to use ToSingular<T>");
            return "";
        }

        /// <summary>Concatenate an enumeration into a delimiter-separated string.</summary>
        /// <typeparam name="T">The enumerated value type.</typeparam>
        /// <param name="ienum">The values to concatenate.</param>
        /// <param name="split">The value separator.</param>
        public static string ToSingular<T>(this IEnumerable<T> ienum, string split = ", ")
        {
            //Apparently Keys[] won't split normally :l
            if (typeof(T) == typeof(Keys))
            {
                return string.Join(split, ienum.ToArray());
            }
            return string.Join(split, ienum);
        }

        /// <summary>Get whether the value can be parsed as a number.</summary>
        /// <param name="o">The value.</param>
        public static bool IsInt32(this object o)
        {
            int i;
            return int.TryParse(o.ToString(), out i);
        }

        /// <summary>Get the numeric representation of a value.</summary>
        /// <param name="o">The value.</param>
        public static int AsInt32(this object o)
        {
            return int.Parse(o.ToString());
        }

        /// <summary>Get whether the value can be parsed as a boolean.</summary>
        /// <param name="o">The value.</param>
        public static bool IsBool(this object o)
        {
            bool b;
            return bool.TryParse(o.ToString(), out b);
        }

        /// <summary>Get the boolean representation of a value.</summary>
        /// <param name="o">The value.</param>
        public static bool AsBool(this object o)
        {
            return bool.Parse(o.ToString());
        }

        /// <summary>Get a list hash calculated from the hashes of the values it contains.</summary>
        /// <param name="enumerable">The values to hash.</param>
        public static int GetHash(this IEnumerable enumerable)
        {
            var hash = 0;
            foreach (var v in enumerable)
                hash ^= v.GetHashCode();
            return hash;
        }

        /// <summary>Cast a value to the given type. This returns <c>null</c> if the value can't be cast.</summary>
        /// <typeparam name="T">The type to which to cast.</typeparam>
        /// <param name="o">The value.</param>
        public static T Cast<T>(this object o) where T : class
        {
            return o as T;
        }

        /// <summary>Get all private types on an object.</summary>
        /// <param name="o">The object to scan.</param>
        public static FieldInfo[] GetPrivateFields(this object o)
        {
            return o.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
        }

        /// <summary>Get metadata for a private field.</summary>
        /// <param name="t">The type to scan.</param>
        /// <param name="name">The name of the field to find.</param>
        public static FieldInfo GetBaseFieldInfo(this Type t, string name)
        {
            return t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
        }

        /// <summary>Get the value of a private field.</summary>
        /// <param name="t">The type to scan.</param>
        /// <param name="o">The instance for which to get a value.</param>
        /// <param name="name">The name of the field to find.</param>
        public static T GetBaseFieldValue<T>(this Type t, object o, string name) where T : class
        {
            return t.GetBaseFieldInfo(name).GetValue(o) as T;
        }

        /// <summary>Set the value of a private field.</summary>
        /// <param name="t">The type to scan.</param>
        /// <param name="o">The instance for which to set a value.</param>
        /// <param name="name">The name of the field to find.</param>
        /// <param name="newValue">The value to set.</param>
        public static void SetBaseFieldValue<T>(this Type t, object o, string name, object newValue) where T : class
        {
            t.GetBaseFieldInfo(name).SetValue(o, newValue as T);
        }

        /// <summary>Get a copy of the string with only alphanumeric characters. (Numbers are not removed, despite the name.)</summary>
        /// <param name="st">The string to copy.</param>
        public static string RemoveNumerics(this string st)
        {
            var s = st;
            foreach (var c in s)
            {
                if (!char.IsLetterOrDigit(c))
                    s = s.Replace(c.ToString(), "");
            }
            return s;
        }
    }
}