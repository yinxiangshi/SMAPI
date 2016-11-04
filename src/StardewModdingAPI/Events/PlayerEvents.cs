using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Inheritance;
using StardewValley;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the player data changes.</summary>
    public static class PlayerEvents
    {
        /*********
        ** Events
        *********/
        /// <summary>Raised after the player loads a saved game.</summary>
        public static event EventHandler<EventArgsLoadedGameChanged> LoadedGame = delegate { };

        /// <summary>Raised after the game assigns a new player character. This happens just before <see cref="LoadedGame"/>; it's unclear how this would happen any other time.</summary>
        public static event EventHandler<EventArgsFarmerChanged> FarmerChanged = delegate { };

        /// <summary>Raised after the player's inventory changes in any way (added or removed item, sorted, etc).</summary>
        public static event EventHandler<EventArgsInventoryChanged> InventoryChanged = delegate { };

        /// <summary> Raised after the player levels up a skill. This happens as soon as they level up, not when the game notifies the player after their character goes to bed.</summary>
        public static event EventHandler<EventArgsLevelUp> LeveledUp = delegate { };


        /*********
        ** Internal methods
        *********/
        /// <summary>Raise a <see cref="LoadedGame"/> event.</summary>
        /// <param name="loaded">Whether the save has been loaded. This is always true.</param>
        internal static void InvokeLoadedGame(EventArgsLoadedGameChanged loaded)
        {
            PlayerEvents.LoadedGame.Invoke(null, loaded);
        }

        /// <summary>Raise a <see cref="FarmerChanged"/> event.</summary>
        /// <param name="priorFarmer">The previous player character.</param>
        /// <param name="newFarmer">The new player character.</param>
        internal static void InvokeFarmerChanged(Farmer priorFarmer, Farmer newFarmer)
        {
            PlayerEvents.FarmerChanged.Invoke(null, new EventArgsFarmerChanged(priorFarmer, newFarmer));
        }

        /// <summary>Raise an <see cref="InventoryChanged"/> event.</summary>
        /// <param name="inventory">The player's inventory.</param>
        /// <param name="changedItems">The inventory changes.</param>
        internal static void InvokeInventoryChanged(List<Item> inventory, IEnumerable<ItemStackChange> changedItems)
        {
            PlayerEvents.InventoryChanged.Invoke(null, new EventArgsInventoryChanged(inventory, changedItems.ToList()));
        }

        /// <summary>Rase a <see cref="LeveledUp"/> event.</summary>
        /// <param name="type">The player skill that leveled up.</param>
        /// <param name="newLevel">The new skill level.</param>
        internal static void InvokeLeveledUp(EventArgsLevelUp.LevelType type, int newLevel)
        {
            PlayerEvents.LeveledUp.Invoke(null, new EventArgsLevelUp(type, newLevel));
        }
    }
}
