using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the player transitions between game locations, a location is added or removed, or the objects in the current location change.</summary>
    public static class LocationEvents
    {
        /*********
        ** Events
        *********/
        /// <summary>Raised after the player warps to a new location.</summary>
        public static event EventHandler<EventArgsCurrentLocationChanged> CurrentLocationChanged;

        /// <summary>Raised after a game location is added or removed.</summary>
        public static event EventHandler<EventArgsGameLocationsChanged> LocationsChanged;

        /// <summary>Raised after the list of objects in the current location changes (e.g. an object is added or removed).</summary>
        public static event EventHandler<EventArgsLocationObjectsChanged> LocationObjectsChanged;


        /*********
        ** Internal methods
        *********/
        /// <summary>Raise a <see cref="CurrentLocationChanged"/> event.</summary>
        /// <param name="priorLocation">The player's previous location.</param>
        /// <param name="newLocation">The player's current location.</param>
        internal static void InvokeCurrentLocationChanged(GameLocation priorLocation, GameLocation newLocation)
        {
            LocationEvents.CurrentLocationChanged?.Invoke(null, new EventArgsCurrentLocationChanged(priorLocation, newLocation));
        }

        /// <summary>Raise a <see cref="LocationsChanged"/> event.</summary>
        /// <param name="newLocations">The current list of game locations.</param>
        internal static void InvokeLocationsChanged(List<GameLocation> newLocations)
        {
            LocationEvents.LocationsChanged?.Invoke(null, new EventArgsGameLocationsChanged(newLocations));
        }

        /// <summary>Raise a <see cref="LocationObjectsChanged"/> event.</summary>
        /// <param name="newObjects">The current list of objects in the current location.</param>
        internal static void InvokeOnNewLocationObject(SerializableDictionary<Vector2, Object> newObjects)
        {
            LocationEvents.LocationObjectsChanged?.Invoke(null, new EventArgsLocationObjectsChanged(newObjects));
        }
    }
}
