using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace TrainerMod.Framework.Commands.Player
{
    /// <summary>A command which adds a ring to the player inventory.</summary>
    internal class AddRingCommand : TrainerCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public AddRingCommand()
            : base("player_addring", "Gives the player a ring.\n\nUsage: player_addring <item>\n- item: the ring ID (use the 'list_items' command to see a list).") { }

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
            if (!int.TryParse(args[0], out int ringID))
            {
                monitor.Log("<item> is invalid", LogLevel.Error);
                return;
            }
            if (ringID < Ring.ringLowerIndexRange || ringID > Ring.ringUpperIndexRange)
            {
                monitor.Log($"There is no such ring ID (must be between {Ring.ringLowerIndexRange} and {Ring.ringUpperIndexRange}).", LogLevel.Error);
                return;
            }

            // handle
            Ring ring = new Ring(ringID);
            Game1.player.addItemByMenuIfNecessary(ring);
            monitor.Log($"OK, added {ring.Name} to your inventory.", LogLevel.Info);
        }
    }
}
