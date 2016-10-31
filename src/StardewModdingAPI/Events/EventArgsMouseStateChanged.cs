using System;
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
        public MouseState NewState { get; private set; }

        /// <summary>The current mouse state.</summary>
        public MouseState PriorState { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="priorState">The previous mouse state.</param>
        /// <param name="newState">The current mouse state.</param>
        public EventArgsMouseStateChanged(MouseState priorState, MouseState newState)
        {
            this.PriorState = priorState;
            this.NewState = newState;
        }
    }
}
