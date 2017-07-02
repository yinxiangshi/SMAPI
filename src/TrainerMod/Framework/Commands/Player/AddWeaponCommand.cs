using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace TrainerMod.Framework.Commands.Player
{
    /// <summary>A command which adds a weapon to the player inventory.</summary>
    internal class AddWeaponCommand : TrainerCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public AddWeaponCommand()
            : base("player_addweapon", "Gives the player a weapon.\n\nUsage: player_addweapon <item>\n- item: the weapon ID (use the 'list_items' command to see a list).") { }

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
            if (!int.TryParse(args[0], out int weaponID))
            {
                this.LogUsageError(monitor, "The weapon ID must be an integer.", command);
                return;
            }

            // get raw weapon data
            if (!Game1.content.Load<Dictionary<int, string>>("Data\\weapons").TryGetValue(weaponID, out string data))
            {
                monitor.Log("There is no such weapon ID.", LogLevel.Error);
                return;
            }

            // get raw weapon type
            int type;
            {
                string[] fields = data.Split('/');
                string typeStr = fields.Length > 8 ? fields[8] : null;
                if (!int.TryParse(typeStr, out type))
                {
                    monitor.Log("Could not parse the data for the weapon with that ID.", LogLevel.Error);
                    return;
                }
            }

            // get weapon
            Tool weapon;
            switch (type)
            {
                case MeleeWeapon.stabbingSword:
                case MeleeWeapon.dagger:
                case MeleeWeapon.club:
                case MeleeWeapon.defenseSword:
                    weapon = new MeleeWeapon(weaponID);
                    break;

                case 4:
                    weapon = new Slingshot(weaponID);
                    break;

                default:
                    monitor.Log($"The specified weapon has unknown type '{type}' in the game data.", LogLevel.Error);
                    return;
            }

            // validate weapon
            if (weapon.Name == null)
            {
                monitor.Log("That weapon doesn't seem to be valid.", LogLevel.Error);
                return;
            }

            // add weapon
            Game1.player.addItemByMenuIfNecessary(weapon);
            monitor.Log($"OK, added {weapon.Name} to your inventory.", LogLevel.Info);
        }
    }
}
