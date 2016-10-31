using System;
using Microsoft.Xna.Framework;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="LocationEvents.LocationObjectsChanged"/> event.</summary>
    public class EventArgsLocationObjectsChanged : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The current list of objects in the current location.</summary>
        public SerializableDictionary<Vector2, Object> NewObjects { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="newObjects">The current list of objects in the current location.</param>
        public EventArgsLocationObjectsChanged(SerializableDictionary<Vector2, Object> newObjects)
        {
            this.NewObjects = newObjects;
        }
    }
}
