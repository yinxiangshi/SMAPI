using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the player uses a controller, keyboard, or mouse.</summary>
    public static class ControlEvents
    {
        /*********
        ** Events
        *********/
        /// <summary>Raised when the <see cref="KeyboardState"/> changes. That happens when the player presses or releases a key.</summary>
        public static event EventHandler<EventArgsKeyboardStateChanged> KeyboardChanged = delegate { };

        /// <summary>Raised when the player presses a keyboard key.</summary>
        public static event EventHandler<EventArgsKeyPressed> KeyPressed = delegate { };

        /// <summary>Raised when the player releases a keyboard key.</summary>
        public static event EventHandler<EventArgsKeyPressed> KeyReleased = delegate { };

        /// <summary>Raised when the <see cref="MouseState"/> changes. That happens when the player moves the mouse, scrolls the mouse wheel, or presses/releases a button.</summary>
        public static event EventHandler<EventArgsMouseStateChanged> MouseChanged = delegate { };

        /// <summary>The player pressed a controller button. This event isn't raised for trigger buttons.</summary>
        public static event EventHandler<EventArgsControllerButtonPressed> ControllerButtonPressed = delegate { };

        /// <summary>The player released a controller button. This event isn't raised for trigger buttons.</summary>
        public static event EventHandler<EventArgsControllerButtonReleased> ControllerButtonReleased = delegate { };

        /// <summary>The player pressed a controller trigger button.</summary>
        public static event EventHandler<EventArgsControllerTriggerPressed> ControllerTriggerPressed = delegate { };

        /// <summary>The player released a controller trigger button.</summary>
        public static event EventHandler<EventArgsControllerTriggerReleased> ControllerTriggerReleased = delegate { };


        /*********
        ** Internal methods
        *********/
        /// <summary>Raise a <see cref="KeyboardChanged"/> event.</summary>
        /// <param name="priorState">The previous keyboard state.</param>
        /// <param name="newState">The current keyboard state.</param>
        internal static void InvokeKeyboardChanged(KeyboardState priorState, KeyboardState newState)
        {
            ControlEvents.KeyboardChanged.Invoke(null, new EventArgsKeyboardStateChanged(priorState, newState));
        }

        /// <summary>Raise a <see cref="MouseChanged"/> event.</summary>
        /// <param name="priorState">The previous mouse state.</param>
        /// <param name="newState">The current mouse state.</param>
        internal static void InvokeMouseChanged(MouseState priorState, MouseState newState)
        {
            ControlEvents.MouseChanged.Invoke(null, new EventArgsMouseStateChanged(priorState, newState));
        }

        /// <summary>Raise a <see cref="KeyPressed"/> event.</summary>
        /// <param name="key">The keyboard button that was pressed.</param>
        internal static void InvokeKeyPressed(Keys key)
        {
            ControlEvents.KeyPressed.Invoke(null, new EventArgsKeyPressed(key));
        }

        /// <summary>Raise a <see cref="KeyReleased"/> event.</summary>
        /// <param name="key">The keyboard button that was released.</param>
        internal static void InvokeKeyReleased(Keys key)
        {
            ControlEvents.KeyReleased.Invoke(null, new EventArgsKeyPressed(key));
        }

        /// <summary>Raise a <see cref="ControllerButtonPressed"/> event.</summary>
        /// <param name="playerIndex">The player who pressed the button.</param>
        /// <param name="button">The controller button that was pressed.</param>
        internal static void InvokeButtonPressed(PlayerIndex playerIndex, Buttons button)
        {
            ControlEvents.ControllerButtonPressed.Invoke(null, new EventArgsControllerButtonPressed(playerIndex, button));
        }

        /// <summary>Raise a <see cref="ControllerButtonReleased"/> event.</summary>
        /// <param name="playerIndex">The player who released the button.</param>
        /// <param name="button">The controller button that was released.</param>
        internal static void InvokeButtonReleased(PlayerIndex playerIndex, Buttons button)
        {
            ControlEvents.ControllerButtonReleased.Invoke(null, new EventArgsControllerButtonReleased(playerIndex, button));
        }

        /// <summary>Raise a <see cref="ControllerTriggerPressed"/> event.</summary>
        /// <param name="playerIndex">The player who pressed the trigger button.</param>
        /// <param name="button">The trigger button that was pressed.</param>
        /// <param name="value">The current trigger value.</param>
        internal static void InvokeTriggerPressed(PlayerIndex playerIndex, Buttons button, float value)
        {
            ControlEvents.ControllerTriggerPressed.Invoke(null, new EventArgsControllerTriggerPressed(playerIndex, button, value));
        }

        /// <summary>Raise a <see cref="ControllerTriggerReleased"/> event.</summary>
        /// <param name="playerIndex">The player who pressed the trigger button.</param>
        /// <param name="button">The trigger button that was pressed.</param>
        /// <param name="value">The current trigger value.</param>
        internal static void InvokeTriggerReleased(PlayerIndex playerIndex, Buttons button, float value)
        {
            ControlEvents.ControllerTriggerReleased.Invoke(null, new EventArgsControllerTriggerReleased(playerIndex, button, value));
        }
    }
}
