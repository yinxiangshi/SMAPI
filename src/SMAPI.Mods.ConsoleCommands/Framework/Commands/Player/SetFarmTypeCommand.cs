using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace StardewModdingAPI.Mods.ConsoleCommands.Framework.Commands.Player
{
    /// <summary>A command which changes the player's farm type.</summary>
    internal class SetFarmTypeCommand : ConsoleCommand
    {
        /*********
        ** Fields
        *********/
        /// <summary>The vanilla farm type IDs.</summary>
        private static readonly ISet<int> VanillaFarmTypes = new HashSet<int>(
            Enumerable.Range(0, Farm.layout_max + 1)
        );


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SetFarmTypeCommand()
            : base("set_farm_type", $"Sets the current player's farm type.\n\nUsage: set_farm_type <farm type>\n- farm type: one of {string.Join(", ", SetFarmTypeCommand.VanillaFarmTypes.Select(id => $"{id} ({SetFarmTypeCommand.GetFarmLabel(id)})"))}.") { }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, ArgumentParser args)
        {
            // validation checks
            if (!Context.IsWorldReady)
            {
                monitor.Log("You must load a save to use this command.", LogLevel.Error);
                return;
            }

            // parse argument
            if (!args.TryGetInt(0, "farm type", out int farmType, min: 0, max: Farm.layout_max))
                return;

            // handle
            if (Game1.whichFarm == farmType)
            {
                monitor.Log($"Your current farm is already set to {farmType} ({SetFarmTypeCommand.GetFarmLabel(farmType)}).", LogLevel.Info);
                return;
            }

            this.SetFarmType(farmType);
            monitor.Log($"Your current farm has been converted to {farmType} ({SetFarmTypeCommand.GetFarmLabel(farmType)}).", LogLevel.Warn);
            monitor.Log("Saving and reloading is recommended to make sure everything is updated for the change.", LogLevel.Warn);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Change the farm type to the given value.</summary>
        /// <param name="type">The farm type ID.</param>
        private void SetFarmType(int type)
        {
            Game1.whichFarm = type;

            Farm farm = Game1.getFarm();
            farm.mapPath.Value = $@"Maps\{Farm.getMapNameFromTypeInt(Game1.whichFarm)}";
            farm.reloadMap();
        }

        /// <summary>Get the display name for a vanilla farm type.</summary>
        /// <param name="type">The farm type.</param>
        private static string GetFarmLabel(int type)
        {
            string translationKey = type switch
            {
                Farm.default_layout => "Character_FarmStandard",
                Farm.riverlands_layout => "Character_FarmFishing",
                Farm.forest_layout => "Character_FarmForaging",
                Farm.mountains_layout => "Character_FarmMining",
                Farm.combat_layout => "Character_FarmCombat",
                Farm.fourCorners_layout => "Character_FarmFourCorners",
                Farm.beach_layout => "Character_FarmBeach",
                _ => null
            };

            return translationKey != null
                ? Game1.content.LoadString(@$"Strings\UI:{translationKey}").Split('_')[0]
                : type.ToString();
        }
    }
}
