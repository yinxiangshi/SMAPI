using StardewModdingAPI;
using StardewValley;

namespace TrainerMod.Framework.Commands.Player
{
    /// <summary>A command which adds an item to the player inventory.</summary>
    internal class AddItemCommand : TrainerCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public AddItemCommand()
            : base("player_additem", $"Gives the player an item.\n\nUsage: player_additem <item> [count] [quality]\n- item: the item ID (use the 'list_items' command to see a list).\n- count (optional): how many of the item to give.\n- quality (optional): one of {Object.lowQuality} (normal), {Object.medQuality} (silver), {Object.highQuality} (gold), or {Object.bestQuality} (iridium).") { }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, ArgumentParser args)
        {
            // read arguments
            if (!args.TryGetInt(0, "item ID", out int itemID, min: 0))
                return;
            if (!args.TryGetInt(1, "count", out int count, min: 1, required: false))
                count = 1;
            if (!args.TryGetInt(2, "quality", out int quality, min: Object.lowQuality, max: Object.bestQuality, required: false))
                quality = Object.lowQuality;

            // spawn item
            var item = new Object(itemID, count) { quality = quality };
            if (item.Name == "Error Item")
            {
                monitor.Log("There is no such item ID.", LogLevel.Error);
                return;
            }

            // add to inventory
            Game1.player.addItemByMenuIfNecessary(item);
            monitor.Log($"OK, added {item.Name} to your inventory.", LogLevel.Info);
        }
    }
}
