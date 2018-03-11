using System;
using Microsoft.Xna.Framework;
#if STARDEW_VALLEY_1_3
using Netcode;
#else
using StardewValley;
#endif
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
#if STARDEW_VALLEY_1_3
        public IDictionary<Vector2, NetRef<Object>> NewObjects { get; }
#else
        public SerializableDictionary<Vector2, Object> NewObjects { get; }
#endif


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="newObjects">The current list of objects in the current location.</param>
        public EventArgsLocationObjectsChanged(
#if STARDEW_VALLEY_1_3
            IDictionary<Vector2, NetRef<Object>> newObjects
#else
            SerializableDictionary<Vector2, Object> newObjects
#endif
    )
        {
            this.NewObjects = newObjects;
        }
    }
}
