using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Inheritance;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Network;
using StardewValley.Tools;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Object = StardewValley.Object;

namespace StardewModdingAPI
{
    public class Program
    {
        public static string ExecutionPath { get; private set; }
        public static string DataPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"));
        public static string ModPath = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley")), "Mods");

        public static SGame gamePtr;
        public static bool ready;

        public static Assembly StardewAssembly;
        public static Type StardewProgramType;
        public static FieldInfo StardewGameInfo;
        public static Form StardewForm;

        public static Thread gameThread;
        public static Thread consoleInputThread;

        public static int frozenTime;
        public static bool infHealth, infStamina, infMoney, freezeTime;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void Main(string[] args)
        {
            Console.Title = "Stardew Modding API Console";

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            if (File.Exists(ModPath))
                File.Delete(ModPath);
            if (!Directory.Exists(ModPath))
                Directory.CreateDirectory(ModPath);

            ExecutionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Log(ExecutionPath);
            LogInfo("Initializing SDV Assembly...");
            StardewAssembly = Assembly.LoadFile(ExecutionPath + "\\Stardew Valley.exe");//AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name.Equals("Stardew Valley"));
            StardewProgramType = StardewAssembly.GetType("StardewValley.Program", true);
            StardewGameInfo = StardewProgramType.GetField("gamePtr");

            LogInfo("Injecting New SDV Version...");
            Game1.version += "-Z_MODDED";

            gameThread = new Thread(RunGame);
            LogInfo("Starting SDV...");
            gameThread.Start();

            SGame.Thing();

            while (!ready)
            {

            }

            Log("SDV Loaded Into Memory");

            consoleInputThread = new Thread(ConsoleInputThread);
            LogInfo("Initializing Console Input Thread...");
            consoleInputThread.Start();

            Events.KeyPressed += Events_KeyPressed;
            Events.UpdateTick += Events_UpdateTick;

            LogInfo("Applying Final SDV Tweaks...");
            StardewInvoke(() =>
            {
                                    gamePtr.IsMouseVisible = false;
                                    gamePtr.Window.Title = "Stardew Valley";
            });

            LogInfo("Game Loaded");
            LogColour(ConsoleColor.Cyan, "Type 'help' for help, or 'help <cmd>' for a command's usage");
            Events.InvokeGameLoaded();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        public static void RunGame()
        {
            try
            {
                gamePtr = new SGame();
                Game1.graphics.GraphicsProfile = GraphicsProfile.HiDef;
                LoadMods();

                StardewForm = Control.FromHandle(Program.gamePtr.Window.Handle).FindForm();
                StardewGameInfo.SetValue(StardewProgramType, gamePtr);

                ready = true;

                gamePtr.Run();
            }
            catch (Exception ex)
            {
                LogError("Game failed to start: " + ex);
            }
            ready = false;
            if (consoleInputThread != null && consoleInputThread.ThreadState == ThreadState.Running)
                consoleInputThread.Abort();
            Log("Game Execution Finished");
            Console.ReadKey();
            Environment.Exit(0);
        }

        public static void LoadMods()
        {
            LogColour(ConsoleColor.Green, "LOADING MODS");
            foreach (String s in Directory.GetFiles(ModPath, "*.dll"))
            {
                LogColour(ConsoleColor.Green, "Found DLL: " + s);
                Assembly mod = Assembly.LoadFile(s);

                if (mod.DefinedTypes.Count(x => x.BaseType == typeof (Mod)) > 0)
                {
                    LogColour(ConsoleColor.Green, "Loading Mod DLL...");
                    TypeInfo tar = mod.DefinedTypes.First(x => x.BaseType == typeof(Mod));
                    Mod m = (Mod)mod.CreateInstance(tar.ToString());
                    Console.WriteLine("LOADED MOD: {0} by {1} - Version {2} | Description: {3}", m.Name, m.Authour, m.Version, m.Description);
                    m.Entry();
                }
                else
                {
                    LogError("Invalid Mod DLL");
                }
            }
        }

        public static void ConsoleInputThread()
        {
            string input = string.Empty;

            RegisterCommands();

            while (true)
            {
                Command.CallCommand(Console.ReadLine());
            }
        }

        static void Events_KeyPressed(Keys key)
        {
            
        }

        static void Events_UpdateTick()
        {
            if (infHealth)
            {
                Game1.player.health = Game1.player.maxHealth;
            }
            if (infStamina)
            {
                Game1.player.stamina = Game1.player.MaxStamina;
            }
            if (infMoney)
            {
                Game1.player.money = 999999;
            }
            if (freezeTime)
            {
                Game1.timeOfDay = frozenTime;
            }
        }

        public static void StardewInvoke(Action a)
        {
            StardewForm.Invoke(a);
        }

        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(',') ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");

            dllName = dllName.Replace(".", "_");

            if (dllName.EndsWith("_resources")) return null;

            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(typeof(Program).Namespace + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());

            byte[] bytes = (byte[])rm.GetObject(dllName);

            return System.Reflection.Assembly.Load(bytes);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        #region Commands

        public static void RegisterCommands()
        {
            Command.RegisterCommand("help", "Lists all commands | 'help <cmd>' returns command description").CommandFired += help_CommandFired;
            Command.RegisterCommand("types", "Lists all value types | types").CommandFired += types_CommandFired;

            Command.RegisterCommand("hide", "Hides the game form | hide").CommandFired += hide_CommandFired;
            Command.RegisterCommand("show", "Shows the game form | show").CommandFired += show_CommandFired;

            Command.RegisterCommand("save", "Saves the game? Doesn't seem to work. | save").CommandFired += save_CommandFired;

            Command.RegisterCommand("exit", "Closes the game | exit").CommandFired += exit_CommandFired;
            Command.RegisterCommand("stop", "Closes the game | stop").CommandFired += exit_CommandFired;

            Command.RegisterCommand("player_setname", "Sets the player's name | player_setname <object> <value>", new[] { "(player, pet, farm)<object> (String)<value> The target name" }).CommandFired += player_setName;
            Command.RegisterCommand("player_setmoney", "Sets the player's money | player_setmoney <value>|inf", new[] { "(Int32)<value> The target money" }).CommandFired += player_setMoney;
            Command.RegisterCommand("player_setstamina", "Sets the player's stamina | player_setstamina <value>|inf", new[] { "(Int32)<value> The target stamina" }).CommandFired += player_setStamina;
            Command.RegisterCommand("player_setmaxstamina", "Sets the player's max stamina | player_setmaxstamina <value>", new[] { "(Int32)<value> The target max stamina" }).CommandFired += player_setMaxStamina;
            Command.RegisterCommand("player_sethealth", "Sets the player's health | player_sethealth <value>|inf", new[] { "(Int32)<value> The target health" }).CommandFired += player_setHealth;
            Command.RegisterCommand("player_setmaxhealth", "Sets the player's max health | player_setmaxhealth <value>", new[] { "(Int32)<value> The target max health" }).CommandFired += player_setMaxHealth;
            Command.RegisterCommand("player_setimmunity", "Sets the player's immunity | player_setimmunity <value>", new[] { "(Int32)<value> The target immunity" }).CommandFired += player_setImmunity;

            Command.RegisterCommand("player_setlevel", "Sets the player's specified skill to the specified value | player_setlevel <skill> <value>", new[] { "(luck, mining, combat, farming, fishing, foraging)<skill> (1-10)<value> The target level" }).CommandFired += player_setLevel;
            Command.RegisterCommand("player_setspeed", "Sets the player's speed to the specified value?", new[] {"(Int32)<value> The target speed [0 is normal]"}).CommandFired += player_setSpeed;
            Command.RegisterCommand("player_changecolour", "Sets the player's colour of the specified object | player_changecolor <object> <colour>", new[] { "(hair, eyes, pants)<object> (r,g,b)<colour>" }).CommandFired += player_changeColour;
            Command.RegisterCommand("player_changestyle", "Sets the player's style of the specified object | player_changecolor <object> <value>", new[] { "(hair, shirt, skin, acc, shoe, swim, gender)<object> (Int32)<value>" }).CommandFired += player_changeStyle;

            Command.RegisterCommand("player_additem", "Gives the player an item | player_additem <item> <count>", new[] { "?<item> (Int32)<count>" }).CommandFired += player_addItem;
            Command.RegisterCommand("player_addmelee", "Gives the player a melee item | player_addmelee <item>", new[] { "?<item>" }).CommandFired += player_addMelee;

            Command.RegisterCommand("out_items", "Outputs a list of items | out_items", new[] { "" }).CommandFired += out_items;
            Command.RegisterCommand("out_melee", "Outputs a list of melee weapons | out_melee", new[] { "" }).CommandFired += out_melee;

            Command.RegisterCommand("world_settime", "Sets the time to the specified value | world_settime <value>", new[] { "(Int32)<value> The target time [06:00 AM is 600]" }).CommandFired += world_setTime;
            Command.RegisterCommand("world_freezetime", "Freezes or thaws time | world_freezetime <value>", new[] { "(0 - 1)<value> Whether or not to freeze time. 0 is thawed, 1 is frozen" }).CommandFired += world_freezeTime;
            Command.RegisterCommand("world_setday", "Sets the day to the specified value | world_setday <value>", new[] { "(Int32)<value> The target day [1-28]" }).CommandFired += world_setDay;
            Command.RegisterCommand("world_setseason", "Sets the season to the specified value | world_setseason <value>", new[] { "(winter, spring, summer, fall)<value> The target season" }).CommandFired += world_setSeason;
            Command.RegisterCommand("world_downminelevel", "Goes down one mine level? | world_downminelevel", new[] { "" }).CommandFired += world_downMineLevel;
            Command.RegisterCommand("world_setminelevel", "Sets mine level? | world_setminelevel", new[] { "(Int32)<value> The target level" }).CommandFired += world_setMineLevel;
        }

        static void help_CommandFired(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                Command fnd = Command.FindCommand(cmd.CalledArgs[0]);
                if (fnd == null)
                    LogError("The command specified could not be found");
                else
                {
                    if (fnd.CommandArgs.Length > 0)
                        LogInfo("{0}: {1} - {2}", fnd.CommandName, fnd.CommandDesc, fnd.CommandArgs.ToSingular());
                    else
                        LogInfo("{0}: {1}", fnd.CommandName, fnd.CommandDesc);
                }
            }
            else
                LogInfo("Commands: " + Command.RegisteredCommands.Select(x => x.CommandName).ToSingular());
        }

