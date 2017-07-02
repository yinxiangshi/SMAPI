using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace TrainerMod.Framework.Commands.Player
{
    /// <summary>A command which adds a floor item to the player inventory.</summary>
    internal class AddFlooringCommand : TrainerCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public AddFlooringCommand()
            : base("player_addflooring", "Gives the player a flooring item.\n\nUsage: player_addflooring <flooring>\n- flooring: the flooring ID (ranges from 0 to 39).") { }

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
            if (!int.TryParse(args[0], out int floorID))
            {
                this.LogArgumentNotInt(monitor, command);
                return;
            }
            if (floorID < 0 || floorID > 39)
            {
                monitor.Log("There is no such flooring ID (must be between 0 and 39).", LogLevel.Error);
                return;
            }

            // handle
            Wallpaper wallpaper = new Wallpaper(floorID, isFloor: true);
            Game1.player.addItemByMenuIfNecessary(wallpaper);
            monitor.Log($"OK, added flooring {floorID} to your inventory.", LogLevel.Info);
        }
    }
}
