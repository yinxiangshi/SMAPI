#if !SMAPI_2_0
using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="StardewModdingAPI.Command.CommandFired"/> event.</summary>
    [Obsolete("Use " + nameof(IModHelper) + "." + nameof(IModHelper.ConsoleCommands))]
    public class EventArgsCommand : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The triggered command.</summary>
        public Command Command { get; }


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
#endif