        static void types_CommandFired(Command cmd)
        {
            LogInfo("[Int32: {0} - {1}], [Int64: {2} - {3}], [String: \"raw text\"], [Colour: r,g,b (EG: 128, 32, 255)]", Int32.MinValue, Int32.MaxValue, Int64.MinValue, Int64.MaxValue);
        }

        static void hide_CommandFired(Command cmd)
        {
            StardewInvoke(() => { StardewForm.Hide(); });
        }

        static void show_CommandFired(Command cmd)
        {
            StardewInvoke(() => { StardewForm.Show(); });
        }

        static void save_CommandFired(Command cmd)
        {
            StardewValley.SaveGame.Save();
        }

        static void exit_CommandFired(Command cmd)
        {
            Application.Exit();
            Environment.Exit(0);
        }

        static void player_setName(Command cmd)
        {
            if (cmd.CalledArgs.Length > 1)
            {
                string obj = cmd.CalledArgs[0];
                string[] objs = "player,pet,farm".Split(new[] {','});
                if (objs.Contains(obj))
                {
                    switch (obj)
                    {
                        case "player":
                            Game1.player.Name = cmd.CalledArgs[1];
                            break;
                        case "pet":
                            LogError("Pets cannot currently be renamed.");
                            break;
                        case "farm":
                            Game1.player.farmName = cmd.CalledArgs[1];
                            break;
                    }
                }
                else
                {
                    LogObjectInvalid();
                }
            }
            else
            {
                LogObjectValueNotSpecified();
            }
        }

