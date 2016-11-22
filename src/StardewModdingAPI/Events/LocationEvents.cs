using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework;
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
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="priorLocation">The player's previous location.</param>
        /// <param name="newLocation">The player's current location.</param>
        internal static void InvokeCurrentLocationChanged(IMonitor monitor, GameLocation priorLocation, GameLocation newLocation)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(LocationEvents)}.{nameof(LocationEvents.CurrentLocationChanged)}", LocationEvents.CurrentLocationChanged?.GetInvocationList(), null, new EventArgsCurrentLocationChanged(priorLocation, newLocation));
        }

        /// <summary>Raise a <see cref="LocationsChanged"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="newLocations">The current list of game locations.</param>
        internal static void InvokeLocationsChanged(IMonitor monitor, List<GameLocation> newLocations)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(LocationEvents)}.{nameof(LocationEvents.LocationsChanged)}", LocationEvents.LocationsChanged?.GetInvocationList(), null, new EventArgsGameLocationsChanged(newLocations));
        }

        /// <summary>Raise a <see cref="LocationObjectsChanged"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="newObjects">The current list of objects in the current location.</param>
        internal static void InvokeOnNewLocationObject(IMonitor monitor, SerializableDictionary<Vector2, Object> newObjects)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(LocationEvents)}.{nameof(LocationEvents.LocationObjectsChanged)}", LocationEvents.LocationObjectsChanged?.GetInvocationList(), null, new EventArgsLocationObjectsChanged(newObjects));
        }
    }
}
