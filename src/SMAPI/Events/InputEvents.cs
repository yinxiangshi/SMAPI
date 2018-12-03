using System;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the player uses a controller, keyboard, or mouse button.</summary>
    [Obsolete("Use " + nameof(Mod.Helper) + "." + nameof(IModHelper.Events) + " instead. See https://smapi.io/3.0 for more info.")]
    public static class InputEvents
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
        /// <summary>Raised when the player presses a button on the keyboard, controller, or mouse.</summary>
        public static event EventHandler<EventArgsInput> ButtonPressed
        {
            add
            {
                InputEvents.DeprecationManager.WarnForOldEvents();
                InputEvents.EventManager.Legacy_ButtonPressed.Add(value);
            }
            remove => InputEvents.EventManager.Legacy_ButtonPressed.Remove(value);
        }

        /// <summary>Raised when the player releases a keyboard key on the keyboard, controller, or mouse.</summary>
        public static event EventHandler<EventArgsInput> ButtonReleased
        {
            add
            {
                InputEvents.DeprecationManager.WarnForOldEvents();
                InputEvents.EventManager.Legacy_ButtonReleased.Add(value);
            }
            remove => InputEvents.EventManager.Legacy_ButtonReleased.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the events.</summary>
        /// <param name="eventManager">The core event manager.</param>
        /// <param name="deprecationManager">Manages deprecation warnings.</param>
        internal static void Init(EventManager eventManager, DeprecationManager deprecationManager)
        {
            InputEvents.EventManager = eventManager;
            InputEvents.DeprecationManager = deprecationManager;
        }
    }
}
