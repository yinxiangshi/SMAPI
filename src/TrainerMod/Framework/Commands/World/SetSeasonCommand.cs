using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace TrainerMod.Framework.Commands.World
{
    /// <summary>A command which sets the current season.</summary>
    internal class SetSeasonCommand : TrainerCommand
    {
        /*********
        ** Properties
        *********/
        /// <summary>The valid season names.</summary>
        private readonly string[] ValidSeasons = { "winter", "spring", "summer", "fall" };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SetSeasonCommand()
            : base("world_setseason", "Sets the season to the specified value.\n\nUsage: world_setseason <season>\n- season: the target season (one of 'spring', 'summer', 'fall', 'winter').") { }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, string[] args)
        {
            // validate
            if (!args.Any())
            {
                monitor.Log($"The current season is {Game1.currentSeason}. Specify a value to change it.", LogLevel.Info);
                return;
            }
            if (!this.ValidSeasons.Contains(args[0]))
            {
                this.LogUsageError(monitor, "That isn't a valid season name.", command);
                return;
            }

            // handle
            Game1.currentSeason = args[0];
            monitor.Log($"OK, the date is now {Game1.currentSeason} {Game1.dayOfMonth}.", LogLevel.Info);
        }
    }
}
