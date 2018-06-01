using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Provides events raised when something changes in the world.</summary>
    public interface IWorldEvents
    {
        /*********
        ** Events
        *********/
        /// <summary>Raised after a game location is added or removed.</summary>
        event EventHandler<WorldLocationsChangedEventArgs> LocationsChanged;

        /// <summary>Raised after buildings are added or removed in a location.</summary>
        event EventHandler<WorldBuildingsChangedEventArgs> BuildingsChanged;

        /// <summary>Raised after objects are added or removed in a location.</summary>
        event EventHandler<WorldObjectsChangedEventArgs> ObjectsChanged;
    }
}
