using System;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the player uses a controller, keyboard, or mouse button.</summary>
    public static class InputEvents
    {
        /*********
        ** Events
        *********/
        /// <summary>Raised when the player presses a button on the keyboard, controller, or mouse.</summary>
        public static event EventHandler<EventArgsInput> ButtonPressed;

        /// <summary>Raised when the player releases a keyboard key on the keyboard, controller, or mouse.</summary>
        public static event EventHandler<EventArgsInput> ButtonReleased;


        /*********
        ** Internal methods
        *********/
        /// <summary>Raise a <see cref="ButtonPressed"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="button">The button on the controller, keyboard, or mouse.</param>
        /// <param name="cursor">The cursor position.</param>
        /// <param name="isActionButton">Whether the input should trigger actions on the affected tile.</param>
        /// <param name="isUseToolButton">Whether the input should use tools on the affected tile.</param>
        internal static void InvokeButtonPressed(IMonitor monitor, SButton button, ICursorPosition cursor, bool isActionButton, bool isUseToolButton)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(InputEvents)}.{nameof(InputEvents.ButtonPressed)}", InputEvents.ButtonPressed?.GetInvocationList(), null, new EventArgsInput(button, cursor, isActionButton, isUseToolButton));
        }

        /// <summary>Raise a <see cref="ButtonReleased"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="button">The button on the controller, keyboard, or mouse.</param>
        /// <param name="cursor">The cursor position.</param>
        /// <param name="isActionButton">Whether the input should trigger actions on the affected tile.</param>
        /// <param name="isUseToolButton">Whether the input should use tools on the affected tile.</param>
        internal static void InvokeButtonReleased(IMonitor monitor, SButton button, ICursorPosition cursor, bool isActionButton, bool isUseToolButton)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(InputEvents)}.{nameof(InputEvents.ButtonReleased)}", InputEvents.ButtonReleased?.GetInvocationList(), null, new EventArgsInput(button, cursor, isActionButton, isUseToolButton));
        }
    }
}
