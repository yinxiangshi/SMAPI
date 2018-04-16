using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Mods.ConsoleCommands.Framework.ItemData;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewModdingAPI.Mods.ConsoleCommands.Framework.Commands.Player
{
    /// <summary>A command which adds an item to the player inventory.</summary>
    internal class AddCommand : TrainerCommand
    {
        /*********
        ** Properties
        *********/
        /// <summary>Provides methods for searching and constructing items.</summary>
        private readonly ItemRepository Items = new ItemRepository();

        private readonly string[] ItemTypeAndName = Enum.GetNames(typeof(ItemType)).Union(new string[] { "Name" }).ToArray();

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public AddCommand()
            : base("player_add", AddCommand.GetDescription())
        { }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, ArgumentParser args)
        {
            SearchableItem match;

            //read arguments
            if (!args.TryGet(0, "item type", out string typeOrName, oneOf: this.ItemTypeAndName))
                return;
            if (Enum.GetNames(typeof(ItemType)).Contains(typeOrName, StringComparer.InvariantCultureIgnoreCase))
                this.FindItemByTypeAndId(monitor, args, typeOrName, out match);
            else
                this.FindItemByName(monitor, args, out match);

            if (match == null)
                return;

            if (!args.TryGetInt(2, "count", out int count, min: 1, required: false))
                count = 1;
            if (!args.TryGetInt(3, "quality", out int quality, min: Object.lowQuality, max: Object.bestQuality, required: false))
                quality = Object.lowQuality;

            // apply count
            match.Item.Stack = count;

            // apply quality
            if (match.Item is Object obj)
                obj.Quality = quality;
            else if (match.Item is Tool tool)
                tool.UpgradeLevel = quality;

            // add to inventory
            Game1.player.addItemByMenuIfNecessary(match.Item);
            monitor.Log($"OK, added {match.Name} ({match.Type} #{match.ID}) to your inventory.", LogLevel.Info);
        }

        /*********
        ** Private methods
        *********/

        /// <summary>
        /// Finds a matching item by item type and id.
        /// </summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="args">The command arguments.</param>
        /// <param name="rawType">The raw item type.</param>
        /// <param name="match">The matching item.</param>
        private void FindItemByTypeAndId(IMonitor monitor, ArgumentParser args, string rawType, out SearchableItem match)
        {
            match = null;

            // read arguments
            if (!args.TryGetInt(1, "item ID", out int id, min: 0))
                return;

            ItemType type = (ItemType)Enum.Parse(typeof(ItemType), rawType, ignoreCase: true);

            // find matching item
            match = this.Items.GetAll().FirstOrDefault(p => p.Type == type && p.ID == id);

            if (match == null)
            {
                monitor.Log($"There's no {type} item with ID {id}.", LogLevel.Error);
            }
        }

        /// <summary>
        /// Finds a matching item by name.
        /// </summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="args">The command arguments.</param>
        /// <param name="name">The item name.</param>
        /// <param name="match">The matching item.</param>
        private void FindItemByName(IMonitor monitor, ArgumentParser args, out SearchableItem match)
        {
            match = null;

            // read arguments
            if (!args.TryGet(1, "item name", out string name))
                return;

            // find matching items
            IEnumerable<SearchableItem> matching = this.Items.GetAll().Where(p => p.DisplayName.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) != -1);
            int numberOfMatches = matching.Count();

            // handle unique requirement
            if (numberOfMatches == 0)
            {
                monitor.Log($"There's no item with name {name}.", LogLevel.Error);
            }
            else if (numberOfMatches == 1)
            {
                match = matching.ElementAt(0);
            }
            else
            {
                string options = this.GetTableString(matching, new string[] { "type", "name", "command" }, item => new string[] { item.Type.ToString(), item.DisplayName, $"player_add {item.Type} {item.ID}" });

                monitor.Log($"Found multiple item names containing '{name}'. Type one of these commands for the one you want:", LogLevel.Error);
                monitor.Log($"\n{options}", LogLevel.Info);
            }
        }

        private static string GetDescription()
        {
            string[] typeValues = Enum.GetNames(typeof(ItemType));
            return "Gives the player an item.\n"
                + "\n"
                + "Usage: player_add <type> (<item>|<name>) [count] [quality]\n"
                + $"- type: the item type (either Name or one of {string.Join(", ", typeValues)}).\n"
                + "- item: the item ID (use the 'list_items' command to see a list).\n"
                + "- name: the display name of the item (when using type Name).\n"
                + "- count (optional): how many of the item to give.\n"
                + $"- quality (optional): one of {Object.lowQuality} (normal), {Object.medQuality} (silver), {Object.highQuality} (gold), or {Object.bestQuality} (iridium).\n"
                + "\n"
                + "This example adds the galaxy sword to your inventory:\n"
                + "  player_add weapon 4";
        }
    }
}
