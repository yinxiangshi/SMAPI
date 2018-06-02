using System;
using System.Collections.Generic;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments when a button is released.</summary>
    public class InputButtonReleasedArgsInput : EventArgs
    {
        /*********
        ** Properties
        *********/
        /// <summary>The buttons to suppress.</summary>
        private readonly HashSet<SButton> SuppressButtons;


        /*********
        ** Accessors
        *********/
        /// <summary>The button on the controller, keyboard, or mouse.</summary>
        public SButton Button { get; }

        /// <summary>The current cursor position.</summary>
        public ICursorPosition Cursor { get; }

        /// <summary>Whether a mod has indicated the key was already handled.</summary>
        public bool IsSuppressed => this.SuppressButtons.Contains(this.Button);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="button">The button on the controller, keyboard, or mouse.</param>
        /// <param name="cursor">The cursor position.</param>
        /// <param name="suppressButtons">The buttons to suppress.</param>
        public InputButtonReleasedArgsInput(SButton button, ICursorPosition cursor, HashSet<SButton> suppressButtons)
        {
            this.Button = button;
            this.Cursor = cursor;
            this.SuppressButtons = suppressButtons;
        }

        /// <summary>Prevent the game from handling the current button press. This doesn't prevent other mods from receiving the event.</summary>
        public void SuppressButton()
        {
            this.SuppressButton(this.Button);
        }

        /// <summary>Prevent the game from handling a button press. This doesn't prevent other mods from receiving the event.</summary>
        /// <param name="button">The button to suppress.</param>
        public void SuppressButton(SButton button)
        {
            this.SuppressButtons.Add(button);
        }
    }
}
