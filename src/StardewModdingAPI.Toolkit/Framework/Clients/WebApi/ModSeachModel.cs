using System;
using System.Linq;

namespace StardewModdingAPI.Toolkit.Framework.Clients.WebApi
{
    /// <summary>Specifies mods whose update-check info to fetch.</summary>
    public class ModSearchModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The namespaced mod keys to search.</summary>
        [Obsolete]
        public string[] ModKeys { get; set; }

        /// <summary>The mods for which to find data.</summary>
        public ModSearchEntryModel[] Mods { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an empty instance.</summary>
        public ModSearchModel()
        {
            // needed for JSON deserialising
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="mods">The mods to search.</param>
        public ModSearchModel(ModSearchEntryModel[] mods)
        {
            this.Mods = mods.ToArray();
        }
    }
}
