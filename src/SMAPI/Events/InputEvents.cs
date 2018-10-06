using System;
using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the player uses a controller, keyboard, or mouse button.</summary>
    public static class InputEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>The core event manager.</summary>
        private static EventManager EventManager;


        /*********
        ** Events
        *********/
        /// <summary>Raised when the player presses a button on the keyboard, controller, or mouse.</summary>
        public static event EventHandler<EventArgsInput> ButtonPressed
        {
            add => InputEvents.EventManager.Legacy_ButtonPressed.Add(value);
            remove => InputEvents.EventManager.Legacy_ButtonPressed.Remove(value);
        }

        /// <summary>Raised when the player releases a keyboard key on the keyboard, controller, or mouse.</summary>
        public static event EventHandler<EventArgsInput> ButtonReleased
        {
            add => InputEvents.EventManager.Legacy_ButtonReleased.Add(value);
            remove => InputEvents.EventManager.Legacy_ButtonReleased.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the events.</summary>
        /// <param name="eventManager">The core event manager.</param>
        internal static void Init(EventManager eventManager)
        {
            InputEvents.EventManager = eventManager;
        }
    }
}
