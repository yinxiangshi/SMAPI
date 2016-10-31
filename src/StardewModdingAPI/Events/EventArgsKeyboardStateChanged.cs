using System;
using Microsoft.Xna.Framework.Input;

namespace StardewModdingAPI.Events
{
    public class EventArgsKeyboardStateChanged : EventArgs
    {
        public EventArgsKeyboardStateChanged(KeyboardState priorState, KeyboardState newState)
        {
            NewState = newState;
            NewState = newState;
        }

        public KeyboardState NewState { get; private set; }
        public KeyboardState PriorState { get; private set; }
    }
}