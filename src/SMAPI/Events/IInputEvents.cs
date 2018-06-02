using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the player provides input using a controller, keyboard, or mouse.</summary>
    public interface IInputEvents
    {
        /// <summary>Raised when the player presses a button on the keyboard, controller, or mouse.</summary>
        event EventHandler<InputButtonPressedArgsInput> ButtonPressed;

        /// <summary>Raised when the player releases a button on the keyboard, controller, or mouse.</summary>
        event EventHandler<InputButtonReleasedArgsInput> ButtonReleased;
    }
}
