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
        public static event EventHandler Resize = delegate { };

        /// <summary>Raised when drawing debug information to the screen (when <see cref="StardewModdingAPI.Inheritance.SGame.Debug"/> is true). This is called after the sprite batch is begun. If you just want to add debug info, use <see cref="StardewModdingAPI.Inheritance.SGame.DebugMessageQueue" /> in your update loop.</summary>
        public static event EventHandler DrawDebug = delegate { };

        /// <summary>Obsolete.</summary>
        [Obsolete("Use the other Pre/Post render events instead.")]
        public static event EventHandler DrawTick = delegate { };

        /// <summary>Obsolete.</summary>
        [Obsolete("Use the other Pre/Post render events instead. All of them will automatically be drawn into the render target if needed.")]
        public static event EventHandler DrawInRenderTargetTick = delegate { };

        /****
        ** Main render events
        ****/
        /// <summary>Raised before drawing everything to the screen during a draw loop.</summary>
        public static event EventHandler OnPreRenderEvent = delegate { };

        /// <summary>Raised after drawing everything to the screen during a draw loop.</summary>
        public static event EventHandler OnPostRenderEvent = delegate { };

        /****
        ** HUD events
        ****/
        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The HUD is available at this point, but not necessarily visible. (For example, the event is raised even if a menu is open.)</summary>
        public static event EventHandler OnPreRenderHudEvent = delegate { };

        /// <summary>Equivalent to <see cref="OnPreRenderHudEvent"/>, but invoked even if the HUD isn't available.</summary>
        public static event EventHandler OnPreRenderHudEventNoCheck = delegate { };

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the screen. The HUD is available at this point, but not necessarily visible. (For example, the event is raised even if a menu is open.)</summary>
        public static event EventHandler OnPostRenderHudEvent = delegate { };

        /// <summary>Equivalent to <see cref="OnPostRenderHudEvent"/>, but invoked even if the HUD isn't available.</summary>
        public static event EventHandler OnPostRenderHudEventNoCheck = delegate { };

        /****
        ** GUI events
        ****/
        /// <summary>Raised before drawing a menu to the screen during a draw loop. This includes the game's internal menus like the title screen.</summary>
        public static event EventHandler OnPreRenderGuiEvent = delegate { };

        /// <summary>Equivalent to <see cref="OnPreRenderGuiEvent"/>, but invoked even if there's no menu being drawn.</summary>
        public static event EventHandler OnPreRenderGuiEventNoCheck = delegate { };

        /// <summary>Raised after drawing a menu to the screen during a draw loop. This includes the game's internal menus like the title screen.</summary>
        public static event EventHandler OnPostRenderGuiEvent = delegate { };

        /// <summary>Equivalent to <see cref="OnPreRenderGuiEvent"/>, but invoked even if there's no menu being drawn.</summary>
        public static event EventHandler OnPostRenderGuiEventNoCheck = delegate { };


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
            GraphicsEvents.Resize.Invoke(sender, e);
        }

        /// <summary>Raise a <see cref="DrawDebug"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeDrawDebug(object sender, EventArgs e)
        {
            GraphicsEvents.DrawDebug.Invoke(sender, e);
        }

        /// <summary>Raise a <see cref="DrawTick"/> event.</summary>
        [Obsolete("Should not be used.")]
        public static void InvokeDrawTick()
        {
            try
            {
                GraphicsEvents.DrawTick.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Log.AsyncR("An exception occured in a Mod's DrawTick: " + ex);
            }
        }

        /// <summary>Raise a <see cref="DrawInRenderTargetTick"/> event.</summary>
        [Obsolete("Should not be used.")]
        public static void InvokeDrawInRenderTargetTick()
        {
            GraphicsEvents.DrawInRenderTargetTick.Invoke(null, EventArgs.Empty);
        }

        /****
        ** Main render events
        ****/
        /// <summary>Raise an <see cref="OnPreRenderEvent"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPreRenderEvent(object sender, EventArgs e)
        {
            GraphicsEvents.OnPreRenderEvent.Invoke(sender, e);
        }

        /// <summary>Raise an <see cref="OnPostRenderEvent"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPostRenderEvent(object sender, EventArgs e)
        {
            GraphicsEvents.OnPostRenderEvent.Invoke(sender, e);
        }

        /****
        ** HUD events
        ****/
        /// <summary>Raise an <see cref="OnPreRenderGuiEvent"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPreRenderGuiEvent(object sender, EventArgs e)
        {
            GraphicsEvents.OnPreRenderGuiEvent.Invoke(sender, e);
        }

        /// <summary>Raise an <see cref="OnPreRenderGuiEventNoCheck"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPreRenderGuiEventNoCheck(object sender, EventArgs e)
        {
            GraphicsEvents.OnPreRenderGuiEventNoCheck.Invoke(sender, e);
        }

        /// <summary>Raise an <see cref="OnPostRenderGuiEvent"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPostRenderGuiEvent(object sender, EventArgs e)
        {
            GraphicsEvents.OnPostRenderGuiEvent.Invoke(sender, e);
        }

        /// <summary>Raise an <see cref="OnPostRenderGuiEventNoCheck"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPostRenderGuiEventNoCheck(object sender, EventArgs e)
        {
            GraphicsEvents.OnPostRenderGuiEventNoCheck.Invoke(sender, e);
        }

        /****
        ** GUI events
        ****/
        /// <summary>Raise an <see cref="OnPreRenderHudEvent"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPreRenderHudEvent(object sender, EventArgs e)
        {
            GraphicsEvents.OnPreRenderHudEvent.Invoke(sender, e);
        }

        /// <summary>Raise an <see cref="OnPreRenderHudEventNoCheck"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPreRenderHudEventNoCheck(object sender, EventArgs e)
        {
            GraphicsEvents.OnPreRenderHudEventNoCheck.Invoke(sender, e);
        }

        /// <summary>Raise an <see cref="OnPostRenderHudEvent"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPostRenderHudEvent(object sender, EventArgs e)
        {
            GraphicsEvents.OnPostRenderHudEvent.Invoke(sender, e);
        }

        /// <summary>Raise an <see cref="OnPostRenderHudEventNoCheck"/> event.</summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The event arguments.</param>
        internal static void InvokeOnPostRenderHudEventNoCheck(object sender, EventArgs e)
        {
            GraphicsEvents.OnPostRenderHudEventNoCheck.Invoke(sender, e);
        }
    }
}
