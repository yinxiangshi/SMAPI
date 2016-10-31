using System;
using StardewValley.Menus;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="MenuEvents.MenuChanged"/> event.</summary>
    public class EventArgsClickableMenuChanged : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The previous menu.</summary>
        public IClickableMenu NewMenu { get; private set; }

        /// <summary>The current menu.</summary>
        public IClickableMenu PriorMenu { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="priorMenu">The previous menu.</param>
        /// <param name="newMenu">The current menu.</param>
        public EventArgsClickableMenuChanged(IClickableMenu priorMenu, IClickableMenu newMenu)
        {
            this.NewMenu = newMenu;
            this.PriorMenu = priorMenu;
        }
    }
}
