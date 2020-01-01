using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework.StateTracking.Comparers;
using StardewModdingAPI.Framework.StateTracking.FieldWatchers;
using StardewValley;
using StardewValley.Objects;
using ChangeType = StardewModdingAPI.Events.ChangeType;

namespace StardewModdingAPI.Framework.StateTracking
{
    /// <summary>Tracks changes to a chest's items.</summary>
    internal class ChestTracker : IDisposable
    {
        /*********
        ** Fields
        *********/
        /// <summary>The item stack sizes as of the last update.</summary>
        private readonly IDictionary<Item, int> StackSizes;

        /// <summary>Items added since the last update.</summary>
        private readonly HashSet<Item> Added = new HashSet<Item>(new ObjectReferenceComparer<Item>());

        /// <summary>Items removed since the last update.</summary>
        private readonly HashSet<Item> Removed = new HashSet<Item>(new ObjectReferenceComparer<Item>());

        /// <summary>The underlying inventory watcher.</summary>
        private readonly ICollectionWatcher<Item> InventoryWatcher;


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
            this.InventoryWatcher = WatcherFactory.ForNetList(chest.items);

            this.StackSizes = this.Chest.items
                .Where(n => n != null)
                .Distinct()
                .ToDictionary(n => n, n => n.Stack);
        }

        /// <summary>Update the current values if needed.</summary>
        public void Update()
        {
            // update watcher
            this.InventoryWatcher.Update();
            foreach (Item item in this.InventoryWatcher.Added.Where(p => p != null))
                this.Added.Add(item);
            foreach (Item item in this.InventoryWatcher.Removed.Where(p => p != null))
            {
                if (!this.Added.Remove(item)) // item didn't change if it was both added and removed, so remove it from both lists
                    this.Removed.Add(item);
            }

            // stop tracking removed stacks
            foreach (Item item in this.Removed)
                this.StackSizes.Remove(item);
        }

        /// <summary>Reset all trackers so their current values are the baseline.</summary>
        public void Reset()
        {
            // update stack sizes
            foreach (Item item in this.StackSizes.Keys.ToArray().Concat(this.Added))
                this.StackSizes[item] = item.Stack;

            // update watcher
            this.InventoryWatcher.Reset();
            this.Added.Clear();
            this.Removed.Clear();
        }

        /// <summary>Get the inventory changes since the last update.</summary>
        public IEnumerable<ItemStackChange> GetInventoryChanges()
        {
            // removed
            foreach (Item item in this.Removed)
                yield return new ItemStackChange { Item = item, StackChange = -item.Stack, ChangeType = ChangeType.Removed };

            // added
            foreach (Item item in this.Added)
                yield return new ItemStackChange { Item = item, StackChange = item.Stack, ChangeType = ChangeType.Added };

            // stack size changed
            foreach (var entry in this.StackSizes)
            {
                Item item = entry.Key;
                int prevStack = entry.Value;

                if (item.Stack != prevStack)
                    yield return new ItemStackChange { Item = item, StackChange = item.Stack - prevStack, ChangeType = ChangeType.StackChange };
            }
        }

        /// <summary>Release watchers and resources.</summary>
        public void Dispose()
        {
            this.StackSizes.Clear();
            this.Added.Clear();
            this.Removed.Clear();
            this.InventoryWatcher.Dispose();
        }
    }
}
