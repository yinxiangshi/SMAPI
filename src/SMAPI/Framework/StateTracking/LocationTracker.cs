using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework.StateTracking.FieldWatchers;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using Object = StardewValley.Object;

namespace StardewModdingAPI.Framework.StateTracking
{
    /// <summary>Tracks changes to a location's data.</summary>
    internal class LocationTracker : IWatcher
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying watchers.</summary>
        private readonly List<IWatcher> Watchers = new List<IWatcher>();


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the value changed since the last reset.</summary>
        public bool IsChanged => this.Watchers.Any(p => p.IsChanged);

        /// <summary>The tracked location.</summary>
        public GameLocation Location { get; }

        /// <summary>Tracks changes to the location's buildings.</summary>
        public ICollectionWatcher<Building> BuildingsWatcher { get; }

        /// <summary>Tracks changes to the location's objects.</summary>
        public IDictionaryWatcher<Vector2, Object> ObjectsWatcher { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The location to track.</param>
        public LocationTracker(GameLocation location)
        {
            this.Location = location;

            // init watchers
            this.ObjectsWatcher = WatcherFactory.ForNetDictionary(location.netObjects);
            this.BuildingsWatcher = location is BuildableGameLocation buildableLocation
                ? WatcherFactory.ForNetCollection(buildableLocation.buildings)
                : (ICollectionWatcher<Building>)WatcherFactory.ForObservableCollection(new ObservableCollection<Building>());

            this.Watchers.AddRange(new IWatcher[]
            {
                this.BuildingsWatcher,
                this.ObjectsWatcher
            });
        }

        /// <summary>Stop watching the player fields and release all references.</summary>
        public void Dispose()
        {
            foreach (IWatcher watcher in this.Watchers)
                watcher.Dispose();
        }

        /// <summary>Update the current value if needed.</summary>
        public void Update()
        {
            foreach (IWatcher watcher in this.Watchers)
                watcher.Update();
        }

        /// <summary>Set the current value as the baseline.</summary>
        public void Reset()
        {
            foreach (IWatcher watcher in this.Watchers)
                watcher.Reset();
        }
    }
}
