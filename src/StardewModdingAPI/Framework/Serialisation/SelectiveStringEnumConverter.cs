using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Converters;

namespace StardewModdingAPI.Framework.Serialisation
{
    /// <summary>A variant of <see cref="StringEnumConverter"/> which only converts certain enums.</summary>
    internal class SelectiveStringEnumConverter : StringEnumConverter
    {
        /*********
        ** Properties
        *********/
        /// <summary>The enum type names to convert.</summary>
        private readonly HashSet<string> Types;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="types">The enum types to convert.</param>
        public SelectiveStringEnumConverter(params Type[] types)
        {
            this.Types = new HashSet<string>(types.Select(p => p.FullName));
        }

        /// <summary>Get whether this instance can convert the specified object type.</summary>
        /// <param name="type">The object type.</param>
        public override bool CanConvert(Type type)
        {
            return base.CanConvert(type) && this.Types.Contains(type.FullName);
        }
    }
}
