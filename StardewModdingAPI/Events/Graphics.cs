using System;

namespace StardewModdingAPI.Events
{
    public static class GraphicsEvents
    {
        public static event EventHandler Resize = delegate { };
        public static event EventHandler DrawTick = delegate { };
        public static event EventHandler DrawInRenderTargetTick = delegate { };

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
    }
}