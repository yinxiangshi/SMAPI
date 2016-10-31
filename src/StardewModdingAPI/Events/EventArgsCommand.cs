using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="StardewModdingAPI.Command.CommandFired"/> event.</summary>
    public class EventArgsCommand : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The triggered command.</summary>
        public Command Command { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="command">The triggered command.</param>
        public EventArgsCommand(Command command)
        {
            this.Command = command;
        }
    }
}
