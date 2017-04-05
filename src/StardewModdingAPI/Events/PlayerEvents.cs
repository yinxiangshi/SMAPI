using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Framework;
using StardewValley;
using SFarmer = StardewValley.Farmer;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the player data changes.</summary>
    public static class PlayerEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>Manages deprecation warnings.</summary>
        private static DeprecationManager DeprecationManager;


        /*********
        ** Events
        *********/
        /// <summary>Raised after the player loads a saved game.</summary>
        [Obsolete("Use " + nameof(SaveEvents) + "." + nameof(SaveEvents.AfterLoad) + " instead")]
        public static event EventHandler<EventArgsLoadedGameChanged> LoadedGame;

        /// <summary>Raised after the game assigns a new player character. This happens just before <see cref="LoadedGame"/>; it's unclear how this would happen any other time.</summary>
        [Obsolete("should no longer be used")]
        public static event EventHandler<EventArgsFarmerChanged> FarmerChanged;

        /// <summary>Raised after the player's inventory changes in any way (added or removed item, sorted, etc).</summary>
        public static event EventHandler<EventArgsInventoryChanged> InventoryChanged;

        /// <summary> Raised after the player levels up a skill. This happens as soon as they level up, not when the game notifies the player after their character goes to bed.</summary>
        public static event EventHandler<EventArgsLevelUp> LeveledUp;


        /*********
        ** Internal methods
        *********/
        /// <summary>Injects types required for backwards compatibility.</summary>
        /// <param name="deprecationManager">Manages deprecation warnings.</param>
        internal static void Shim(DeprecationManager deprecationManager)
        {
            PlayerEvents.DeprecationManager = deprecationManager;
        }

        /// <summary>Raise a <see cref="LoadedGame"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="loaded">Whether the save has been loaded. This is always true.</param>
        internal static void InvokeLoadedGame(IMonitor monitor, EventArgsLoadedGameChanged loaded)
        {
            if (PlayerEvents.LoadedGame == null)
                return;

            string name = $"{nameof(PlayerEvents)}.{nameof(PlayerEvents.LoadedGame)}";
            Delegate[] handlers = PlayerEvents.LoadedGame.GetInvocationList();

            PlayerEvents.DeprecationManager.WarnForEvent(handlers, name, "1.6", DeprecationLevel.Info);
            monitor.SafelyRaiseGenericEvent(name, handlers, null, loaded);
        }

        /// <summary>Raise a <see cref="FarmerChanged"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="priorFarmer">The previous player character.</param>
        /// <param name="newFarmer">The new player character.</param>
        internal static void InvokeFarmerChanged(IMonitor monitor, SFarmer priorFarmer, SFarmer newFarmer)
        {
            if (PlayerEvents.FarmerChanged == null)
                return;

            string name = $"{nameof(PlayerEvents)}.{nameof(PlayerEvents.FarmerChanged)}";
            Delegate[] handlers = PlayerEvents.FarmerChanged.GetInvocationList();

            PlayerEvents.DeprecationManager.WarnForEvent(handlers, name, "1.6", DeprecationLevel.Info);
            monitor.SafelyRaiseGenericEvent(name, handlers, null, new EventArgsFarmerChanged(priorFarmer, newFarmer));
        }

        /// <summary>Raise an <see cref="InventoryChanged"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="inventory">The player's inventory.</param>
        /// <param name="changedItems">The inventory changes.</param>
        internal static void InvokeInventoryChanged(IMonitor monitor, List<Item> inventory, IEnumerable<ItemStackChange> changedItems)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(PlayerEvents)}.{nameof(PlayerEvents.InventoryChanged)}", PlayerEvents.InventoryChanged?.GetInvocationList(), null, new EventArgsInventoryChanged(inventory, changedItems.ToList()));
        }

        /// <summary>Rase a <see cref="LeveledUp"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="type">The player skill that leveled up.</param>
        /// <param name="newLevel">The new skill level.</param>
        internal static void InvokeLeveledUp(IMonitor monitor, EventArgsLevelUp.LevelType type, int newLevel)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(PlayerEvents)}.{nameof(PlayerEvents.LeveledUp)}", PlayerEvents.LeveledUp?.GetInvocationList(), null, new EventArgsLevelUp(type, newLevel));
        }
    }
}
