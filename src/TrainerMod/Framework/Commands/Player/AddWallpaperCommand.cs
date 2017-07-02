using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace TrainerMod.Framework.Commands.Player
{
    /// <summary>A command which adds a wallpaper item to the player inventory.</summary>
    internal class AddWallpaperCommand : TrainerCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public AddWallpaperCommand()
            : base("player_addwallpaper", "Gives the player a wallpaper.\n\nUsage: player_addwallpaper <wallpaper>\n- wallpaper: the wallpaper ID (ranges from 0 to 111).") { }

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
            if (!int.TryParse(args[0], out int wallpaperID))
            {
                this.LogArgumentNotInt(monitor, command);
                return;
            }
            if (wallpaperID < 0 || wallpaperID > 111)
            {
                monitor.Log("There is no such wallpaper ID (must be between 0 and 111).", LogLevel.Error);
                return;
            }

            // handle
            Wallpaper wallpaper = new Wallpaper(wallpaperID);
            Game1.player.addItemByMenuIfNecessary(wallpaper);
            monitor.Log($"OK, added wallpaper {wallpaperID} to your inventory.", LogLevel.Info);
        }
    }
}
