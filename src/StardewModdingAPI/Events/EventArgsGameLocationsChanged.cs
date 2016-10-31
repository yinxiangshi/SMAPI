using System;
using System.Collections.Generic;
using StardewValley;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="LocationEvents.LocationsChanged"/> event.</summary>
    public class EventArgsGameLocationsChanged : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The current list of game locations.</summary>
        public List<GameLocation> NewLocations { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="newLocations">The current list of game locations.</param>
        public EventArgsGameLocationsChanged(List<GameLocation> newLocations)
        {
            this.NewLocations = newLocations;
        }
    }
}
