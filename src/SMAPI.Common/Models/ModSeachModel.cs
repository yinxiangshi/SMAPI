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
        public ModSearchModel()
        {
            // needed for JSON deserialising
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="modKeys">The namespaced mod keys to search.</param>
        public ModSearchModel(IEnumerable<string> modKeys)
        {
            this.ModKeys = modKeys.ToArray();
        }
    }
}
