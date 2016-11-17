using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using TrainerMod.Framework;
using Object = StardewValley.Object;

namespace TrainerMod
{
    /// <summary>The main entry point for the mod.</summary>
    public class TrainerMod : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The time of day at which to freeze time.</summary>
        private int FrozenTime;

        /// <summary>Whether to keep the player's health at its maximum.</summary>
        private bool InfiniteHealth;

        /// <summary>Whether to keep the player's stamina at its maximum.</summary>
        private bool InfiniteStamina;

        /// <summary>Whether to keep the player's money at a set value.</summary>
        private bool InfiniteMoney;

        /// <summary>Whether to freeze time.</summary>
        private bool FreezeTime;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            this.RegisterCommands();
            GameEvents.UpdateTick += this.ReceiveUpdateTick;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Implementation
        ****/
        /// <summary>The method invoked when the game updates its state.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ReceiveUpdateTick(object sender, EventArgs e)
        {
            if (Game1.player == null)
                return;

            if (this.InfiniteHealth)
                Game1.player.health = Game1.player.maxHealth;
            if (this.InfiniteStamina)
                Game1.player.stamina = Game1.player.MaxStamina;
            if (this.InfiniteMoney)
                Game1.player.money = 999999;
            if (this.FreezeTime)
                Game1.timeOfDay = this.FrozenTime;
        }

