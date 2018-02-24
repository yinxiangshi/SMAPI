using System;
using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when a game menu is opened or closed (including internal menus like the title screen).</summary>
    public static class MenuEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>The core event manager.</summary>
        private static EventManager EventManager;


        /*********
        ** Events
        *********/
        /// <summary>Raised after a game menu is opened or replaced with another menu. This event is not invoked when a menu is closed.</summary>
        public static event EventHandler<EventArgsClickableMenuChanged> MenuChanged
        {
            add => MenuEvents.EventManager.Menu_Changed.Add(value);
            remove => MenuEvents.EventManager.Menu_Changed.Remove(value);
        }

        /// <summary>Raised after a game menu is closed.</summary>
        public static event EventHandler<EventArgsClickableMenuClosed> MenuClosed
        {
            add => MenuEvents.EventManager.Menu_Closed.Add(value);
            remove => MenuEvents.EventManager.Menu_Closed.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the events.</summary>
        /// <param name="eventManager">The core event manager.</param>
        internal static void Init(EventManager eventManager)
        {
            MenuEvents.EventManager = eventManager;
        }
    }
}
