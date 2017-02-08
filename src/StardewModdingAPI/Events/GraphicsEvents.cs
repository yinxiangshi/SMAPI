using System;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised during the game's draw loop, when the game is rendering content to the window.</summary>
    public static class GraphicsEvents
    {
        /*********
        ** Events
        *********/
        /****
        ** Generic events
        ****/
        /// <summary>Raised after the game window is resized.</summary>
        public static event EventHandler Resize;

        /// <summary>Raised when drawing debug information to the screen (when <see cref="SGame.Debug"/> is true). This is called after the sprite batch is begun. If you just want to add debug info, use <see cref="SGame.DebugMessageQueue" /> in your update loop.</summary>
        public static event EventHandler DrawDebug;

        /****
        ** Main render events
        ****/
        /// <summary>Raised before drawing the world to the screen.</summary>
        public static event EventHandler OnPreRenderEvent;

        /// <summary>Raised after drawing the world to the screen.</summary>
        public static event EventHandler OnPostRenderEvent;

        /****
        ** HUD events
        ****/
        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The HUD is available at this point, but not necessarily visible. (For example, the event is raised even if a menu is open.)</summary>
        public static event EventHandler OnPreRenderHudEvent;

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the screen. The HUD is available at this point, but not necessarily visible. (For example, the event is raised even if a menu is open.)</summary>
        public static event EventHandler OnPostRenderHudEvent;

        /****
        ** GUI events
        ****/
        /// <summary>Raised before drawing a menu to the screen during a draw loop. This includes the game's internal menus like the title screen.</summary>
        public static event EventHandler OnPreRenderGuiEvent;

        /// <summary>Raised after drawing a menu to the screen during a draw loop. This includes the game's internal menus like the title screen.</summary>
        public static event EventHandler OnPostRenderGuiEvent;


        /*********
        ** Internal methods
        *********/
        /****
        ** Generic events
        ****/
        /// <summary>Raise a <see cref="Resize"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeResize(IMonitor monitor, object sender, EventArgs e)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GraphicsEvents)}.{nameof(GraphicsEvents.Resize)}", GraphicsEvents.Resize?.GetInvocationList(), sender, e);
        }

        /// <summary>Raise a <see cref="DrawDebug"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeDrawDebug(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GraphicsEvents)}.{nameof(GraphicsEvents.DrawDebug)}", GraphicsEvents.DrawDebug?.GetInvocationList());
        }

        /****
        ** Main render events
        ****/
        /// <summary>Raise an <see cref="OnPreRenderEvent"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeOnPreRenderEvent(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GraphicsEvents)}.{nameof(GraphicsEvents.OnPreRenderEvent)}", GraphicsEvents.OnPreRenderEvent?.GetInvocationList());
        }

        /// <summary>Raise an <see cref="OnPostRenderEvent"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeOnPostRenderEvent(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GraphicsEvents)}.{nameof(GraphicsEvents.OnPostRenderEvent)}", GraphicsEvents.OnPostRenderEvent?.GetInvocationList());
        }

        /****
        ** GUI events
        ****/
        /// <summary>Raise an <see cref="OnPreRenderGuiEvent"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeOnPreRenderGuiEvent(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GraphicsEvents)}.{nameof(GraphicsEvents.OnPreRenderGuiEvent)}", GraphicsEvents.OnPreRenderGuiEvent?.GetInvocationList());
        }

        /// <summary>Raise an <see cref="OnPostRenderGuiEvent"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeOnPostRenderGuiEvent(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GraphicsEvents)}.{nameof(GraphicsEvents.OnPostRenderGuiEvent)}", GraphicsEvents.OnPostRenderGuiEvent?.GetInvocationList());
        }

        /****
        ** HUD events
        ****/
        /// <summary>Raise an <see cref="OnPreRenderHudEvent"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeOnPreRenderHudEvent(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GraphicsEvents)}.{nameof(GraphicsEvents.OnPreRenderHudEvent)}", GraphicsEvents.OnPreRenderHudEvent?.GetInvocationList());
        }

        /// <summary>Raise an <see cref="OnPostRenderHudEvent"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeOnPostRenderHudEvent(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(GraphicsEvents)}.{nameof(GraphicsEvents.OnPostRenderHudEvent)}", GraphicsEvents.OnPostRenderHudEvent?.GetInvocationList());
        }
    }
}
