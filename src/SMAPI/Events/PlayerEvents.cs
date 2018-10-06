using System;
using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the player data changes.</summary>
    public static class PlayerEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>The core event manager.</summary>
        private static EventManager EventManager;


        /*********
        ** Events
        *********/
        /// <summary>Raised after the player's inventory changes in any way (added or removed item, sorted, etc).</summary>
        public static event EventHandler<EventArgsInventoryChanged> InventoryChanged
        {
            add => PlayerEvents.EventManager.Legacy_InventoryChanged.Add(value);
            remove => PlayerEvents.EventManager.Legacy_InventoryChanged.Remove(value);
        }

        /// <summary>Raised after the player levels up a skill. This happens as soon as they level up, not when the game notifies the player after their character goes to bed.</summary>
        public static event EventHandler<EventArgsLevelUp> LeveledUp
        {
            add => PlayerEvents.EventManager.Legacy_LeveledUp.Add(value);
            remove => PlayerEvents.EventManager.Legacy_LeveledUp.Remove(value);
        }

        /// <summary>Raised after the player warps to a new location.</summary>
        public static event EventHandler<EventArgsPlayerWarped> Warped
        {
            add => PlayerEvents.EventManager.Legacy_PlayerWarped.Add(value);
            remove => PlayerEvents.EventManager.Legacy_PlayerWarped.Remove(value);
        }



        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the events.</summary>
        /// <param name="eventManager">The core event manager.</param>
        internal static void Init(EventManager eventManager)
        {
            PlayerEvents.EventManager = eventManager;
        }
    }
}
