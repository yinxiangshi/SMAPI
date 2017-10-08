using System.Collections.Generic;
using System.Linq;

namespace StardewModdingAPI.Common.Models
{
    /// <summary>Specifies mods whose update-check info to fetch.</summary>
    internal class ModSearchModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The namespaced mod keys to search.</summary>
        public string[] ModKeys { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an empty instance.</summary>
        /// <remarks>This constructed is needed for JSON deserialisation.</remarks>
        public ModSearchModel() { }

        /// <summary>Construct an valid instance.</summary>
        /// <param name="modKeys">The namespaced mod keys to search.</param>
        public ModSearchModel(IEnumerable<string> modKeys)
        {
            this.ModKeys = modKeys.ToArray();
        }
    }
}
