using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using Item = StardewValley.Item;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="IWorldEvents.ChestItemChanged"/> event.</summary>
    public class ChestItemChangedEventArgs : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The location which changed.</summary>
        public GameLocation Location { get; }

        /// <summary>The objects added to the location.</summary>
        public IEnumerable<Item> Added { get; }

        /// <summary>The objects removed from the location.</summary>
        public IEnumerable<Item> Removed { get; }

        /// <summary>The location of the chest from where the item was added or removed</summary>
        public Vector2 LocationOfChest { get; }

        /// <summary>Whether this is the location containing the local player.</summary>
        public bool IsCurrentLocation => object.ReferenceEquals(this.Location, Game1.player?.currentLocation);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The location which changed.</param>
        /// <param name="added">The objects added to the location.</param>
        /// <param name="removed">The objects removed from the location.</param>
        internal ChestItemChangedEventArgs(GameLocation location, IEnumerable<Item> added, IEnumerable<Item> removed, Vector2 locationOfChest)
        {
            this.Location = location;
            this.Added = added.ToArray();
            this.Removed = removed.ToArray();
            this.LocationOfChest = locationOfChest;
        }
    }
}
