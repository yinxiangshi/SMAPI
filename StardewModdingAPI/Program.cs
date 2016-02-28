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
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace StardewModdingAPI
{
    public class Program
    {
        public static string DataPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"));
        public static string ModPath = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley")), "Mods");

        public static Game1 gamePtr;
        public static bool ready;

        public static Assembly StardewAssembly;
        public static Type StardewProgramType;
        public static FieldInfo StardewGameInfo;
        public static Form StardewForm;

        public static Thread gameThread;
        public static Thread consoleInputThread;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void Main(string[] args)
        {
            Console.Title = "Stardew Modding API Console";

            if (File.Exists(ModPath))
                File.Delete(ModPath);
            if (!Directory.Exists(ModPath))
                Directory.CreateDirectory(ModPath);

            Log(Assembly.GetExecutingAssembly().Location);
            LogInfo("Initializing SDV Assembly...");
            StardewAssembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name.Equals("Stardew Valley"));
            StardewProgramType = StardewAssembly.GetType("StardewValley.Program", true);
            StardewGameInfo = StardewProgramType.GetField("gamePtr");

            LogInfo("Injecting New SDV Version...");
            Game1.version += "-Z_MODDED";

            gameThread = new Thread(RunGame);
            LogInfo("Starting SDV...");
            gameThread.Start();

            while (!ready)
            {

            }

            Log("SDV Loaded Into Memory");

            consoleInputThread = new Thread(ConsoleInputThread);
            LogInfo("Initializing Console Input Thread...");
            consoleInputThread.Start();


            LogInfo("Applying Final SDV Tweaks...");
            StardewInvoke(() =>
            {
                                    gamePtr.IsMouseVisible = false;
                                    gamePtr.Window.Title = "Stardew Valley";
            });

            LogInfo("Game Loaded");
            LogColour(ConsoleColor.Cyan,  "Type 'help' for help, or 'help <cmd>' for a command's usage");
            Events.InvokeGameLoaded();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void RunGame()
        {
            try
            {
                gamePtr = new Game1();
                Game1.graphics.GraphicsProfile = GraphicsProfile.HiDef;
                LoadMods();

                StardewForm = Control.FromHandle(Program.gamePtr.Window.Handle).FindForm();
                StardewGameInfo.SetValue(StardewProgramType, gamePtr);

                KeyboardInput.KeyDown += KeyboardInput_KeyDown;

                ready = true;

                gamePtr.Run();
            }
            catch (Exception ex)
            {
                LogError("Game failed to start: " + ex);
            }
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

        static void KeyboardInput_KeyDown(object sender, StardewValley.KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.B:
                    Game1.player.hairstyleColor = Extensions.RandomColour();
                    break;
                case Keys.OemPlus:
                    Game1.player.Money += 5000;
                    break;
            }
        }

        public static void StardewInvoke(Action a)
        {
            StardewForm.Invoke(a);
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
            Command.RegisterCommand("player_setmoney", "Sets the player's money | player_setmoney <value>", new[] { "(Int32)<value> The target money" }).CommandFired += player_setMoney;
            Command.RegisterCommand("player_setenergy", "Sets the player's energy | player_setenergy <value>", new[] { "(Int32)<value> The target energy" }).CommandFired += player_setEnergy;
            Command.RegisterCommand("player_setmaxenergy", "Sets the player's max energy | player_setmaxenergy <value>", new[] { "(Int32)<value> The target max energy" }).CommandFired += player_setMaxEnergy;

            Command.RegisterCommand("player_setlevel", "Sets the player's specified skill to the specified value | player_setlevel <skill> <value>", new[] { "(luck, mining, combat, farming, fishing, foraging)<skill> (1-10)<value> The target level" }).CommandFired += player_setLevel;
            Command.RegisterCommand("player_setspeed", "Sets the player's speed to the specified value?", new[] {"(Int32:5)<value> The target speed"}).CommandFired += player_setSpeed;
            Command.RegisterCommand("player_changecolour", "Sets the player's colour of the specified object | player_changecolor <object> <colour>", new[] { "(hair, eyes, pants)<object> (r,g,b)<colour>" }).CommandFired += player_changeColour;
            Command.RegisterCommand("player_changestyle", "Sets the player's style of the specified object | player_changecolor <object> <value>", new[] { "(hair, shirt, skin, acc, shoe, swim, gender)<object> (Int32)<value>" }).CommandFired += player_changeStyle;

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
                    LogError("<object> is invalid");
                }
            }
            else
            {
                LogError("<object> and <value> must be specified");
            }
        }

        static void player_setMoney(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                int ou = 0;
                if (Int32.TryParse(cmd.CalledArgs[0], out ou))
                {
                    Game1.player.Money = ou;
                    LogInfo("Set {0}'s money to {1}", Game1.player.Name, Game1.player.Money);
                }
                else
                {
                    LogError("<value> must be a whole number (Int32)");
                }
            }
            else
            {
                LogError("<value> must be specified");
            }
        }

        static void player_setEnergy(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                int ou = 0;
                if (Int32.TryParse(cmd.CalledArgs[0], out ou))
                {
                    Game1.player.Stamina = ou;
                    LogInfo("Set {0}'s energy to {1}", Game1.player.Name, Game1.player.Stamina);
                }
                else
                {
                    LogError("<value> must be a whole number (Int32)");
                }
            }
            else
            {
                LogError("<value> must be specified");
            }
        }

        static void player_setMaxEnergy(Command cmd)
        {
            if (cmd.CalledArgs.Length > 0)
            {
                int ou = 0;
                if (Int32.TryParse(cmd.CalledArgs[0], out ou))
                {
                    Game1.player.MaxStamina = ou;
                    LogInfo("Set {0}'s max energy to {1}", Game1.player.Name, Game1.player.MaxStamina);
                }
                else
                {
                    LogError("<value> must be a whole number (Int32)");
                }
            }
            else
            {
                LogError("<value> must be specified");
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
                        LogError("<value> must be a whole number (Int32)");
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
                    Game1.player.Speed = cmd.CalledArgs[0].AsInt32();
                    LogInfo("Set {0}'s speed to {1}", Game1.player.Name, Game1.player.Speed);
                }
                else
                {
                    LogError("<value> must be a whole number (Int32)");
                }
            }
            else
            {
                LogError("<value> must be specified");
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
                    LogError("<object> is invalid");
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
                        LogError("<value> is invalid");
                    }
                }
                else
                {
                    LogError("<object> is invalid");
                }
            }
            else
            {
                LogError("<object> and <value> must be specified");
            }
        }

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

        #endregion
    }
}
