using System;
using StardewValley.Menus;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when a game menu is opened or closed (including internal menus like the title screen).</summary>
    public static class MenuEvents
    {
        /*********
        ** Events
        *********/
        /// <summary>Raised after a game menu is opened or replaced with another menu. This event is not invoked when a menu is closed.</summary>
        public static event EventHandler<EventArgsClickableMenuChanged> MenuChanged = delegate { };

        /// <summary>Raised after a game menu is closed.</summary>
        public static event EventHandler<EventArgsClickableMenuClosed> MenuClosed = delegate { };


        /*********
        ** Internal methods
        *********/
        /// <summary>Raise a <see cref="MenuChanged"/> event.</summary>
        /// <param name="priorMenu">The previous menu.</param>
        /// <param name="newMenu">The current menu.</param>
        internal static void InvokeMenuChanged(IClickableMenu priorMenu, IClickableMenu newMenu)
        {
            MenuEvents.MenuChanged.Invoke(null, new EventArgsClickableMenuChanged(priorMenu, newMenu));
        }

        /// <summary>Raise a <see cref="MenuClosed"/> event.</summary>
        /// <param name="priorMenu">The menu that was closed.</param>
        internal static void InvokeMenuClosed(IClickableMenu priorMenu)
        {
            MenuEvents.MenuClosed.Invoke(null, new EventArgsClickableMenuClosed(priorMenu));
        }
    }
}
