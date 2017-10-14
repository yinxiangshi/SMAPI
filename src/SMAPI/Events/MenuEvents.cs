using System;
using StardewModdingAPI.Framework;
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
        public static event EventHandler<EventArgsClickableMenuChanged> MenuChanged;

        /// <summary>Raised after a game menu is closed.</summary>
        public static event EventHandler<EventArgsClickableMenuClosed> MenuClosed;


        /*********
        ** Internal methods
        *********/
        /// <summary>Raise a <see cref="MenuChanged"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="priorMenu">The previous menu.</param>
        /// <param name="newMenu">The current menu.</param>
        internal static void InvokeMenuChanged(IMonitor monitor, IClickableMenu priorMenu, IClickableMenu newMenu)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(MenuEvents)}.{nameof(MenuEvents.MenuChanged)}", MenuEvents.MenuChanged?.GetInvocationList(), null, new EventArgsClickableMenuChanged(priorMenu, newMenu));
        }

        /// <summary>Raise a <see cref="MenuClosed"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="priorMenu">The menu that was closed.</param>
        internal static void InvokeMenuClosed(IMonitor monitor, IClickableMenu priorMenu)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(MenuEvents)}.{nameof(MenuEvents.MenuClosed)}", MenuEvents.MenuClosed?.GetInvocationList(), null, new EventArgsClickableMenuClosed(priorMenu));
        }
    }
}
