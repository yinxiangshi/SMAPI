using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace TrainerMod.Framework.Commands.Player
{
    /// <summary>A command which edits the player's current added speed.</summary>
    internal class SetSpeedCommand : TrainerCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SetSpeedCommand()
            : base("player_setspeed", "Sets the player's added speed to the specified value.\n\nUsage: player_setspeed <value>\n- value: an integer amount (0 is normal).") { }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, string[] args)
        {
            // validate
            if (!args.Any())
            {
                monitor.Log($"You currently have {Game1.player.addedSpeed} added speed. Specify a value to change it.", LogLevel.Info);
                return;
            }
            if (!int.TryParse(args[0], out int addedSpeed))
            {
                this.LogArgumentNotInt(monitor, command);
                return;
            }

            // handle
            Game1.player.addedSpeed = addedSpeed;
            monitor.Log($"OK, your added speed is now {Game1.player.addedSpeed}.", LogLevel.Info);
        }
    }
}
