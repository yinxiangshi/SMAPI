using System.Collections.Generic;
using System.Linq;

namespace StardewModdingAPI.Internal.Models
{
    /// <summary>Specifies mods whose update-check info to fetch.</summary>
    internal class ModSearchModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The namespaced mod keys to search.</summary>
        public string[] ModKeys { get; set; }

        /// <summary>Whether to allow non-semantic versions, instead of returning an error for those.</summary>
        public bool AllowInvalidVersions { get; set; }


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
        /// <param name="allowInvalidVersions">Whether to allow non-semantic versions, instead of returning an error for those.</param>
        public ModSearchModel(IEnumerable<string> modKeys, bool allowInvalidVersions)
        {
            this.ModKeys = modKeys.ToArray();
            this.AllowInvalidVersions = allowInvalidVersions;
        }
    }
}
