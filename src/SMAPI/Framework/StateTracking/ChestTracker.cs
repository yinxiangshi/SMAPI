using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using ChangeType = StardewModdingAPI.Events.ChangeType;

namespace StardewModdingAPI.Framework.StateTracking
{
    /// <summary>Tracks changes to a chest's items.</summary>
    internal class ChestTracker
    {
        /*********
        ** Fields
        *********/
        /// <summary>The chest's inventory as of the last reset.</summary>
        private IDictionary<Item, int> PreviousInventory;

        /// <summary>The chest's inventory change as of the last update.</summary>
        private IDictionary<Item, int> CurrentInventory;


        /*********
        ** Accessors
        *********/
        /// <summary>The chest being tracked.</summary>
        public Chest Chest { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The chest being tracked.</param>
        public ChestTracker(Chest chest)
        {
            this.Chest = chest;
            this.PreviousInventory = this.GetInventory();
        }

        /// <summary>Update the current values if needed.</summary>
        public void Update()
        {
            this.CurrentInventory = this.GetInventory();
        }

        /// <summary>Reset all trackers so their current values are the baseline.</summary>
        public void Reset()
        {
            if (this.CurrentInventory != null)
                this.PreviousInventory = this.CurrentInventory;
        }

        /// <summary>Get the inventory changes since the last update.</summary>
        public IEnumerable<ItemStackChange> GetInventoryChanges()
        {
            IDictionary<Item, int> previous = this.PreviousInventory;
            IDictionary<Item, int> current = this.GetInventory();

            foreach (Item item in previous.Keys.Union(current.Keys))
            {
                if (!previous.TryGetValue(item, out int prevStack))
                    yield return new ItemStackChange { Item = item, StackChange = item.Stack, ChangeType = ChangeType.Added };
                else if (!current.TryGetValue(item, out int newStack))
                    yield return new ItemStackChange { Item = item, StackChange = -item.Stack, ChangeType = ChangeType.Removed };
                else if (prevStack != newStack)
                    yield return new ItemStackChange { Item = item, StackChange = newStack - prevStack, ChangeType = ChangeType.StackChange };
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the player's current inventory.</summary>
        private IDictionary<Item, int> GetInventory()
        {
            return this.Chest.items
                .Where(n => n != null)
                .Distinct()
                .ToDictionary(n => n, n => n.Stack);
        }
    }
}
