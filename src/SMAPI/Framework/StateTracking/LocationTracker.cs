using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework.StateTracking.FieldWatchers;
using StardewValley;
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

        /// <summary>Tracks changes to the current location's objects.</summary>
        public IDictionaryWatcher<Vector2, Object> LocationObjectsWatcher { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The location to track.</param>
        public LocationTracker(GameLocation location)
        {
            this.Location = location;

            // init watchers
            this.LocationObjectsWatcher = WatcherFactory.ForNetDictionary(location.netObjects);
            this.Watchers.AddRange(new[]
            {
                this.LocationObjectsWatcher
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
