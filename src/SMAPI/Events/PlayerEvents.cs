#if !SMAPI_3_0_STRICT
using System;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the player data changes.</summary>
    [Obsolete("Use " + nameof(Mod.Helper) + "." + nameof(IModHelper.Events) + " instead. See https://smapi.io/3.0 for more info.")]
    public static class PlayerEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>The core event manager.</summary>
        private static EventManager EventManager;

        /// <summary>Manages deprecation warnings.</summary>
        private static DeprecationManager DeprecationManager;


        /*********
        ** Events
        *********/
        /// <summary>Raised after the player's inventory changes in any way (added or removed item, sorted, etc).</summary>
        public static event EventHandler<EventArgsInventoryChanged> InventoryChanged
        {
            add
            {
                PlayerEvents.DeprecationManager.WarnForOldEvents();
                PlayerEvents.EventManager.Legacy_InventoryChanged.Add(value);
            }
            remove => PlayerEvents.EventManager.Legacy_InventoryChanged.Remove(value);
        }

        /// <summary>Raised after the player levels up a skill. This happens as soon as they level up, not when the game notifies the player after their character goes to bed.</summary>
        public static event EventHandler<EventArgsLevelUp> LeveledUp
        {
            add
            {
                PlayerEvents.DeprecationManager.WarnForOldEvents();
                PlayerEvents.EventManager.Legacy_LeveledUp.Add(value);
            }
            remove => PlayerEvents.EventManager.Legacy_LeveledUp.Remove(value);
        }

        /// <summary>Raised after the player warps to a new location.</summary>
        public static event EventHandler<EventArgsPlayerWarped> Warped
        {
            add
            {
                PlayerEvents.DeprecationManager.WarnForOldEvents();
                PlayerEvents.EventManager.Legacy_PlayerWarped.Add(value);
            }
            remove => PlayerEvents.EventManager.Legacy_PlayerWarped.Remove(value);
        }



        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the events.</summary>
        /// <param name="eventManager">The core event manager.</param>
        /// <param name="deprecationManager">Manages deprecation warnings.</param>
        internal static void Init(EventManager eventManager, DeprecationManager deprecationManager)
        {
            PlayerEvents.EventManager = eventManager;
            PlayerEvents.DeprecationManager = deprecationManager;
        }
    }
}
#endif
