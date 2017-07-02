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
        public override void Handle(IMonitor monitor, string command, ArgumentParser args)
        {
            // parse arguments
            if (!args.TryGetInt(0, "wallpaper ID", out int wallpaperID, min: 0, max: 111))
                return;

            // handle
            Wallpaper wallpaper = new Wallpaper(wallpaperID);
            Game1.player.addItemByMenuIfNecessary(wallpaper);
            monitor.Log($"OK, added wallpaper {wallpaperID} to your inventory.", LogLevel.Info);
        }
    }
}
