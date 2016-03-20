using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewModdingAPI
{
    public class Mod
    {
        /// <summary>
        /// The mod's manifest
        /// </summary>
        public Manifest Manifest { get; internal set; }

        /// <summary>
        /// Where the mod is located on the disk.
        /// </summary>
        public string PathOnDisk { get; internal set; }

        /// <summary>
        /// A basic method that is the entry-point of your mod. It will always be called once when the mod loads.
        /// </summary>
        public virtual void Entry(params object[] objects)
        {

        }
    }
}
