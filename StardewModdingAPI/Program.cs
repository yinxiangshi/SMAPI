using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Inheritance;
using StardewModdingAPI.Inheritance.Menus;
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
        public static List<string> ModPaths = new List<string>();
        public static List<string> ModContentPaths = new List<string>(); 
        public static string LogPath = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley")), "ErrorLogs");
        public static string CurrentLog { get; private set; }
        public static StreamWriter LogStream { get; private set; }

        public static Texture2D DebugPixel { get; private set; }

        public static SGame gamePtr;
        public static bool ready;

        public static Assembly StardewAssembly;
        public static Type StardewProgramType;
        public static FieldInfo StardewGameInfo;
        public static Form StardewForm;

        public static Thread gameThread;
        public static Thread consoleInputThread;

        public const string Version = "0.36 Alpha";
        public const bool debug = true;
        public static bool disableLogging { get; private set; }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        private static void Main(string[] args)
        {
            Console.Title = "Stardew Modding API Console";

            Console.Title += " - Version " + Version;
            if (debug)
                Console.Title += " - DEBUG IS NOT FALSE, AUTHOUR NEEDS TO REUPLOAD THIS VERSION";
            
            //TODO: Have an app.config and put the paths inside it so users can define locations to load mods from
            ExecutionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ModPaths.Add(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley")), "Mods"));
            ModPaths.Add(Path.Combine(ExecutionPath, "Mods"));
            ModPaths.Add(Path.Combine(Path.Combine(ExecutionPath, "Mods"), "Content"));
            ModContentPaths.Add(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley")), "Mods"), "Content"));

            //Checks that all defined modpaths exist as directories
            foreach (string ModPath in ModPaths)
            {
                try
                {
                    if (File.Exists(ModPath))
                        File.Delete(ModPath);
                    if (!Directory.Exists(ModPath))
                        Directory.CreateDirectory(ModPath);
                }
                catch (Exception ex)
                {
                    
                    LogError("Could not create a missing ModPath: " + ModPath + "\n\n" + ex);
                }
            }
            //Same for content
            foreach (string ModContentPath in ModContentPaths)
            {
                try
                {
                    if (!Directory.Exists(ModContentPath))
                        Directory.CreateDirectory(ModContentPath);
                }
                catch (Exception ex)
                {
                    LogError("Could not create a missing ModContentPath: " + ModContentPath + "\n\n" + ex);
                }
            }
            //And then make sure we have an errorlog dir
            try
            {
                if (!Directory.Exists(LogPath))
                    Directory.CreateDirectory(LogPath);
            }
            catch (Exception ex)
            {
                LogError("Could not create the missing ErrorLogs path: " + LogPath + "\n\n" + ex);
            }

            //Define the path to the current log file
            CurrentLog = LogPath + "\\MODDED_ProgramLog_LATEST"/* + System.DateTime.Now.Ticks + */ + ".txt";

            Log(ExecutionPath, false);

            //Create a writer to the log file
            try
            {
                LogStream = new StreamWriter(CurrentLog, false);
            }
            catch (Exception ex)
            {
                disableLogging = true;
                LogError("Could not initialize LogStream - Logging is disabled");
            }


            LogInfo("Initializing SDV Assembly...");
            if (!File.Exists(ExecutionPath + "\\Stardew Valley.exe"))
            {
                //If the api isn't next to SDV.exe then terminate. Though it'll crash before we even get here w/o sdv.exe. Perplexing.
                LogError("Could not find: " + ExecutionPath + "\\Stardew Valley.exe");
                LogError("The API will now terminate.");
                Console.ReadKey();
                Environment.Exit(-4);
            }

            //Load in that assembly. Also, ignore security :D
            StardewAssembly = Assembly.UnsafeLoadFrom(ExecutionPath + "\\Stardew Valley.exe");
            StardewProgramType = StardewAssembly.GetType("StardewValley.Program", true);
            StardewGameInfo = StardewProgramType.GetField("gamePtr");

            //Change the game's version
            LogInfo("Injecting New SDV Version...");
            Game1.version += "-Z_MODDED | SMAPI " + Version;

            //Create the thread for the game to run in.
            gameThread = new Thread(RunGame);
            LogInfo("Starting SDV...");
            gameThread.Start();

            //I forget.
            SGame.GetStaticFields();
            
            while (!ready)
            {
                //Wait for the game to load up
            }

            //SDV is running
            Log("SDV Loaded Into Memory");

            //Create definition to listen for input
            LogInfo("Initializing Console Input Thread...");
            consoleInputThread = new Thread(ConsoleInputThread);

            //The only command in the API (at least it should be, for now)
            Command.RegisterCommand("help", "Lists all commands | 'help <cmd>' returns command description").CommandFired += help_CommandFired;
            //Command.RegisterCommand("crash", "crashes sdv").CommandFired += delegate { Game1.player.draw(null); };

            //Subscribe to events
            Events.KeyPressed += Events_KeyPressed;
            Events.LoadContent += Events_LoadContent;
            //Events.MenuChanged += Events_MenuChanged; //Idk right now
            if (debug)
            {
                //Experimental
                //Events.LocationsChanged += Events_LocationsChanged;
                //Events.CurrentLocationChanged += Events_CurrentLocationChanged;
            }

            //Do tweaks using winforms invoke because I'm lazy
            LogInfo("Applying Final SDV Tweaks...");
            StardewInvoke(() =>
            {
                                    gamePtr.IsMouseVisible = false;
                                    gamePtr.Window.Title = "Stardew Valley - Version " + Game1.version;
                                    StardewForm.Resize += Events.InvokeResize;
            });

            //Game's in memory now, send the event
            LogInfo("Game Loaded");
            Events.InvokeGameLoaded();

            LogColour(ConsoleColor.Cyan, "Type 'help' for help, or 'help <cmd>' for a command's usage");
            //Begin listening to input
            consoleInputThread.Start();


            while (ready)
            {
                //Check if the game is still running 10 times a second
                Thread.Sleep(1000 / 10);
            }

            //abort the thread, we're closing
            if (consoleInputThread != null && consoleInputThread.ThreadState == ThreadState.Running)
                consoleInputThread.Abort();

            LogInfo("Game Execution Finished");
            LogInfo("Shutting Down...");
            Thread.Sleep(100);
            /*
            int time = 0;
            int step = 100;
            int target = 1000;
            while (true)
            {
                time += step;
                Thread.Sleep(step);

                Console.Write(".");

                if (time >= target)
                    break;
            }
            */
            Environment.Exit(0);
        }

        

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        public static void RunGame()
        {
            //Does this even do anything???
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //I've yet to see it called :|

            try
            {
                gamePtr = new SGame();
                LogInfo("Patching SDV Graphics Profile...");
                Game1.graphics.GraphicsProfile = GraphicsProfile.HiDef;
                LoadMods();

                StardewForm = Control.FromHandle(Program.gamePtr.Window.Handle).FindForm();
                StardewForm.Closing += StardewForm_Closing;
                StardewGameInfo.SetValue(StardewProgramType, gamePtr);

                ready = true;

                gamePtr.Run();
            }
            catch (Exception ex)
            {
                LogError("Game failed to start: " + ex);
            }
        }

        static void StardewForm_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            gamePtr.Exit();
            gamePtr.Dispose();
            StardewForm.Hide();
            ready = false;
        }

        public static void LoadMods()
        {
            LogColour(ConsoleColor.Green, "LOADING MODS");
            int loadedMods = 0;
            foreach (string ModPath in ModPaths)
            {
                foreach (String s in Directory.GetFiles(ModPath, "*.dll"))
                {
                    LogColour(ConsoleColor.Green, "Found DLL: " + s);
                    try
                    {
                        Assembly mod = Assembly.UnsafeLoadFrom(s); //to combat internet-downloaded DLLs

                        if (mod.DefinedTypes.Count(x => x.BaseType == typeof (Mod)) > 0)
                        {
                            LogColour(ConsoleColor.Green, "Loading Mod DLL...");
                            TypeInfo tar = mod.DefinedTypes.First(x => x.BaseType == typeof (Mod));
                            Mod m = (Mod) mod.CreateInstance(tar.ToString());
                            Console.WriteLine("LOADED MOD: {0} by {1} - Version {2} | Description: {3}", m.Name, m.Authour, m.Version, m.Description);
                            loadedMods += 1;
                            m.Entry();
                        }
                        else
                        {
                            LogError("Invalid Mod DLL");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Failed to load mod '{0}'. Exception details:\n" + ex, s);
                    }
                }
            }
            LogColour(ConsoleColor.Green, "LOADED {0} MODS", loadedMods);
        }

        public static void ConsoleInputThread()
        {
            string input = string.Empty;

            while (true)
            {
                Command.CallCommand(Console.ReadLine());
            }
        }

        static void Events_LoadContent()
        {
            LogInfo("Initializing Debug Assets...");
            DebugPixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            DebugPixel.SetData(new Color[] { Color.White });

            if (debug)
            {
                LogColour(ConsoleColor.Magenta, "REGISTERING BASE CUSTOM ITEM");
                SObject so = new SObject();
                so.Name = "Mario Block";
                so.CategoryName = "SMAPI Test Mod";
                so.Description = "It's a block from Mario!\nLoaded in realtime by SMAPI.";
                so.Texture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, new FileStream(ModContentPaths[0] + "\\Test.png", FileMode.Open));
                so.IsPassable = true;
                so.IsPlaceable = true;
                LogColour(ConsoleColor.Cyan, "REGISTERED WITH ID OF: " + SGame.RegisterModItem(so));

                LogColour(ConsoleColor.Magenta, "REGISTERING SECOND CUSTOM ITEM");
                SObject so2 = new SObject();
                so2.Name = "Mario Painting";
                so2.CategoryName = "SMAPI Test Mod";
                so2.Description = "It's a painting of a creature from Mario!\nLoaded in realtime by SMAPI.";
                so2.Texture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, new FileStream(ModContentPaths[0] + "\\PaintingTest.png", FileMode.Open));
                so2.IsPassable = true;
                so2.IsPlaceable = true;
                LogColour(ConsoleColor.Cyan, "REGISTERED WITH ID OF: " + SGame.RegisterModItem(so2));
            }

            if (debug)
                Command.CallCommand("load");
        }

        static void Events_KeyPressed(Keys key)
        {
            
        }

        static void Events_MenuChanged(IClickableMenu newMenu)
        {
            LogInfo("NEW MENU: " + newMenu.GetType());
            if (newMenu is GameMenu)
            {
                Game1.activeClickableMenu = SGameMenu.ConstructFromBaseClass(Game1.activeClickableMenu as GameMenu);
            }
        }

        static void Events_LocationsChanged(List<GameLocation> newLocations)
        {
            if (debug)
            {
                SGame.ModLocations = SGameLocation.ConstructFromBaseClasses(Game1.locations);
            }
        }

        static void Events_CurrentLocationChanged(GameLocation newLocation)
        {
            //SGame.CurrentLocation = null;
            //System.Threading.Thread.Sleep(10);
            if (debug)
            {
                Console.WriteLine(newLocation.name);
                SGame.CurrentLocation = SGame.LoadOrCreateSGameLocationFromName(newLocation.name);
            }
            //Game1.currentLocation = SGame.CurrentLocation;
            //LogInfo(((SGameLocation) newLocation).name);
            //LogInfo("LOC CHANGED: " + SGame.currentLocation.name);
        }

        public static void StardewInvoke(Action a)
        {
            StardewForm.Invoke(a);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("An exception has been caught");
            File.WriteAllText(Program.LogPath + "\\MODDED_ErrorLog_" + Extensions.Random.Next(100000000, 999999999) + ".txt", e.ExceptionObject.ToString());
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Console.WriteLine("A thread exception has been caught");
            File.WriteAllText(Program.LogPath + "\\MODDED_ErrorLog_" + Extensions.Random.Next(100000000, 999999999) + ".txt", e.Exception.ToString());
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

        #region Logging

        public static void Log(object o, params object[] format)
        {
            if (format.Length > 0)
            {
                if (format[0] is bool)
                {
                    if ((bool)format[0] == false)
                    {
                        //suppress logging to file
                        Console.WriteLine("[{0}] {1}", System.DateTime.Now.ToLongTimeString(), String.Format(o.ToString(), format));
                        return;
                    }
                }
            }
            string toLog = string.Format("[{0}] {1}", System.DateTime.Now.ToLongTimeString(), String.Format(o.ToString(), format));
            Console.WriteLine(toLog);

            if (!disableLogging)
            {
                LogStream.WriteLine(toLog);
                LogStream.Flush();
            }
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

        public static void LogDebug(object o, params object[] format)
        {
            if (!debug)
                return;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
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
