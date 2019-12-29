using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="IWorldEvents.ChestInventoryChanged"/> event.</summary>
    public class ChestInventoryChangedEventArgs : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The location containing the chest.</summary>
        public GameLocation Location { get; }

        /// <summary>The tile position of the chest.</summary>
        public Vector2 Tile { get; }

        /// <summary>The objects added to the location.</summary>
        public IEnumerable<Item> Added { get; }

        /// <summary>The objects removed from the location.</summary>
        public IEnumerable<Item> Removed { get; }

        /// <summary>Whether this is the location containing the local player.</summary>
        public bool IsCurrentLocation => object.ReferenceEquals(this.Location, Game1.player?.currentLocation);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The location containing the chest.</param>
        /// <param name="tile">The tile position of the chest.</param>
        /// <param name="added">The objects added to the location.</param>
        /// <param name="removed">The objects removed from the location.</param>
        internal ChestInventoryChangedEventArgs(GameLocation location, Vector2 tile, IEnumerable<Item> added, IEnumerable<Item> removed)
        {
            this.Location = location;
            this.Tile = tile;
            this.Added = added.ToArray();
            this.Removed = removed.ToArray();
        }
    }
}
