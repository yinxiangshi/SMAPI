using System;
using StardewValley.Menus;

namespace StardewModdingAPI.Events
{
    public class EventArgsClickableMenuClosed : EventArgs
    {
        public EventArgsClickableMenuClosed(IClickableMenu priorMenu)
        {
            PriorMenu = priorMenu;
        }

        public IClickableMenu PriorMenu { get; private set; }
    }
}