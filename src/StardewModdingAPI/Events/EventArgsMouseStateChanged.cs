using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="ControlEvents.MouseChanged"/> event.</summary>
    public class EventArgsMouseStateChanged : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The previous mouse state.</summary>
        public MouseState PriorState { get; private set; }

        /// <summary>The current mouse state.</summary>
        public MouseState NewState { get; private set; }

        /// <summary>The previous mouse position on the screen adjusted for the zoom level.</summary>
        public Point PriorPosition { get; private set; }

        /// <summary>The current mouse position on the screen adjusted for the zoom level.</summary>
        public Point NewPosition { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="priorState">The previous mouse state.</param>
        /// <param name="newState">The current mouse state.</param>
        /// <param name="priorPosition">The previous mouse position on the screen adjusted for the zoom level.</param>
        /// <param name="newPosition">The current mouse position on the screen adjusted for the zoom level.</param>
        public EventArgsMouseStateChanged(MouseState priorState, MouseState newState, Point priorPosition, Point newPosition)
        {
            this.PriorState = priorState;
            this.NewState = newState;
            this.PriorPosition = priorPosition;
            this.NewPosition = newPosition;
        }
    }
}
