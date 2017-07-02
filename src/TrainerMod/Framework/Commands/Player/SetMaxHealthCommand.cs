using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace TrainerMod.Framework.Commands.Player
{
    /// <summary>A command which edits the player's maximum health.</summary>
    internal class SetMaxHealthCommand : TrainerCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SetMaxHealthCommand()
            : base("player_setmaxhealth", "Sets the player's max health.\n\nUsage: player_setmaxhealth [value]\n- value: an integer amount.") { }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, string[] args)
        {
            // validate
            if (!args.Any())
            {
                monitor.Log($"You currently have {Game1.player.maxHealth} max health. Specify a value to change it.", LogLevel.Info);
                return;
            }

            // handle
            if (int.TryParse(args[0], out int maxHealth))
            {
                Game1.player.maxHealth = maxHealth;
                monitor.Log($"OK, you now have {Game1.player.maxHealth} max health.", LogLevel.Info);
            }
            else
                this.LogArgumentNotInt(monitor, command);
        }
    }
}
