using System;
using StardewModdingAPI.Events;

namespace StardewModdingAPI.Framework.Events
{
    /// <summary>Events raised when something changes in the world.</summary>
    internal class ModWorldEvents : ModEventsBase, IWorldEvents
    {
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

        /// <summary>Raised after debris are added or removed in a location.</summary>
        public event EventHandler<WorldDebrisListChangedEventArgs> DebrisListChanged
        {
            add => this.EventManager.World_DebrisListChanged.Add(value, this.Mod);
            remove => this.EventManager.World_DebrisListChanged.Remove(value);
        }

        /// <summary>Raised after large terrain features (like bushes) are added or removed in a location.</summary>
        public event EventHandler<WorldLargeTerrainFeatureListChangedEventArgs> LargeTerrainFeatureListChanged
        {
            add => this.EventManager.World_LargeTerrainFeatureListChanged.Add(value, this.Mod);
            remove => this.EventManager.World_LargeTerrainFeatureListChanged.Remove(value);
        }

        /// <summary>Raised after NPCs are added or removed in a location.</summary>
        public event EventHandler<WorldNpcListChangedEventArgs> NpcListChanged
        {
            add => this.EventManager.World_NpcListChanged.Add(value);
            remove => this.EventManager.World_NpcListChanged.Remove(value);
        }

        /// <summary>Raised after objects are added or removed in a location.</summary>
        public event EventHandler<WorldObjectListChangedEventArgs> ObjectListChanged
        {
            add => this.EventManager.World_ObjectListChanged.Add(value);
            remove => this.EventManager.World_ObjectListChanged.Remove(value);
        }

        /// <summary>Raised after terrain features (like floors and trees) are added or removed in a location.</summary>
        public event EventHandler<WorldTerrainFeatureListChangedEventArgs> TerrainFeatureListChanged
        {
            add => this.EventManager.World_TerrainFeatureListChanged.Add(value);
            remove => this.EventManager.World_TerrainFeatureListChanged.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The mod which uses this instance.</param>
        /// <param name="eventManager">The underlying event manager.</param>
        internal ModWorldEvents(IModMetadata mod, EventManager eventManager)
            : base(mod, eventManager) { }
    }
}
