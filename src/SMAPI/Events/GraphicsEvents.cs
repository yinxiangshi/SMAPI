using System;
using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised during the game's draw loop, when the game is rendering content to the window.</summary>
    public static class GraphicsEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>The core event manager.</summary>
        private static EventManager EventManager;


        /*********
        ** Events
        *********/
        /// <summary>Raised after the game window is resized.</summary>
        public static event EventHandler Resize
        {
            add => GraphicsEvents.EventManager.Graphics_Resize.Add(value);
            remove => GraphicsEvents.EventManager.Graphics_Resize.Remove(value);
        }

        /****
        ** Main render events
        ****/
        /// <summary>Raised before drawing the world to the screen.</summary>
        public static event EventHandler OnPreRenderEvent
        {
            add => GraphicsEvents.EventManager.Graphics_OnPreRenderEvent.Add(value);
            remove => GraphicsEvents.EventManager.Graphics_OnPreRenderEvent.Remove(value);
        }

        /// <summary>Raised after drawing the world to the screen.</summary>
        public static event EventHandler OnPostRenderEvent
        {
            add => GraphicsEvents.EventManager.Graphics_OnPostRenderEvent.Add(value);
            remove => GraphicsEvents.EventManager.Graphics_OnPostRenderEvent.Remove(value);
        }

        /****
        ** HUD events
        ****/
        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The HUD is available at this point, but not necessarily visible. (For example, the event is raised even if a menu is open.)</summary>
        public static event EventHandler OnPreRenderHudEvent
        {
            add => GraphicsEvents.EventManager.Graphics_OnPreRenderHudEvent.Add(value);
            remove => GraphicsEvents.EventManager.Graphics_OnPreRenderHudEvent.Remove(value);
        }

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the screen. The HUD is available at this point, but not necessarily visible. (For example, the event is raised even if a menu is open.)</summary>
        public static event EventHandler OnPostRenderHudEvent
        {
            add => GraphicsEvents.EventManager.Graphics_OnPostRenderHudEvent.Add(value);
            remove => GraphicsEvents.EventManager.Graphics_OnPostRenderHudEvent.Remove(value);
        }

        /****
        ** GUI events
        ****/
        /// <summary>Raised before drawing a menu to the screen during a draw loop. This includes the game's internal menus like the title screen.</summary>
        public static event EventHandler OnPreRenderGuiEvent
        {
            add => GraphicsEvents.EventManager.Graphics_OnPreRenderGuiEvent.Add(value);
            remove => GraphicsEvents.EventManager.Graphics_OnPreRenderGuiEvent.Remove(value);
        }

        /// <summary>Raised after drawing a menu to the screen during a draw loop. This includes the game's internal menus like the title screen.</summary>
        public static event EventHandler OnPostRenderGuiEvent
        {
            add => GraphicsEvents.EventManager.Graphics_OnPostRenderGuiEvent.Add(value);
            remove => GraphicsEvents.EventManager.Graphics_OnPostRenderGuiEvent.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the events.</summary>
        /// <param name="eventManager">The core event manager.</param>
        internal static void Init(EventManager eventManager)
        {
            GraphicsEvents.EventManager = eventManager;
        }
    }
}
