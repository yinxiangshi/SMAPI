using System;

namespace StardewModdingAPI.Events
{
    public static class GraphicsEvents
    {
        public static event EventHandler Resize = delegate { };
        public static event EventHandler DrawTick = delegate { };
        public static event EventHandler DrawInRenderTargetTick = delegate { };

        /// <summary>
        /// Draws when SGame.Debug is true. F3 toggles this.
        /// Game1.spriteBatch.Begin() is pre-called.
        /// Do not make end or begin calls to the spritebatch.
        /// If you are only trying to add debug information, use SGame.DebugMessageQueue in your Update loop.
        /// </summary>
        public static event EventHandler DrawDebug = delegate { };

        public static void InvokeDrawTick()
        {
            try
            {
                DrawTick.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Log.AsyncR("An exception occured in a Mod's DrawTick: " + ex);
            }
        }

        public static void InvokeDrawInRenderTargetTick()
        {
            DrawInRenderTargetTick.Invoke(null, EventArgs.Empty);
        }

        public static void InvokeResize(object sender, EventArgs e)
        {
            Resize.Invoke(sender, e);
        }

        public static void InvokeDrawDebug(object sender, EventArgs e)
        {
            DrawDebug.Invoke(sender, e);
        }
    }
}