        /// <summary>Register all trainer commands.</summary>
        private void RegisterCommands()
        {
            Command.RegisterCommand("types", "Lists all value types | types").CommandFired += this.HandleTypes;

            Command.RegisterCommand("save", "Saves the game? Doesn't seem to work. | save").CommandFired += this.HandleSave;
            Command.RegisterCommand("load", "Shows the load screen | load").CommandFired += this.HandleLoad;

            Command.RegisterCommand("exit", "Closes the game | exit").CommandFired += this.HandleExit;
            Command.RegisterCommand("stop", "Closes the game | stop").CommandFired += this.HandleExit;

            Command.RegisterCommand("player_setname", "Sets the player's name | player_setname <object> <value>", new[] { "(player, pet, farm)<object> (String)<value> The target name" }).CommandFired += this.HandlePlayerSetName;
            Command.RegisterCommand("player_setmoney", "Sets the player's money | player_setmoney <value>|inf", new[] { "(Int32)<value> The target money" }).CommandFired += this.HandlePlayerSetMoney;
            Command.RegisterCommand("player_setstamina", "Sets the player's stamina | player_setstamina <value>|inf", new[] { "(Int32)<value> The target stamina" }).CommandFired += this.HandlePlayerSetStamina;
            Command.RegisterCommand("player_setmaxstamina", "Sets the player's max stamina | player_setmaxstamina <value>", new[] { "(Int32)<value> The target max stamina" }).CommandFired += this.HandlePlayerSetMaxStamina;
            Command.RegisterCommand("player_sethealth", "Sets the player's health | player_sethealth <value>|inf", new[] { "(Int32)<value> The target health" }).CommandFired += this.HandlePlayerSetHealth;
            Command.RegisterCommand("player_setmaxhealth", "Sets the player's max health | player_setmaxhealth <value>", new[] { "(Int32)<value> The target max health" }).CommandFired += this.HandlePlayerSetMaxHealth;
            Command.RegisterCommand("player_setimmunity", "Sets the player's immunity | player_setimmunity <value>", new[] { "(Int32)<value> The target immunity" }).CommandFired += this.HandlePlayerSetImmunity;

            Command.RegisterCommand("player_setlevel", "Sets the player's specified skill to the specified value | player_setlevel <skill> <value>", new[] { "(luck, mining, combat, farming, fishing, foraging)<skill> (1-10)<value> The target level" }).CommandFired += this.HandlePlayerSetLevel;
            Command.RegisterCommand("player_setspeed", "Sets the player's speed to the specified value?", new[] { "(Int32)<value> The target speed [0 is normal]" }).CommandFired += this.HandlePlayerSetSpeed;
            Command.RegisterCommand("player_changecolour", "Sets the player's colour of the specified object | player_changecolor <object> <colour>", new[] { "(hair, eyes, pants)<object> (r,g,b)<colour>" }).CommandFired += this.HandlePlayerChangeColor;
            Command.RegisterCommand("player_changestyle", "Sets the player's style of the specified object | player_changecolor <object> <value>", new[] { "(hair, shirt, skin, acc, shoe, swim, gender)<object> (Int32)<value>" }).CommandFired += this.HandlePlayerChangeStyle;

            Command.RegisterCommand("player_additem", "Gives the player an item | player_additem <item> [count] [quality]", new[] { "(Int32)<id> (Int32)[count] (Int32)[quality]" }).CommandFired += this.HandlePlayerAddItem;
            Command.RegisterCommand("player_addmelee", "Gives the player a melee item | player_addmelee <item>", new[] { "?<item>" }).CommandFired += this.HandlePlayerAddMelee;
            Command.RegisterCommand("player_addring", "Gives the player a ring | player_addring <item>", new[] { "?<item>" }).CommandFired += this.HandlePlayerAddRing;

            Command.RegisterCommand("out_items", "Outputs a list of items | out_items", new[] { "" }).CommandFired += this.HandleOutItems;
            Command.RegisterCommand("out_melee", "Outputs a list of melee weapons | out_melee", new[] { "" }).CommandFired += this.HandleOutMelee;
            Command.RegisterCommand("out_rings", "Outputs a list of rings | out_rings", new[] { "" }).CommandFired += this.HandleOutRings;

            Command.RegisterCommand("world_settime", "Sets the time to the specified value | world_settime <value>", new[] { "(Int32)<value> The target time [06:00 AM is 600]" }).CommandFired += this.HandleWorldSetTime;
            Command.RegisterCommand("world_freezetime", "Freezes or thaws time | world_freezetime <value>", new[] { "(0 - 1)<value> Whether or not to freeze time. 0 is thawed, 1 is frozen" }).CommandFired += this.HandleWorldFreezeTime;
            Command.RegisterCommand("world_setday", "Sets the day to the specified value | world_setday <value>", new[] { "(Int32)<value> The target day [1-28]" }).CommandFired += this.world_setDay;
            Command.RegisterCommand("world_setseason", "Sets the season to the specified value | world_setseason <value>", new[] { "(winter, spring, summer, fall)<value> The target season" }).CommandFired += this.HandleWorldSetSeason;
            Command.RegisterCommand("world_downminelevel", "Goes down one mine level? | world_downminelevel", new[] { "" }).CommandFired += this.HandleWorldDownMineLevel;
            Command.RegisterCommand("world_setminelevel", "Sets mine level? | world_setminelevel", new[] { "(Int32)<value> The target level" }).CommandFired += this.HandleWorldSetMineLevel;
        }

        /****
        ** Command handlers
        ****/
        /// <summary>The event raised when the 'types' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleTypes(object sender, EventArgsCommand e)
        {
            this.Monitor.Log($"[Int32: {int.MinValue} - {int.MaxValue}], [Int64: {long.MinValue} - {long.MaxValue}], [String: \"raw text\"], [Colour: r,g,b (EG: 128, 32, 255)]", LogLevel.Info);
        }

        /// <summary>The event raised when the 'save' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleSave(object sender, EventArgsCommand e)
        {
            SaveGame.Save();
        }

        /// <summary>The event raised when the 'load' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleLoad(object sender, EventArgsCommand e)
        {
            Game1.hasLoadedGame = false;
            Game1.activeClickableMenu = new LoadGameMenu();
        }

        /// <summary>The event raised when the 'exit' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleExit(object sender, EventArgsCommand e)
        {
            Program.gamePtr.Exit();
            Environment.Exit(0);
        }

