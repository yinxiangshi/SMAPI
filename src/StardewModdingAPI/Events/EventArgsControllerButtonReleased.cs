using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="ControlEvents.ControllerButtonReleased"/> event.</summary>
    public class EventArgsControllerButtonReleased : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The player who pressed the button.</summary>
        public PlayerIndex PlayerIndex { get; private set; }

        /// <summary>The controller button that was pressed.</summary>
        public Buttons ButtonReleased { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="playerIndex">The player who pressed the button.</param>
        /// <param name="button">The controller button that was released.</param>
        public EventArgsControllerButtonReleased(PlayerIndex playerIndex, Buttons button)
        {
            this.PlayerIndex = playerIndex;
            this.ButtonReleased = button;
        }
    }
}
