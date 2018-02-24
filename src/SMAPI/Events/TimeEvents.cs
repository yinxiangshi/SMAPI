using System;
using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the in-game date or time changes.</summary>
    public static class TimeEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>The core event manager.</summary>
        private static EventManager EventManager;


        /*********
        ** Events
        *********/
        /// <summary>Raised after the game begins a new day, including when loading a save.</summary>
        public static event EventHandler AfterDayStarted
        {
            add => TimeEvents.EventManager.Time_AfterDayStarted.Add(value);
            remove => TimeEvents.EventManager.Time_AfterDayStarted.Remove(value);
        }

        /// <summary>Raised after the in-game clock changes.</summary>
        public static event EventHandler<EventArgsIntChanged> TimeOfDayChanged
        {
            add => TimeEvents.EventManager.Time_TimeOfDayChanged.Add(value);
            remove => TimeEvents.EventManager.Time_TimeOfDayChanged.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the events.</summary>
        /// <param name="eventManager">The core event manager.</param>
        internal static void Init(EventManager eventManager)
        {
            TimeEvents.EventManager = eventManager;
        }
    }
}