        /// <summary>The event raised when the 'player_setName' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandlePlayerSetName(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 1)
            {
                string target = e.Command.CalledArgs[0];
                string[] validTargets = { "player", "pet", "farm" };
                if (validTargets.Contains(target))
                {
                    switch (target)
                    {
                        case "player":
                            Game1.player.Name = e.Command.CalledArgs[1];
                            break;
                        case "pet":
                            this.Monitor.Log("Pets cannot currently be renamed.", LogLevel.Info);
                            break;
                        case "farm":
                            Game1.player.farmName = e.Command.CalledArgs[1];
                            break;
                    }
                }
                else
                    this.LogObjectInvalid();
            }
            else
                this.LogObjectValueNotSpecified();
        }

        /// <summary>The event raised when the 'player_setMoney' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandlePlayerSetMoney(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                string amountStr = e.Command.CalledArgs[0];
                if (amountStr == "inf")
                    this.InfiniteMoney = true;
                else
                {
                    this.InfiniteMoney = false;
                    int amount;
                    if (int.TryParse(amountStr, out amount))
                    {
                        Game1.player.Money = amount;
                        this.Monitor.Log($"Set {Game1.player.Name}'s money to {Game1.player.Money}", LogLevel.Info);
                    }
                    else
                        this.LogValueNotInt32();
                }
            }
            else
                this.LogValueNotSpecified();
        }

        /// <summary>The event raised when the 'player_setStamina' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandlePlayerSetStamina(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                string amountStr = e.Command.CalledArgs[0];
                if (amountStr == "inf")
                    this.InfiniteStamina = true;
                else
                {
                    this.InfiniteStamina = false;
                    int amount;
                    if (int.TryParse(amountStr, out amount))
                    {
                        Game1.player.Stamina = amount;
                        this.Monitor.Log($"Set {Game1.player.Name}'s stamina to {Game1.player.Stamina}", LogLevel.Info);
                    }
                    else
                        this.LogValueNotInt32();
                }
            }
            else
                this.LogValueNotSpecified();
        }

        /// <summary>The event raised when the 'player_setMaxStamina' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandlePlayerSetMaxStamina(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                int amount;
                if (int.TryParse(e.Command.CalledArgs[0], out amount))
                {
                    Game1.player.MaxStamina = amount;
                    this.Monitor.Log($"Set {Game1.player.Name}'s max stamina to {Game1.player.MaxStamina}", LogLevel.Info);
                }
                else
                    this.LogValueNotInt32();
            }
            else
                this.LogValueNotSpecified();
        }

        /// <summary>The event raised when the 'player_setLevel' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandlePlayerSetLevel(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 1)
            {
                string skill = e.Command.CalledArgs[0];
                string[] skills = { "luck", "mining", "combat", "farming", "fishing", "foraging" };
                if (skills.Contains(skill))
                {
                    int level;
                    if (int.TryParse(e.Command.CalledArgs[1], out level))
                    {
                        switch (skill)
                        {
                            case "luck":
                                Game1.player.LuckLevel = level;
                                break;
                            case "mining":
                                Game1.player.MiningLevel = level;
                                break;
                            case "combat":
                                Game1.player.CombatLevel = level;
                                break;
                            case "farming":
                                Game1.player.FarmingLevel = level;
                                break;
                            case "fishing":
                                Game1.player.FishingLevel = level;
                                break;
                            case "foraging":
                                Game1.player.ForagingLevel = level;
                                break;
                        }
                    }
                    else
                        this.LogValueNotInt32();
                }
                else
                    this.Monitor.Log("<skill> is invalid", LogLevel.Error);
            }
            else
                this.Monitor.Log("<skill> and <value> must be specified", LogLevel.Error);
        }

        /// <summary>The event raised when the 'player_setSpeed' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandlePlayerSetSpeed(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                string amountStr = e.Command.CalledArgs[0];
                if (amountStr.IsInt())
                {
                    Game1.player.addedSpeed = amountStr.ToInt();
                    this.Monitor.Log($"Set {Game1.player.Name}'s added speed to {Game1.player.addedSpeed}", LogLevel.Info);
                }
                else
                    this.LogValueNotInt32();
            }
            else
                this.LogValueNotSpecified();
        }

        /// <summary>The event raised when the 'player_changeColour' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandlePlayerChangeColor(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 1)
            {
                string target = e.Command.CalledArgs[0];
                string[] validTargets = { "hair", "eyes", "pants" };
                if (validTargets.Contains(target))
                {
                    string[] colorHexes = e.Command.CalledArgs[1].Split(new[] { ',' }, 3);
                    if (colorHexes[0].IsInt() && colorHexes[1].IsInt() && colorHexes[2].IsInt())
                    {
                        var color = new Color(colorHexes[0].ToInt(), colorHexes[1].ToInt(), colorHexes[2].ToInt());
                        switch (target)
                        {
                            case "hair":
                                Game1.player.hairstyleColor = color;
                                break;
                            case "eyes":
                                Game1.player.changeEyeColor(color);
                                break;
                            case "pants":
                                Game1.player.pantsColor = color;
                                break;
                        }
                    }
                    else
                        this.Monitor.Log("<colour> is invalid", LogLevel.Error);
                }
                else
                    this.LogObjectInvalid();
            }
            else
                this.Monitor.Log("<object> and <colour> must be specified", LogLevel.Error);
        }

        /// <summary>The event raised when the 'player_changeStyle' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandlePlayerChangeStyle(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 1)
            {
                string target = e.Command.CalledArgs[0];
                string[] validTargets = { "hair", "shirt", "skin", "acc", "shoe", "swim", "gender" };
                if (validTargets.Contains(target))
                {
                    if (e.Command.CalledArgs[1].IsInt())
                    {
                        var styleID = e.Command.CalledArgs[1].ToInt();
                        switch (target)
                        {
                            case "hair":
                                Game1.player.changeHairStyle(styleID);
                                break;
                            case "shirt":
                                Game1.player.changeShirt(styleID);
                                break;
                            case "acc":
                                Game1.player.changeAccessory(styleID);
                                break;
                            case "skin":
                                Game1.player.changeSkinColor(styleID);
                                break;
                            case "shoe":
                                Game1.player.changeShoeColor(styleID);
                                break;
                            case "swim":
                                if (styleID == 0)
                                    Game1.player.changeOutOfSwimSuit();
                                else if (styleID == 1)
                                    Game1.player.changeIntoSwimsuit();
                                else
                                    this.Monitor.Log("<value> must be 0 or 1 for this <object>", LogLevel.Error);
                                break;
                            case "gender":
                                if (styleID == 0)
                                    Game1.player.changeGender(true);
                                else if (styleID == 1)
                                    Game1.player.changeGender(false);
                                else
                                    this.Monitor.Log("<value> must be 0 or 1 for this <object>", LogLevel.Error);
                                break;
                        }
                    }
                    else
                        this.LogValueInvalid();
                }
                else
                    this.LogObjectInvalid();
            }
            else
                this.LogObjectValueNotSpecified();
        }

        /// <summary>The event raised when the 'world_freezeTime' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleWorldFreezeTime(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                string valueStr = e.Command.CalledArgs[0];
                if (valueStr.IsInt())
                {
                    int value = valueStr.ToInt();
                    if (value == 0 || value == 1)
                    {
                        this.FreezeTime = value == 1;
                        this.FrozenTime = this.FreezeTime ? Game1.timeOfDay : 0;
                        this.Monitor.Log("Time is now " + (this.FreezeTime ? "frozen" : "thawed"), LogLevel.Info);
                    }
                    else
                        this.Monitor.Log("<value> should be 0 or 1", LogLevel.Error);
                }
                else
                    this.LogValueNotInt32();
            }
            else
                this.LogValueNotSpecified();
        }

        /// <summary>The event raised when the 'world_setTime' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleWorldSetTime(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                string timeStr = e.Command.CalledArgs[0];
                if (timeStr.IsInt())
                {
                    int time = timeStr.ToInt();

                    if (time <= 2600 && time >= 600)
                    {
                        Game1.timeOfDay = e.Command.CalledArgs[0].ToInt();
                        this.FrozenTime = this.FreezeTime ? Game1.timeOfDay : 0;
                        this.Monitor.Log($"Time set to: {Game1.timeOfDay}", LogLevel.Info);
                    }
                    else
                        this.Monitor.Log("<value> should be between 600 and 2600 (06:00 AM - 02:00 AM [NEXT DAY])", LogLevel.Error);
                }
                else
                    this.LogValueNotInt32();
            }
            else
                this.LogValueNotSpecified();
        }

        /// <summary>The event raised when the 'world_setDay' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void world_setDay(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                string dayStr = e.Command.CalledArgs[0];

                if (dayStr.IsInt())
                {
                    int day = dayStr.ToInt();
                    if (day <= 28 && day > 0)
                        Game1.dayOfMonth = day;
                    else
                        this.Monitor.Log("<value> must be between 1 and 28", LogLevel.Error);
                }
                else
                    this.LogValueNotInt32();
            }
            else
                this.LogValueNotSpecified();
        }

        /// <summary>The event raised when the 'world_setSeason' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleWorldSetSeason(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                string season = e.Command.CalledArgs[0];
                string[] validSeasons = { "winter", "spring", "summer", "fall" };
                if (validSeasons.Contains(season))
                    Game1.currentSeason = season;
                else
                    this.LogValueInvalid();
            }
            else
                this.LogValueNotSpecified();
        }

        /// <summary>The event raised when the 'player_setHealth' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandlePlayerSetHealth(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                string amountStr = e.Command.CalledArgs[0];

                if (amountStr == "inf")
                    this.InfiniteHealth = true;
                else
                {
                    this.InfiniteHealth = false;
                    if (amountStr.IsInt())
                        Game1.player.health = amountStr.ToInt();
                    else
                        this.LogValueNotInt32();
                }
            }
            else
                this.LogValueNotSpecified();
        }

        /// <summary>The event raised when the 'player_setMaxHealth' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandlePlayerSetMaxHealth(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                string amountStr = e.Command.CalledArgs[0];
                if (amountStr.IsInt())
                    Game1.player.maxHealth = amountStr.ToInt();
                else
                    this.LogValueNotInt32();
            }
            else
                this.LogValueNotSpecified();
        }

        /// <summary>The event raised when the 'player_setImmunity' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandlePlayerSetImmunity(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                string amountStr = e.Command.CalledArgs[0];
                if (amountStr.IsInt())
                    Game1.player.immunity = amountStr.ToInt();
                else
                    this.LogValueNotInt32();
            }
            else
                this.LogValueNotSpecified();
        }

        /// <summary>The event raised when the 'player_addItem' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandlePlayerAddItem(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                string itemIdStr = e.Command.CalledArgs[0];
                if (itemIdStr.IsInt())
                {
                    int itemID = itemIdStr.ToInt();
                    int count = 1;
                    int quality = 0;
                    if (e.Command.CalledArgs.Length > 1)
                    {
                        if (e.Command.CalledArgs[1].IsInt())
                            count = e.Command.CalledArgs[1].ToInt();
                        else
                        {
                            this.Monitor.Log("[count] is invalid", LogLevel.Error);
                            return;
                        }

                        if (e.Command.CalledArgs.Length > 2)
                        {
                            if (e.Command.CalledArgs[2].IsInt())
                                quality = e.Command.CalledArgs[2].ToInt();
                            else
                            {
                                this.Monitor.Log("[quality] is invalid", LogLevel.Error);
                                return;
                            }
                        }
                    }

                    var item = new Object(itemID, count) { quality = quality };

                    Game1.player.addItemByMenuIfNecessary(item);
                }
                else
                    this.Monitor.Log("<item> is invalid", LogLevel.Error);
            }
            else
                this.LogObjectValueNotSpecified();
        }

        /// <summary>The event raised when the 'player_addMelee' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandlePlayerAddMelee(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0].IsInt())
                {
                    MeleeWeapon weapon = new MeleeWeapon(e.Command.CalledArgs[0].ToInt());
                    Game1.player.addItemByMenuIfNecessary(weapon);
                    this.Monitor.Log($"Gave {weapon.Name} to {Game1.player.Name}", LogLevel.Info);
                }
                else
                    this.Monitor.Log("<item> is invalid", LogLevel.Error);
            }
            else
                this.LogObjectValueNotSpecified();
        }

        /// <summary>The event raised when the 'player_addRing' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandlePlayerAddRing(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0].IsInt())
                {
                    Ring ring = new Ring(e.Command.CalledArgs[0].ToInt());
                    Game1.player.addItemByMenuIfNecessary(ring);
                    this.Monitor.Log($"Gave {ring.Name} to {Game1.player.Name}", LogLevel.Info);
                }
                else
                    this.Monitor.Log("<item> is invalid", LogLevel.Error);
            }
            else
                this.LogObjectValueNotSpecified();
        }

        /// <summary>The event raised when the 'out_items' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleOutItems(object sender, EventArgsCommand e)
        {
            for (var itemID = 0; itemID < 1000; itemID++)
            {
                try
                {
                    Item itemName = new Object(itemID, 1);
                    if (itemName.Name != "Error Item")
                        this.Monitor.Log($"{itemID} | {itemName.Name}", LogLevel.Info);
                }
                catch { }
            }
        }

        /// <summary>The event raised when the 'out_melee' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleOutMelee(object sender, EventArgsCommand e)
        {
            var data = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
            this.Monitor.Log("DATA\\WEAPONS: ", LogLevel.Info);
            foreach (var pair in data)
                this.Monitor.Log($"{pair.Key} | {pair.Value}", LogLevel.Info);
        }

        /// <summary>The event raised when the 'out_rings' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleOutRings(object sender, EventArgsCommand e)
        {
            for (var ringID = 0; ringID < 100; ringID++)
            {
                try
                {
                    Item item = new Ring(ringID);
                    if (item.Name != "Error Item")
                        this.Monitor.Log($"{ringID} | {item.Name}", LogLevel.Info);
                }
                catch { }
            }
        }

        /// <summary>The event raised when the 'world_downMineLevel' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleWorldDownMineLevel(object sender, EventArgsCommand e)
        {
            Game1.nextMineLevel();
        }

        /// <summary>The event raised when the 'world_setMineLevel' command is triggered.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleWorldSetMineLevel(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0].IsInt())
                    Game1.enterMine(true, e.Command.CalledArgs[0].ToInt(), "");
                else
                    this.LogValueNotInt32();
            }
            else
                this.LogValueNotSpecified();
        }

        /****
        ** Logging
        ****/
        /// <summary>Log an error indicating a value must be specified.</summary>
        public void LogValueNotSpecified()
        {
            this.Monitor.Log("<value> must be specified", LogLevel.Error);
        }

        /// <summary>Log an error indicating a target and value must be specified.</summary>
        public void LogObjectValueNotSpecified()
        {
            this.Monitor.Log("<object> and <value> must be specified", LogLevel.Error);
        }

        /// <summary>Log an error indicating a value is invalid.</summary>
        public void LogValueInvalid()
        {
            this.Monitor.Log("<value> is invalid", LogLevel.Error);
        }

        /// <summary>Log an error indicating a target is invalid.</summary>
        public void LogObjectInvalid()
        {
            this.Monitor.Log("<object> is invalid", LogLevel.Error);
        }

        /// <summary>Log an error indicating a value must be an integer.</summary>
        public void LogValueNotInt32()
        {
            this.Monitor.Log("<value> must be a whole number (Int32)", LogLevel.Error);
        }
    }
}
