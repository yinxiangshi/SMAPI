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
        /// <summary>Raised after the player warps to a new location.</summary>
        public static event EventHandler<EventArgsCurrentLocationChanged> CurrentLocationChanged
        {
            add => LocationEvents.EventManager.Location_CurrentLocationChanged.Add(value);
            remove => LocationEvents.EventManager.Location_CurrentLocationChanged.Remove(value);
        }

        /// <summary>Raised after a game location is added or removed.</summary>
        public static event EventHandler<EventArgsGameLocationsChanged> LocationsChanged
        {
            add => LocationEvents.EventManager.Location_LocationsChanged.Add(value);
            remove => LocationEvents.EventManager.Location_LocationsChanged.Remove(value);
        }

        /// <summary>Raised after the list of objects in the current location changes (e.g. an object is added or removed).</summary>
        public static event EventHandler<EventArgsLocationObjectsChanged> LocationObjectsChanged
        {
            add => LocationEvents.EventManager.Location_LocationObjectsChanged.Add(value);
            remove => LocationEvents.EventManager.Location_LocationObjectsChanged.Remove(value);
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
