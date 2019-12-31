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
        public StardewValley.Objects.Chest Chest { get; }

        /// <summary>The inventory changes added to the chest.</summary>
        public ItemStackChange[] Changes { get; }

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The location containing the chest.</param>
        /// <param name="tile">The tile position of the chest.</param>
        /// <param name="added">The objects added to the location.</param>
        /// <param name="removed">The objects removed from the location.</param>
        internal ChestInventoryChangedEventArgs(GameLocation location, StardewValley.Objects.Chest chest, ItemStackChange[] changes)
        {
            this.Location = location;
            this.Chest = chest;
            this.Changes = changes;
        }
    }
}
