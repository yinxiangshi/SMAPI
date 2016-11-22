using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the player uses a controller, keyboard, or mouse.</summary>
    public static class ControlEvents
    {
        /*********
        ** Events
        *********/
        /// <summary>Raised when the <see cref="KeyboardState"/> changes. That happens when the player presses or releases a key.</summary>
        public static event EventHandler<EventArgsKeyboardStateChanged> KeyboardChanged;

        /// <summary>Raised when the player presses a keyboard key.</summary>
        public static event EventHandler<EventArgsKeyPressed> KeyPressed;

        /// <summary>Raised when the player releases a keyboard key.</summary>
        public static event EventHandler<EventArgsKeyPressed> KeyReleased;

        /// <summary>Raised when the <see cref="MouseState"/> changes. That happens when the player moves the mouse, scrolls the mouse wheel, or presses/releases a button.</summary>
        public static event EventHandler<EventArgsMouseStateChanged> MouseChanged;

        /// <summary>The player pressed a controller button. This event isn't raised for trigger buttons.</summary>
        public static event EventHandler<EventArgsControllerButtonPressed> ControllerButtonPressed;

        /// <summary>The player released a controller button. This event isn't raised for trigger buttons.</summary>
        public static event EventHandler<EventArgsControllerButtonReleased> ControllerButtonReleased;

        /// <summary>The player pressed a controller trigger button.</summary>
        public static event EventHandler<EventArgsControllerTriggerPressed> ControllerTriggerPressed;

        /// <summary>The player released a controller trigger button.</summary>
        public static event EventHandler<EventArgsControllerTriggerReleased> ControllerTriggerReleased;


        /*********
        ** Internal methods
        *********/
        /// <summary>Raise a <see cref="KeyboardChanged"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="priorState">The previous keyboard state.</param>
        /// <param name="newState">The current keyboard state.</param>
        internal static void InvokeKeyboardChanged(IMonitor monitor, KeyboardState priorState, KeyboardState newState)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(ControlEvents)}.{nameof(ControlEvents.KeyboardChanged)}", ControlEvents.KeyboardChanged?.GetInvocationList(), null, new EventArgsKeyboardStateChanged(priorState, newState));
        }

        /// <summary>Raise a <see cref="MouseChanged"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="priorState">The previous mouse state.</param>
        /// <param name="newState">The current mouse state.</param>
        /// <param name="priorPosition">The previous mouse position on the screen adjusted for the zoom level.</param>
        /// <param name="newPosition">The current mouse position on the screen adjusted for the zoom level.</param>
        internal static void InvokeMouseChanged(IMonitor monitor, MouseState priorState, MouseState newState, Point priorPosition, Point newPosition)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(ControlEvents)}.{nameof(ControlEvents.MouseChanged)}", ControlEvents.MouseChanged?.GetInvocationList(), null, new EventArgsMouseStateChanged(priorState, newState, priorPosition, newPosition));
        }

        /// <summary>Raise a <see cref="KeyPressed"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="key">The keyboard button that was pressed.</param>
        internal static void InvokeKeyPressed(IMonitor monitor, Keys key)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(ControlEvents)}.{nameof(ControlEvents.KeyPressed)}", ControlEvents.KeyPressed?.GetInvocationList(), null, new EventArgsKeyPressed(key));
        }

        /// <summary>Raise a <see cref="KeyReleased"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="key">The keyboard button that was released.</param>
        internal static void InvokeKeyReleased(IMonitor monitor, Keys key)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(ControlEvents)}.{nameof(ControlEvents.KeyReleased)}", ControlEvents.KeyReleased?.GetInvocationList(), null, new EventArgsKeyPressed(key));
        }

        /// <summary>Raise a <see cref="ControllerButtonPressed"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="playerIndex">The player who pressed the button.</param>
        /// <param name="button">The controller button that was pressed.</param>
        internal static void InvokeButtonPressed(IMonitor monitor, PlayerIndex playerIndex, Buttons button)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(ControlEvents)}.{nameof(ControlEvents.ControllerButtonPressed)}", ControlEvents.ControllerButtonPressed?.GetInvocationList(), null, new EventArgsControllerButtonPressed(playerIndex, button));
        }

        /// <summary>Raise a <see cref="ControllerButtonReleased"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="playerIndex">The player who released the button.</param>
        /// <param name="button">The controller button that was released.</param>
        internal static void InvokeButtonReleased(IMonitor monitor, PlayerIndex playerIndex, Buttons button)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(ControlEvents)}.{nameof(ControlEvents.ControllerButtonReleased)}", ControlEvents.ControllerButtonReleased?.GetInvocationList(), null, new EventArgsControllerButtonReleased(playerIndex, button));
        }

        /// <summary>Raise a <see cref="ControllerTriggerPressed"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="playerIndex">The player who pressed the trigger button.</param>
        /// <param name="button">The trigger button that was pressed.</param>
        /// <param name="value">The current trigger value.</param>
        internal static void InvokeTriggerPressed(IMonitor monitor, PlayerIndex playerIndex, Buttons button, float value)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(ControlEvents)}.{nameof(ControlEvents.ControllerTriggerPressed)}", ControlEvents.ControllerTriggerPressed?.GetInvocationList(), null, new EventArgsControllerTriggerPressed(playerIndex, button, value));
        }

        /// <summary>Raise a <see cref="ControllerTriggerReleased"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="playerIndex">The player who pressed the trigger button.</param>
        /// <param name="button">The trigger button that was pressed.</param>
        /// <param name="value">The current trigger value.</param>
        internal static void InvokeTriggerReleased(IMonitor monitor, PlayerIndex playerIndex, Buttons button, float value)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(ControlEvents)}.{nameof(ControlEvents.ControllerTriggerReleased)}", ControlEvents.ControllerTriggerReleased?.GetInvocationList(), null, new EventArgsControllerTriggerReleased(playerIndex, button, value));
        }
    }
}
