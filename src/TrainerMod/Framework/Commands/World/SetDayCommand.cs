using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace TrainerMod.Framework.Commands.World
{
    /// <summary>A command which sets the current day.</summary>
    internal class SetDayCommand : TrainerCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SetDayCommand()
            : base("world_setday", "Sets the day to the specified value.\n\nUsage: world_setday <value>.\n- value: the target day (a number from 1 to 28).") { }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, string[] args)
        {
            // validate
            if (!args.Any())
            {
                monitor.Log($"The current date is {Game1.currentSeason} {Game1.dayOfMonth}. Specify a value to change the day.", LogLevel.Info);
                return;
            }
            if (!int.TryParse(args[0], out int day))
            {
                this.LogArgumentNotInt(monitor, command);
                return;
            }
            if (day > 28 || day <= 0)
            {
                this.LogUsageError(monitor, "That isn't a valid day.", command);
                return;
            }

            // handle
            Game1.dayOfMonth = day;
            monitor.Log($"OK, the date is now {Game1.currentSeason} {Game1.dayOfMonth}.", LogLevel.Info);
        }
    }
}
