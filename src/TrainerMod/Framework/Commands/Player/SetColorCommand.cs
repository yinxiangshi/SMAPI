using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace TrainerMod.Framework.Commands.Player
{
    /// <summary>A command which edits the color of a player feature.</summary>
    internal class SetColorCommand : TrainerCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SetColorCommand()
            : base("player_changecolor", "Sets the color of a player feature.\n\nUsage: player_changecolor <target> <color>\n- target: what to change (one of 'hair', 'eyes', or 'pants').\n- color: a color value in RGB format, like (255,255,255).") { }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, string[] args)
        {
            // validate
            if (args.Length <= 2)
            {
                this.LogArgumentsInvalid(monitor, command);
                return;
            }
            if (!this.TryParseColor(args[1], out Color color))
            {
                this.LogUsageError(monitor, "The color should be an RBG value like '255,150,0'.", command);
                return;
            }

            // handle
            switch (args[0])
            {
                case "hair":
                    Game1.player.hairstyleColor = color;
                    monitor.Log("OK, your hair color is updated.", LogLevel.Info);
                    break;

                case "eyes":
                    Game1.player.changeEyeColor(color);
                    monitor.Log("OK, your eye color is updated.", LogLevel.Info);
                    break;

                case "pants":
                    Game1.player.pantsColor = color;
                    monitor.Log("OK, your pants color is updated.", LogLevel.Info);
                    break;

                default:
                    this.LogArgumentsInvalid(monitor, command);
                    break;
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Try to parse a color from a string.</summary>
        /// <param name="input">The input string.</param>
        /// <param name="color">The color to set.</param>
        private bool TryParseColor(string input, out Color color)
        {
            string[] colorHexes = input.Split(new[] { ',' }, 3);
            if (int.TryParse(colorHexes[0], out int r) && int.TryParse(colorHexes[1], out int g) && int.TryParse(colorHexes[2], out int b))
            {
                color = new Color(r, g, b);
                return true;
            }

            color = Color.Transparent;
            return false;
        }
    }
}
