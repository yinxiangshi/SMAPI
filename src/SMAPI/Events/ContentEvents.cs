using System;
using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the game loads content.</summary>
    public static class ContentEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>The core event manager.</summary>
        private static EventManager EventManager;


        /*********
        ** Events
        *********/
        /// <summary>Raised after the content language changes.</summary>
        public static event EventHandler<EventArgsValueChanged<string>> AfterLocaleChanged
        {
            add => ContentEvents.EventManager.Legacy_LocaleChanged.Add(value);
            remove => ContentEvents.EventManager.Legacy_LocaleChanged.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the events.</summary>
        /// <param name="eventManager">The core event manager.</param>
        internal static void Init(EventManager eventManager)
        {
            ContentEvents.EventManager = eventManager;
        }
    }
}
