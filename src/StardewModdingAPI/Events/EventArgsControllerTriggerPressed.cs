using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="ControlEvents.ControllerTriggerPressed"/> event.</summary>
    public class EventArgsControllerTriggerPressed : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The player who pressed the button.</summary>
        public PlayerIndex PlayerIndex { get; private set; }

        /// <summary>The controller button that was pressed.</summary>
        public Buttons ButtonPressed { get; private set; }

        /// <summary>The current trigger value.</summary>
        public float Value { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="playerIndex">The player who pressed the trigger button.</param>
        /// <param name="button">The trigger button that was pressed.</param>
        /// <param name="value">The current trigger value.</param>
        public EventArgsControllerTriggerPressed(PlayerIndex playerIndex, Buttons button, float value)
        {
            this.PlayerIndex = playerIndex;
            this.ButtonPressed = button;
            this.Value = value;
        }
    }
}
