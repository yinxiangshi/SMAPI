using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewModdingAPI.Events
{
    /// <summary>
    /// An attribute for controlling event priority of an event handler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EventPriorityAttribute : System.Attribute
    {
        /// <summary>
        /// The priority for the method marked by this attribute.
        /// </summary>
        public EventPriority Priority { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="priority">The priority for method marked by this attribute.</param>
        public EventPriorityAttribute( EventPriority priority )
        {
            this.Priority = priority;
        }
    }
}
