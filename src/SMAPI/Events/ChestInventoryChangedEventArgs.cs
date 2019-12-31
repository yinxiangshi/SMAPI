using System;
using StardewValley;
using StardewValley.Objects;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="IWorldEvents.ChestInventoryChanged"/> event.</summary>
    public class ChestInventoryChangedEventArgs : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The chest whose inventory changed.</summary>
        public Chest Chest { get; }

        /// <summary>The location containing the chest.</summary>
        public GameLocation Location { get; }

        /// <summary>The inventory changes in the chest.</summary>
        public ItemStackChange[] Changes { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The chest whose inventory changed.</param>
        /// <param name="location">The location containing the chest.</param>
        /// <param name="changes">The inventory changes in the chest.</param>
        internal ChestInventoryChangedEventArgs(Chest chest, GameLocation location, ItemStackChange[] changes)
        {
            this.Location = location;
            this.Chest = chest;
            this.Changes = changes;
        }
    }
}
