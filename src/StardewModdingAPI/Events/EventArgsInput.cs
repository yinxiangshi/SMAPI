#if SMAPI_2_0
using System;
using StardewModdingAPI.Utilities;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments when a button is pressed or released.</summary>
    public class EventArgsInput : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The button on the controller, keyboard, or mouse.</summary>
        public SButton Button { get; }

        /// <summary>The current cursor position.</summary>
        public ICursorPosition Cursor { get; set; }

        /// <summary>Whether the input is considered a 'click' by the game for enabling action.</summary>
        public bool IsClick { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="button">The button on the controller, keyboard, or mouse.</param>
        /// <param name="cursor">The cursor position.</param>
        /// <param name="isClick">Whether the input is considered a 'click' by the game for enabling action.</param>
        public EventArgsInput(SButton button, ICursorPosition cursor, bool isClick)
        {
            this.Button = button;
            this.Cursor = cursor;
            this.IsClick = isClick;
        }
    }
}
#endif
