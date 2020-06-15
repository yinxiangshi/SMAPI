using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewModdingAPI.Events
{
    /// <summary>
    /// Event priority for method handlers.
    /// </summary>
    public enum EventPriority
    {
        /// <summary>
        /// Low priority.
        /// </summary>
        Low = 3,

        /// <summary>
        /// Normal priority. This is the default.
        /// </summary>
        Normal = 2,

        /// <summary>
        /// High priority.
        /// </summary>
        High = 1,
    }
}
