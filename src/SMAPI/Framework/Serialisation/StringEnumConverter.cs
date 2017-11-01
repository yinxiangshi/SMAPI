using System;
using Newtonsoft.Json.Converters;

namespace StardewModdingAPI.Framework.Serialisation
{
    /// <summary>A variant of <see cref="StringEnumConverter"/> which only converts a specified enum.</summary>
    /// <typeparam name="T">The enum type.</typeparam>
    internal class StringEnumConverter<T> : StringEnumConverter
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get whether this instance can convert the specified object type.</summary>
        /// <param name="type">The object type.</param>
        public override bool CanConvert(Type type)
        {
            return
                base.CanConvert(type)
                && (Nullable.GetUnderlyingType(type) ?? type) == typeof(T);
        }
    }
}
