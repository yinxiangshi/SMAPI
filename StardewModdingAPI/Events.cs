using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewModdingAPI
{
    public static class Events
    {
        public delegate void BlankEventHandler();

        public static event BlankEventHandler GameLoaded = delegate { };
        public static event BlankEventHandler UpdateInitialized = delegate { };
        public static event BlankEventHandler UpdateTick = delegate { };

        public static void InvokeGameLoaded()
        {
            GameLoaded.Invoke();
        }

        public static void InvokeUpdateInitialized()
        {
            UpdateInitialized.Invoke();
        }

        public static void InvokeUpdateTick()
        {
            UpdateTick.Invoke();
        }
    }
}
