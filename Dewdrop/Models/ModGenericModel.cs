using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dewdrop.Models
{
    public class ModGenericModel
    {
        /// <summary>
        /// An identifier for the mod.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The mod's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The vendor identifier for the mod.
        /// </summary>
        public string Vendor { get; set; }

        /// <summary>
        /// The mod's version number.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The mod's URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Is the mod a valid mod.
        /// </summary>
        public bool Valid { get; set; } = true;
    }
}
