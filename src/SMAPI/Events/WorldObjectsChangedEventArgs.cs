using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="IWorldEvents.ObjectsChanged"/> event.</summary>
    public class WorldObjectsChangedEventArgs : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The location which changed.</summary>
        public GameLocation Location { get; }

        /// <summary>The objects added to the location.</summary>
        public IEnumerable<KeyValuePair<Vector2, Object>> Added { get; }

        /// <summary>The objects removed from the location.</summary>
        public IEnumerable<KeyValuePair<Vector2, Object>> Removed { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The location which changed.</param>
        /// <param name="added">The objects added to the location.</param>
        /// <param name="removed">The objects removed from the location.</param>
        public WorldObjectsChangedEventArgs(GameLocation location, IEnumerable<KeyValuePair<Vector2, Object>> added, IEnumerable<KeyValuePair<Vector2, Object>> removed)
        {
            this.Location = location;
            this.Added = added.ToArray();
            this.Removed = removed.ToArray();
        }
    }
}
