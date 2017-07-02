using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace TrainerMod.Framework.Commands.World
{
    /// <summary>A command which sets the current time.</summary>
    internal class SetTimeCommand : TrainerCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SetTimeCommand()
            : base("world_settime", "Sets the time to the specified value.\n\nUsage: world_settime <value>\n- value: the target time in military time (like 0600 for 6am and 1800 for 6pm).") { }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, string[] args)
        {
            // validate
            if (!args.Any())
            {
                monitor.Log($"The current time is {Game1.timeOfDay}. Specify a value to change it.", LogLevel.Info);
                return;
            }
            if (!int.TryParse(args[0], out int time))
            {
                this.LogArgumentNotInt(monitor, command);
                return;
            }
            if (time > 2600 || time < 600)
            {
                this.LogUsageError(monitor, "That isn't a valid time.", command);
                return;
            }

            // handle
            Game1.timeOfDay = time;
            FreezeTimeCommand.FrozenTime = Game1.timeOfDay;
            monitor.Log($"OK, the time is now {Game1.timeOfDay.ToString().PadLeft(4, '0')}.", LogLevel.Info);
        }
    }
}
