using System;
using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised during the multiplayer sync process.</summary>
    public static class MultiplayerEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>The core event manager.</summary>
        private static EventManager EventManager;


        /*********
        ** Events
        *********/
        /// <summary>Raised before the game syncs changes from other players.</summary>
        public static event EventHandler BeforeMainSync
        {
            add => MultiplayerEvents.EventManager.Multiplayer_BeforeMainSync.Add(value);
            remove => MultiplayerEvents.EventManager.Multiplayer_BeforeMainSync.Remove(value);
        }

        /// <summary>Raised after the game syncs changes from other players.</summary>
        public static event EventHandler AfterMainSync
        {
            add => MultiplayerEvents.EventManager.Multiplayer_AfterMainSync.Add(value);
            remove => MultiplayerEvents.EventManager.Multiplayer_AfterMainSync.Remove(value);
        }

        /// <summary>Raised before the game broadcasts changes to other players.</summary>
        public static event EventHandler BeforeMainBroadcast
        {
            add => MultiplayerEvents.EventManager.Multiplayer_BeforeMainBroadcast.Add(value);
            remove => MultiplayerEvents.EventManager.Multiplayer_BeforeMainBroadcast.Remove(value);
        }

        /// <summary>Raised after the game broadcasts changes to other players.</summary>
        public static event EventHandler AfterMainBroadcast
        {
            add => MultiplayerEvents.EventManager.Multiplayer_AfterMainBroadcast.Add(value);
            remove => MultiplayerEvents.EventManager.Multiplayer_AfterMainBroadcast.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the events.</summary>
        /// <param name="eventManager">The core event manager.</param>
        internal static void Init(EventManager eventManager)
        {
            MultiplayerEvents.EventManager = eventManager;
        }
    }
}
