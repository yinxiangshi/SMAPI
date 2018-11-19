using System;
using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the player transitions between game locations, a location is added or removed, or the objects in the current location change.</summary>
    public static class LocationEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>The core event manager.</summary>
        private static EventManager EventManager;


        /*********
        ** Events
        *********/
        /// <summary>Raised after a game location is added or removed.</summary>
        public static event EventHandler<EventArgsLocationsChanged> LocationsChanged
        {
            add => LocationEvents.EventManager.Legacy_LocationsChanged.Add(value);
            remove => LocationEvents.EventManager.Legacy_LocationsChanged.Remove(value);
        }

        /// <summary>Raised after buildings are added or removed in a location.</summary>
        public static event EventHandler<EventArgsLocationBuildingsChanged> BuildingsChanged
        {
            add => LocationEvents.EventManager.Legacy_BuildingsChanged.Add(value);
            remove => LocationEvents.EventManager.Legacy_BuildingsChanged.Remove(value);
        }

        /// <summary>Raised after objects are added or removed in a location.</summary>
        public static event EventHandler<EventArgsLocationObjectsChanged> ObjectsChanged
        {
            add => LocationEvents.EventManager.Legacy_ObjectsChanged.Add(value);
            remove => LocationEvents.EventManager.Legacy_ObjectsChanged.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the events.</summary>
        /// <param name="eventManager">The core event manager.</param>
        internal static void Init(EventManager eventManager)
        {
            LocationEvents.EventManager = eventManager;
        }
    }
}
