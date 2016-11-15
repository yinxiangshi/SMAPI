using System;

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

        /// <summary>Raised when drawing debug information to the screen (when <see cref="StardewModdingAPI.Inheritance.SGame.Debug"/> is true). This is called after the sprite batch is begun. If you just want to add debug info, use <see cref="StardewModdingAPI.Inheritance.SGame.DebugMessageQueue" /> in your update loop.</summary>
        public static event EventHandler DrawDebug;

        /// <summary>Obsolete.</summary>
        [Obsolete("Use the other Pre/Post render events instead.")]
        public static event EventHandler DrawTick;

        /// <summary>Obsolete.</summary>
        [Obsolete("Use the other Pre/Post render events instead. All of them will automatically be drawn into the render target if needed.")]
        public static event EventHandler DrawInRenderTargetTick;

        /****
        ** Main render events
        ****/
        /// <summary>Raised before drawing everything to the screen during a draw loop.</summary>
        public static event EventHandler OnPreRenderEvent;

        /// <summary>Raised after drawing everything to the screen during a draw loop.</summary>
        public static event EventHandler OnPostRenderEvent;

        /****
        ** HUD events
        ****/
        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The HUD is available at this point, but not necessarily visible. (For example, the event is raised even if a menu is open.)</summary>
        public static event EventHandler OnPreRenderHudEvent;

        /// <summary>Equivalent to <see cref="OnPreRenderHudEvent"/>, but invoked even if the HUD isn't available.</summary>
        public static event EventHandler OnPreRenderHudEventNoCheck;

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the screen. The HUD is available at this point, but not necessarily visible. (For example, the event is raised even if a menu is open.)</summary>
        public static event EventHandler OnPostRenderHudEvent;

        /// <summary>Equivalent to <see cref="OnPostRenderHudEvent"/>, but invoked even if the HUD isn't available.</summary>
        public static event EventHandler OnPostRenderHudEventNoCheck;

        /****
        ** GUI events
        ****/
        /// <summary>Raised before drawing a menu to the screen during a draw loop. This includes the game's internal menus like the title screen.</summary>
        public static event EventHandler OnPreRenderGuiEvent;

        /// <summary>Equivalent to <see cref="OnPreRenderGuiEvent"/>, but invoked even if there's no menu being drawn.</summary>
        public static event EventHandler OnPreRenderGuiEventNoCheck;

        /// <summary>Raised after drawing a menu to the screen during a draw loop. This includes the game's internal menus like the title screen.</summary>
        public static event EventHandler OnPostRenderGuiEvent;

        /// <summary>Equivalent to <see cref="OnPreRenderGuiEvent"/>, but invoked even if there's no menu being drawn.</summary>
        public static event EventHandler OnPostRenderGuiEventNoCheck;


        /*********
        ** Internal methods
        *********/
        /****
        ** Generic events
        ****/
        /// <summary>Raise a <see cref="Resize"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeResize(object sender, EventArgs e)
        {
            GraphicsEvents.Resize?.Invoke(sender, e);
        }

        /// <summary>Raise a <see cref="DrawDebug"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeDrawDebug(object sender, EventArgs e)
        {
            GraphicsEvents.DrawDebug?.Invoke(sender, e);
        }

        /// <summary>Raise a <see cref="DrawTick"/> event.</summary>
        /// <param name="monitor">Encapsulates logging and monitoring.</param>
        [Obsolete("Should not be used.")]
        public static void InvokeDrawTick(IMonitor monitor)
        {
            try
            {
                GraphicsEvents.DrawTick?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                monitor.Log($"A mod crashed handling an event.\n{ex}", LogLevel.Error);
            }
        }

        /// <summary>Raise a <see cref="DrawInRenderTargetTick"/> event.</summary>
        [Obsolete("Should not be used.")]
        public static void InvokeDrawInRenderTargetTick()
        {
            GraphicsEvents.DrawInRenderTargetTick?.Invoke(null, EventArgs.Empty);
        }

        /****
        ** Main render events
        ****/
        /// <summary>Raise an <see cref="OnPreRenderEvent"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPreRenderEvent(object sender, EventArgs e)
        {
            GraphicsEvents.OnPreRenderEvent?.Invoke(sender, e);
        }

        /// <summary>Raise an <see cref="OnPostRenderEvent"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPostRenderEvent(object sender, EventArgs e)
        {
            GraphicsEvents.OnPostRenderEvent?.Invoke(sender, e);
        }

        /****
        ** HUD events
        ****/
        /// <summary>Raise an <see cref="OnPreRenderGuiEvent"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPreRenderGuiEvent(object sender, EventArgs e)
        {
            GraphicsEvents.OnPreRenderGuiEvent?.Invoke(sender, e);
        }

        /// <summary>Raise an <see cref="OnPreRenderGuiEventNoCheck"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPreRenderGuiEventNoCheck(object sender, EventArgs e)
        {
            GraphicsEvents.OnPreRenderGuiEventNoCheck?.Invoke(sender, e);
        }

        /// <summary>Raise an <see cref="OnPostRenderGuiEvent"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPostRenderGuiEvent(object sender, EventArgs e)
        {
            GraphicsEvents.OnPostRenderGuiEvent?.Invoke(sender, e);
        }

        /// <summary>Raise an <see cref="OnPostRenderGuiEventNoCheck"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPostRenderGuiEventNoCheck(object sender, EventArgs e)
        {
            GraphicsEvents.OnPostRenderGuiEventNoCheck?.Invoke(sender, e);
        }

        /****
        ** GUI events
        ****/
        /// <summary>Raise an <see cref="OnPreRenderHudEvent"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPreRenderHudEvent(object sender, EventArgs e)
        {
            GraphicsEvents.OnPreRenderHudEvent?.Invoke(sender, e);
        }

        /// <summary>Raise an <see cref="OnPreRenderHudEventNoCheck"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPreRenderHudEventNoCheck(object sender, EventArgs e)
        {
            GraphicsEvents.OnPreRenderHudEventNoCheck?.Invoke(sender, e);
        }

        /// <summary>Raise an <see cref="OnPostRenderHudEvent"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPostRenderHudEvent(object sender, EventArgs e)
        {
            GraphicsEvents.OnPostRenderHudEvent?.Invoke(sender, e);
        }

        /// <summary>Raise an <see cref="OnPostRenderHudEventNoCheck"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPostRenderHudEventNoCheck(object sender, EventArgs e)
        {
            GraphicsEvents.OnPostRenderHudEventNoCheck?.Invoke(sender, e);
        }
    }
}
