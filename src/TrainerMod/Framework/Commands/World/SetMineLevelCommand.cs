using System;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace TrainerMod.Framework.Commands.World
{
    /// <summary>A command which moves the player to the given mine level.</summary>
    internal class SetMineLevelCommand : TrainerCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SetMineLevelCommand()
            : base("world_setminelevel", "Sets the mine level?\n\nUsage: world_setminelevel <value>\n- value: The target level (a number starting at 1).") { }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, string[] args)
        {
            // validate
            if (!args.Any())
            {
                this.LogArgumentsInvalid(monitor, command);
                return;
            }
            if (!int.TryParse(args[0], out int level))
            {
                this.LogArgumentNotInt(monitor, command);
                return;
            }

            // handle
            level = Math.Max(1, level);
            monitor.Log($"OK, warping you to mine level {level}.", LogLevel.Info);
            Game1.enterMine(true, level, "");
        }
    }
}