        static void player_setMoney(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                if (cmd.CalledArgs[0] == "inf")
                {
                    infMoney = true;
                }
                else
                {
                    infMoney = false;
                    int ou = 0;
                    if (Int32.TryParse(cmd.CalledArgs[0], out ou))
                    {
                        Game1.player.Money = ou;
                        LogInfo("Set {0}'s money to {1}", Game1.player.Name, Game1.player.Money);
                    }
                    else
                    {
                        LogValueNotInt32();
                    }
                }
            }
            else
            {
                LogValueNotSpecified();
            }
        }

        static void player_setStamina(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                if (cmd.CalledArgs[0] == "inf")
                {
                    infStamina = true;
                }
                else
                {
                    infStamina = false;
                    int ou = 0;
                    if (Int32.TryParse(cmd.CalledArgs[0], out ou))
                    {
                        Game1.player.Stamina = ou;
                        LogInfo("Set {0}'s stamina to {1}", Game1.player.Name, Game1.player.Stamina);
                    }
                    else
                    {
                        LogValueNotInt32();
                    }
                }
            }
            else
            {
                LogValueNotSpecified();
            }
        }

        static void player_setMaxStamina(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                int ou = 0;
                if (Int32.TryParse(cmd.CalledArgs[0], out ou))
                {
                    Game1.player.MaxStamina = ou;
                    LogInfo("Set {0}'s max stamina to {1}", Game1.player.Name, Game1.player.MaxStamina);
                }
                else
                {
                    LogValueNotInt32();
                }
            }
            else
            {
                LogValueNotSpecified();
            }
        }

        static void player_setLevel(Command cmd)
        {
            if (cmd.CalledArgs.Length > 1)
            {
                string skill = cmd.CalledArgs[0];
                string[] skills = "luck,mining,combat,farming,fishing,foraging".Split(new[] { ',' });
                if (skills.Contains(skill))
                {
                    int ou = 0;
                    if (Int32.TryParse(cmd.CalledArgs[1], out ou))
                    {
                        switch (skill)
                        {
                            case "luck":
                                Game1.player.LuckLevel = ou;
                                break;
                            case "mining":
                                Game1.player.MiningLevel = ou;
                                break;
                            case "combat":
                                Game1.player.CombatLevel = ou;
                                break;
                            case "farming":
                                Game1.player.FarmingLevel = ou;
                                break;
                            case "fishing":
                                Game1.player.FishingLevel = ou;
                                break;
                            case "foraging":
                                Game1.player.ForagingLevel = ou;
                                break;
                        }
                    }
                    else
                    {
                        LogValueNotInt32();
                    }
                }
                else
                {
                    LogError("<skill> is invalid");
                }
            }
            else
            {
                LogError("<skill> and <value> must be specified");
            }
        }

        static void player_setSpeed(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                if (cmd.CalledArgs[0].IsInt32())
                {
                    Game1.player.addedSpeed = cmd.CalledArgs[0].AsInt32();
                    LogInfo("Set {0}'s added speed to {1}", Game1.player.Name, Game1.player.addedSpeed);
                }
                else
                {
                    LogValueNotInt32();
                }
            }
            else
            {
                LogValueNotSpecified();
            }
        }

        static void player_changeColour(Command cmd)
        {
            if (cmd.CalledArgs.Length > 1)
            {
                string obj = cmd.CalledArgs[0];
                string[] objs = "hair,eyes,pants".Split(new[] { ',' });
                if (objs.Contains(obj))
                {
                    string[] cs = cmd.CalledArgs[1].Split(new[] {','}, 3);
                    if (cs[0].IsInt32() && cs[1].IsInt32() && cs[2].IsInt32())
                    {
                        Color c = new Color(cs[0].AsInt32(), cs[1].AsInt32(), cs[2].AsInt32());
                        switch (obj)
                        {
                            case "hair":
                                Game1.player.hairstyleColor = c;
                                break;
                            case "eyes":
                                Game1.player.changeEyeColor(c);
                                break;
                            case "pants":
                                Game1.player.pantsColor = c;
                                break;
                        }
                    }
                    else
                    {
                        LogError("<colour> is invalid");
                    }
                }
                else
                {
                    LogObjectInvalid();
                }
            }
            else
            {
                LogError("<object> and <colour> must be specified");
            }
        }

        static void player_changeStyle(Command cmd)
        {
            if (cmd.CalledArgs.Length > 1)
            {
                string obj = cmd.CalledArgs[0];
                string[] objs = "hair,shirt,skin,acc,shoe,swim,gender".Split(new[] { ',' });
                if (objs.Contains(obj))
                {
                    if (cmd.CalledArgs[1].IsInt32())
                    {
                        int i = cmd.CalledArgs[1].AsInt32();
                        switch (obj)
                        {
                            case "hair":
                                Game1.player.changeHairStyle(i);
                                break;
                            case "shirt":
                                Game1.player.changeShirt(i);
                                break;
                            case "acc":
                                Game1.player.changeAccessory(i);
                                break;
                            case "skin":
                                Game1.player.changeSkinColor(i);
                                break;
                            case "shoe":
                                Game1.player.changeShoeColor(i);
                                break;
                            case "swim":
                                if (i == 0)
                                    Game1.player.changeOutOfSwimSuit();
                                else if (i == 1)
                                    Game1.player.changeIntoSwimsuit();
                                else
                                    LogError("<value> must be 0 or 1 for this <object>");
                                break;
                            case "gender":
                                if (i == 0)
                                    Game1.player.changeGender(true);
                                else if (i == 1)
                                    Game1.player.changeGender(false);
                                else
                                    LogError("<value> must be 0 or 1 for this <object>");
                                break;
                        }
                    }
                    else
                    {
                        LogValueInvalid();
                    }
                }
                else
                {
                    LogObjectInvalid();
                }
            }
            else
            {
                LogObjectValueNotSpecified();
            }
        }

        static void world_freezeTime(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                if (cmd.CalledArgs[0].IsInt32())
                {
                    if (cmd.CalledArgs[0].AsInt32() == 0 || cmd.CalledArgs[0].AsInt32() == 1)
                    {
                        freezeTime = cmd.CalledArgs[0].AsInt32() == 1;
                        frozenTime = freezeTime ? Game1.timeOfDay : 0;
                        LogInfo("Time is now " + (freezeTime ? "frozen" : "thawed"));
                    }
                    else
                    {
                        LogError("<value> should be 0 or 1");
                    }
                }
                else
                {
                    LogValueNotInt32();
                }
            }
            else
            {
                LogValueNotSpecified();
            }
        }

        static void world_setTime(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                if (cmd.CalledArgs[0].IsInt32())
                {
                    if (cmd.CalledArgs[0].AsInt32() <= 2600 && cmd.CalledArgs[0].AsInt32() >= 600)
                    {
                        Game1.timeOfDay = cmd.CalledArgs[0].AsInt32();
                        frozenTime = freezeTime ? Game1.timeOfDay : 0;
                        LogInfo("Time set to: " + Game1.timeOfDay);
                    }
                    else
                    {
                        LogError("<value> should be between 600 and 2600 (06:00 AM - 02:00 AM [NEXT DAY])");
                    }
                }
                else
                {
                    LogValueNotInt32();
                }
            }
            else
            {
                LogValueNotSpecified();
            }
        }

        static void world_setDay(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                if (cmd.CalledArgs[0].IsInt32())
                {
                    if (cmd.CalledArgs[0].AsInt32() <= 28 && cmd.CalledArgs[0].AsInt32() > 0)
                    {
                        Game1.dayOfMonth = cmd.CalledArgs[0].AsInt32();
                    }
                    else
                    {
                        LogError("<value> must be between 1 and 28");
                    }
                }
                else
                {
                    LogValueNotInt32();
                }
            }
            else
            {
                LogValueNotSpecified();
            }
        }

        static void world_setSeason(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                string obj = cmd.CalledArgs[0];
                string[] objs = "winter,spring,summer,fall".Split(new []{','});
                if (objs.Contains(obj))
                {
                    Game1.currentSeason = obj;
                }
                else
                {
                    LogValueInvalid();
                }
            }
            else
            {
                LogValueNotSpecified();
            }
        }

        static void player_setHealth(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                if (cmd.CalledArgs[0] == "inf")
                {
                    infHealth = true;
                }
                else
                {
                    infHealth = false;
                    if (cmd.CalledArgs[0].IsInt32())
                    {
                        Game1.player.health = cmd.CalledArgs[0].AsInt32();
                    }
                    else
                    {
                        LogValueNotInt32();
                    }
                }
            }
            else
            {
                LogValueNotSpecified();
            }
        }

        static void player_setMaxHealth(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                if (cmd.CalledArgs[0].IsInt32())
                {
                    Game1.player.maxHealth = cmd.CalledArgs[0].AsInt32();
                }
                else
                {
                    LogValueNotInt32();
                }
            }
            else
            {
                LogValueNotSpecified();
            }
        }

        static void player_setImmunity(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                if (cmd.CalledArgs[0].IsInt32())
                {
                    Game1.player.immunity = cmd.CalledArgs[0].AsInt32();
                }
                else
                {
                    LogValueNotInt32();
                }
            }
            else
            {
                LogValueNotSpecified();
            }
        }

        static void player_addItem(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                if (cmd.CalledArgs[0].IsInt32())
                {
                    int count = 1;
                    if (cmd.CalledArgs.Length > 1)
                    {
                        if (cmd.CalledArgs[1].IsInt32())
                        {
                            count = cmd.CalledArgs[1].AsInt32();
                        }
                        else
                        {
                            LogError("<count> is invalid");
                            return;
                        }
                    }
                    Game1.player.addItemByMenuIfNecessary((Item) new StardewValley.Object(cmd.CalledArgs[0].AsInt32(), count));
                }
                else
                {
                    LogError("<item> is invalid");
                }
            }
            else
            {
                LogObjectValueNotSpecified();
            }
        }

        static void player_addMelee(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                if (cmd.CalledArgs[0].IsInt32())
                {
                    MeleeWeapon toAdd = new MeleeWeapon(cmd.CalledArgs[0].AsInt32());
                    Game1.player.addItemByMenuIfNecessary(toAdd);
                    LogInfo("Given {0} to {1}", toAdd.Name, Game1.player.Name);
                }
                else
                {
                    LogError("<item> is invalid");
                }
            }
            else
            {
                LogObjectValueNotSpecified();
            }
        }

        static void out_items(Command cmd)
        {
            for (int i = 0; i < 1000; i++)
            {
                try
                {
                    Item it = new StardewValley.Object(i, 1);
                    Console.WriteLine(i + "| " + it.Name);
                }
                catch
                {
                    
                }
            }
        }

        static void out_melee(Command cmd)
        {
            Dictionary<int, string> d = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
            Console.Write("DATA\\WEAPONS: ");
            foreach (var v in d)
            {
                Console.WriteLine(v.Key + " | " + v.Value);
            }
        }

        static void world_downMineLevel(Command cmd)
        {
            Game1.nextMineLevel();
        }

        static void world_setMineLevel(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                if (cmd.CalledArgs[0].IsInt32())
                {
                    Game1.enterMine(true, cmd.CalledArgs[0].AsInt32(), "");
                }
                else
                {
                    LogValueNotInt32();
                }
            }
            else
            {
                LogValueNotSpecified();
            }
        }

        static void blank_command(Command cmd) { }

        #endregion



        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        #region Logging

        public static void Log(object o, params object[] format)
        {
            Console.WriteLine("[{0}] {1}", System.DateTime.Now.ToLongTimeString(), String.Format(o.ToString(), format));
        }

        public static void LogColour(ConsoleColor c, object o, params object[] format)
        {
            Console.ForegroundColor = c;
            Log(o.ToString(), format);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void LogInfo(object o, params object[] format)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log(o.ToString(), format);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void LogError(object o, params object[] format)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Log(o.ToString(), format);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void LogValueNotSpecified()
        {
            LogError("<value> must be specified");
        }

        public static void LogObjectValueNotSpecified()
        {
            LogError("<object> and <value> must be specified");
        }

        public static void LogValueInvalid()
        {
            LogError("<value> is invalid");
        }

        public static void LogObjectInvalid()
        {
            LogError("<object> is invalid");
        }

        public static void LogValueNotInt32()
        {
            LogError("<value> must be a whole number (Int32)");
        }

        #endregion
    }
}
