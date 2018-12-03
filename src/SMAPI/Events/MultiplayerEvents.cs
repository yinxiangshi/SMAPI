using System;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised during the multiplayer sync process.</summary>
    [Obsolete("Use " + nameof(Mod.Helper) + "." + nameof(IModHelper.Events) + " instead. See https://smapi.io/3.0 for more info.")]
    public static class MultiplayerEvents
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
        /// <summary>Raised before the game syncs changes from other players.</summary>
        public static event EventHandler BeforeMainSync
        {
            add
            {
                MultiplayerEvents.DeprecationManager.WarnForOldEvents();
                MultiplayerEvents.EventManager.Legacy_BeforeMainSync.Add(value);
            }
            remove => MultiplayerEvents.EventManager.Legacy_BeforeMainSync.Remove(value);
        }

        /// <summary>Raised after the game syncs changes from other players.</summary>
        public static event EventHandler AfterMainSync
        {
            add
            {
                MultiplayerEvents.DeprecationManager.WarnForOldEvents();
                MultiplayerEvents.EventManager.Legacy_AfterMainSync.Add(value);
            }
            remove => MultiplayerEvents.EventManager.Legacy_AfterMainSync.Remove(value);
        }

        /// <summary>Raised before the game broadcasts changes to other players.</summary>
        public static event EventHandler BeforeMainBroadcast
        {
            add
            {
                MultiplayerEvents.DeprecationManager.WarnForOldEvents();
                MultiplayerEvents.EventManager.Legacy_BeforeMainBroadcast.Add(value);
            }
            remove => MultiplayerEvents.EventManager.Legacy_BeforeMainBroadcast.Remove(value);
        }

        /// <summary>Raised after the game broadcasts changes to other players.</summary>
        public static event EventHandler AfterMainBroadcast
        {
            add
            {
                MultiplayerEvents.DeprecationManager.WarnForOldEvents();
                MultiplayerEvents.EventManager.Legacy_AfterMainBroadcast.Add(value);
            }
            remove => MultiplayerEvents.EventManager.Legacy_AfterMainBroadcast.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the events.</summary>
        /// <param name="eventManager">The core event manager.</param>
        /// <param name="deprecationManager">Manages deprecation warnings.</param>
        internal static void Init(EventManager eventManager, DeprecationManager deprecationManager)
        {
            MultiplayerEvents.EventManager = eventManager;
            MultiplayerEvents.DeprecationManager = deprecationManager;
        }
    }
}
