using System;
using StardewModdingAPI.Events;

namespace StardewModdingAPI.Framework.Events
{
    /// <summary>Events raised when something changes in the world.</summary>
    public class ModWorldEvents : IWorldEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying event manager.</summary>
        private readonly EventManager EventManager;

        /// <summary>The mod which uses this instance.</summary>
        private readonly IModMetadata Mod;


        /*********
        ** Accessors
        *********/
        /// <summary>Raised after a game location is added or removed.</summary>
        public event EventHandler<WorldLocationListChangedEventArgs> LocationListChanged
        {
            add => this.EventManager.World_LocationListChanged.Add(value, this.Mod);
            remove => this.EventManager.World_LocationListChanged.Remove(value);
        }

        /// <summary>Raised after buildings are added or removed in a location.</summary>
        public event EventHandler<WorldBuildingListChangedEventArgs> BuildingListChanged
        {
            add => this.EventManager.World_BuildingListChanged.Add(value, this.Mod);
            remove => this.EventManager.World_BuildingListChanged.Remove(value);
        }

        /// <summary>Raised after objects are added or removed in a location.</summary>
        public event EventHandler<WorldObjectListChangedEventArgs> ObjectListChanged
        {
            add => this.EventManager.World_ObjectListChanged.Add(value);
            remove => this.EventManager.World_ObjectListChanged.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The mod which uses this instance.</param>
        /// <param name="eventManager">The underlying event manager.</param>
        internal ModWorldEvents(IModMetadata mod, EventManager eventManager)
        {
            this.Mod = mod;
            this.EventManager = eventManager;
        }
    }
}
