using System;
using StardewModdingAPI.Events;

namespace StardewModdingAPI.Framework.Events
{
    /// <summary>Events raised when the player provides input using a controller, keyboard, or mouse.</summary>
    internal class ModInputEvents : ModEventsBase, IInputEvents
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        public event EventHandler<InputButtonPressedArgsInput> ButtonPressed
        {
            add => this.EventManager.Input_ButtonPressed.Add(value);
            remove => this.EventManager.Input_ButtonPressed.Remove(value);
        }

        /// <summary>Raised after the player releases a button on the keyboard, controller, or mouse.</summary>
        public event EventHandler<InputButtonReleasedArgsInput> ButtonReleased
        {
            add => this.EventManager.Input_ButtonReleased.Add(value);
            remove => this.EventManager.Input_ButtonReleased.Remove(value);
        }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        public event EventHandler<InputCursorMovedArgsInput> CursorMoved
        {
            add => this.EventManager.Input_CursorMoved.Add(value);
            remove => this.EventManager.Input_CursorMoved.Remove(value);
        }

        /// <summary>Raised after the player scrolls the mouse wheel.</summary>
        public event EventHandler<InputMouseWheelScrolledEventArgs> MouseWheelScrolled
        {
            add => this.EventManager.Input_MouseWheelScrolled.Add(value);
            remove => this.EventManager.Input_MouseWheelScrolled.Remove(value);
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The mod which uses this instance.</param>
        /// <param name="eventManager">The underlying event manager.</param>
        internal ModInputEvents(IModMetadata mod, EventManager eventManager)
            : base(mod, eventManager) { }
    }
}
