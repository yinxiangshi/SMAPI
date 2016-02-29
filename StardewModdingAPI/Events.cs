using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace StardewModdingAPI
{
    public static class Events
    {
        public delegate void BlankEventHandler();

        public static event BlankEventHandler GameLoaded = delegate { };
        public static event BlankEventHandler Initialize = delegate { };
        public static event BlankEventHandler LoadContent = delegate { };
        public static event BlankEventHandler UpdateTick = delegate { };
        public static event BlankEventHandler DrawTick = delegate { };

        public delegate void StateChanged(KeyboardState newState);
        public static event StateChanged KeyboardChanged = delegate { };

        public delegate void KeyStateChanged(Keys key);
        public static event KeyStateChanged KeyPressed = delegate { };


        public static void InvokeGameLoaded()
        {
            GameLoaded.Invoke();
        }

        public static void InvokeInitialize()
        {
            try
            {
                Initialize.Invoke();
            }
            catch (Exception ex)
            {
                Program.LogError("An exception occured in XNA Initialize: " + ex.ToString());
            }
        }

        public static void InvokeLoadContent()
        {
            try
            {
                LoadContent.Invoke();
            }
            catch (Exception ex)
            {
                Program.LogError("An exception occured in XNA LoadContent: " + ex.ToString());
            }
        }

        public static void InvokeUpdateTick()
        {
            try
            {
                UpdateTick.Invoke();
            }
            catch (Exception ex)
            {
                Program.LogError("An exception occured in XNA UpdateTick: " + ex.ToString());
            }
        }

        public static void InvokeDrawTick()
        {
            try
            {
                DrawTick.Invoke();
            }
            catch (Exception ex)
            {
                Program.LogError("An exception occured in XNA DrawTick: " + ex.ToString());
            }
        }

        public static void InvokeKeyboardChanged(KeyboardState newState)
        {
            KeyboardChanged.Invoke(newState);
        }

        public static void InvokeKeyPressed(Keys key)
        {
            KeyPressed.Invoke(key);
        }
    }
}
