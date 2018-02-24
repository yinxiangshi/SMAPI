using System;
using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when something happens in the mines.</summary>
    public static class MineEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>The core event manager.</summary>
        private static EventManager EventManager;


        /*********
        ** Events
        *********/
        /// <summary>Raised after the player warps to a new level of the mine.</summary>
        public static event EventHandler<EventArgsMineLevelChanged> MineLevelChanged
        {
            add => MineEvents.EventManager.Mine_LevelChanged.Add(value);
            remove => MineEvents.EventManager.Mine_LevelChanged.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the events.</summary>
        /// <param name="eventManager">The core event manager.</param>
        internal static void Init(EventManager eventManager)
        {
            MineEvents.EventManager = eventManager;
        }
    }
}
