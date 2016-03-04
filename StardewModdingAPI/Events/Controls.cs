using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewModdingAPI.Events
{
    public static class ControlEvents
    {
        public static event EventHandler<EventArgsKeyboardStateChanged> KeyboardChanged = delegate { };
        public static event EventHandler<EventArgsKeyPressed> KeyPressed = delegate { };
        public static event EventHandler<EventArgsMouseStateChanged> MouseChanged = delegate { };

        public static void InvokeKeyboardChanged(KeyboardState priorState, KeyboardState newState)
        {
            KeyboardChanged.Invoke(null, new EventArgsKeyboardStateChanged(priorState, newState));
        }

        public static void InvokeMouseChanged(MouseState priorState, MouseState newState)
        {
            MouseChanged.Invoke(null, new EventArgsMouseStateChanged(priorState, newState));
        }

        public static void InvokeKeyPressed(Keys key)
        {
            KeyPressed.Invoke(null, new EventArgsKeyPressed(key));
        }
    }
}
