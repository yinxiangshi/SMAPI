using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewModdingAPI
{
    public class Manifest
    {
        /// <summary>
        /// The name of your mod.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The name of the mod's authour.
        /// </summary>
        public virtual string Authour { get; set; }

        /// <summary>
        /// The version of the mod.
        /// </summary>
        public virtual string Version { get; set; }

        /// <summary>
        /// A description of the mod.
        /// </summary>
        public virtual string Description { get; set; }

        public string EntryDll { get; set; }
    }
}
