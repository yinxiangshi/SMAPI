using StardewModdingAPI;
using StardewValley;

namespace TrainerMod.Framework.Commands.Player
{
    /// <summary>A command which edits the player's name.</summary>
    internal class SetNameCommand : TrainerCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SetNameCommand()
            : base("player_setname", "Sets the player's name.\n\nUsage: player_setname <target> <name>\n- target: what to rename (one of 'player' or 'farm').\n- name: the new name to set.") { }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, string[] args)
        {
            // validate
            if (args.Length <= 1)
            {
                monitor.Log($"Your name is currently '{Game1.player.Name}'. Type 'help player_setname' for usage.", LogLevel.Info);
                return;
            }

            // handle
            string target = args[0];
            switch (target)
            {
                case "player":
                    Game1.player.Name = args[1];
                    monitor.Log($"OK, your player's name is now {Game1.player.Name}.", LogLevel.Info);
                    break;
                case "farm":
                    Game1.player.farmName = args[1];
                    monitor.Log($"OK, your farm's name is now {Game1.player.Name}.", LogLevel.Info);
                    break;
                default:
                    this.LogArgumentsInvalid(monitor, command);
                    break;
            }
        }
    }
}
