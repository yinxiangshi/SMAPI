using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Inheritance;
using StardewModdingAPI.Inheritance.Menus;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace StardewModdingAPI
{
    public class Program
    {
        public static string ExecutionPath { get; private set; }
        public static string DataPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"));
        public static List<string> ModPaths = new List<string>();
        public static List<string> ModContentPaths = new List<string>();
        public static string LogPath = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley")), "ErrorLogs");

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

        public static bool StardewInjectorLoaded { get; private set; }
        public static Mod StardewInjectorMod { get; private set; }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void Main(string[] args)
        {
            Console.Title = "Stardew Modding API Console";

            Console.Title += " - Version " + Version;
#if DEBUG
            Console.Title += " - DEBUG IS NOT FALSE, AUTHOUR NEEDS TO REUPLOAD THIS VERSION";
#endif

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
                    Log.Error("Could not create a missing ModPath: " + ModPath + "\n\n" + ex);
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
                    Log.Error("Could not create a missing ModContentPath: " + ModContentPath + "\n\n" + ex);
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
                Log.Error("Could not create the missing ErrorLogs path: " + LogPath + "\n\n" + ex);
            }

            Log.Info("Initializing SDV Assembly...");
            if (!File.Exists(ExecutionPath + "\\Stardew Valley.exe"))
            {
                //If the api isn't next to SDV.exe then terminate. Though it'll crash before we even get here w/o sdv.exe. Perplexing.
                Log.Error("Could not find: " + ExecutionPath + "\\Stardew Valley.exe");
                Log.Error("The API will now terminate.");
                Console.ReadKey();
                Environment.Exit(-4);
            }

            //Load in that assembly. Also, ignore security :D
            StardewAssembly = Assembly.UnsafeLoadFrom(ExecutionPath + "\\Stardew Valley.exe");

            //This will load the injector before anything else if it sees it
            //It doesn't matter though
            //I'll leave it as a feature in case anyone in the community wants to tinker with it
            //All you need is a DLL that inherits from mod and is called StardewInjector.dll with an Entry() method
            foreach (string ModPath in ModPaths)
            {
                foreach (String s in Directory.GetFiles(ModPath, "StardewInjector.dll"))
                {
                    Log.Success(ConsoleColor.Green, "Found Stardew Injector DLL: " + s);
                    try
                    {
                        Assembly mod = Assembly.UnsafeLoadFrom(s); //to combat internet-downloaded DLLs

                        if (mod.DefinedTypes.Count(x => x.BaseType == typeof(Mod)) > 0)
                        {
                            Log.Success("Loading Injector DLL...");
                            TypeInfo tar = mod.DefinedTypes.First(x => x.BaseType == typeof(Mod));
                            Mod m = (Mod)mod.CreateInstance(tar.ToString());
                            Console.WriteLine("LOADED: {0} by {1} - Version {2} | Description: {3}", m.Name, m.Authour, m.Version, m.Description);
                            m.Entry(false);
                            StardewInjectorLoaded = true;
                            StardewInjectorMod = m;
                        }
                        else
                        {
                            Log.Error("Invalid Mod DLL");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Failed to load mod '{0}'. Exception details:\n" + ex, s);
                    }
                }
            }

            StardewProgramType = StardewAssembly.GetType("StardewValley.Program", true);
            StardewGameInfo = StardewProgramType.GetField("gamePtr");

            #region deprecated
            /*
             * Lol no. I tried though.
            if (File.Exists(ExecutionPath + "\\Stardew_Injector.exe"))
            {
                //Stardew_Injector Mode
                StardewInjectorLoaded = true;
                Program.Log.LogInfo("STARDEW_INJECTOR DETECTED, LAUNCHING USING INJECTOR CALLS");
                Assembly inj = Assembly.UnsafeLoadFrom(ExecutionPath + "\\Stardew_Injector.exe");
                Type prog = inj.GetType("Stardew_Injector.Program", true);
                FieldInfo hooker = prog.GetField("hooker", BindingFlags.NonPublic | BindingFlags.Static);

                //hook.GetMethod("Initialize").Invoke(hooker.GetValue(null), null);
                //customize the initialize method for SGame instead of Game
                Assembly cecil = Assembly.UnsafeLoadFrom(ExecutionPath + "\\Mono.Cecil.dll");
                Type assDef = cecil.GetType("Mono.Cecil.AssemblyDefinition");
                var aDefs = assDef.GetMethods(BindingFlags.Public | BindingFlags.Static);
                var aDef = aDefs.First(x => x.ToString().Contains("ReadAssembly(System.String)"));
                var theAssDef = aDef.Invoke(null, new object[] { Assembly.GetExecutingAssembly().Location });
                var modDef = assDef.GetProperty("MainModule", BindingFlags.Public | BindingFlags.Instance);
                var theModDef = modDef.GetValue(theAssDef);
                Console.WriteLine("MODDEF: " + theModDef);
                Type hook = inj.GetType("Stardew_Injector.Stardew_Hooker", true);
                hook.GetField("m_vAsmDefinition", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(hooker.GetValue(null), theAssDef);
                hook.GetField("m_vModDefinition", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(hooker.GetValue(null), theModDef);

                //hook.GetMethod("Initialize").Invoke(hooker.GetValue(null), null);
                hook.GetMethod("ApplyHooks").Invoke(hooker.GetValue(null), null);
                //hook.GetMethod("Finalize").Invoke(hooker.GetValue(null), null);
                //hook.GetMethod("Run").Invoke(hooker.GetValue(null), null);

                Console.ReadKey();
                //Now go back and load Stardew through SMAPI
            }
            */
            #endregion

            //Change the game's version
            Log.Info("Injecting New SDV Version...");
            Game1.version += "-Z_MODDED | SMAPI " + Version;

            //Create the thread for the game to run in.
            gameThread = new Thread(RunGame);
            Log.Info("Starting SDV...");
            gameThread.Start();

            //I forget.
            SGame.GetStaticFields();

            while (!ready)
            {
                //Wait for the game to load up
            }

            //SDV is running
            Log.Comment("SDV Loaded Into Memory");

            //Create definition to listen for input
            Log.Verbose("Initializing Console Input Thread...");
            consoleInputThread = new Thread(ConsoleInputThread);

            //The only command in the API (at least it should be, for now)\

            Command.RegisterCommand("help", "Lists all commands | 'help <cmd>' returns command description").CommandFired += help_CommandFired;
            //Command.RegisterCommand("crash", "crashes sdv").CommandFired += delegate { Game1.player.draw(null); };

            //Subscribe to events
            Events.ControlEvents.KeyPressed += Events_KeyPressed;
            Events.GameEvents.LoadContent += Events_LoadContent;
            //Events.MenuChanged += Events_MenuChanged; //Idk right now

#if DEBUG
            //Experimental
            //Events.LocationsChanged += Events_LocationsChanged;
            //Events.CurrentLocationChanged += Events_CurrentLocationChanged;
#endif

            //Do tweaks using winforms invoke because I'm lazy
            Log.Verbose("Applying Final SDV Tweaks...");
            StardewInvoke(() =>
            {
                gamePtr.IsMouseVisible = false;
                gamePtr.Window.Title = "Stardew Valley - Version " + Game1.version;
                StardewForm.Resize += Events.GraphicsEvents.InvokeResize;
            });

            //Game's in memory now, send the event
            Log.Verbose("Game Loaded");
            Events.GameEvents.InvokeGameLoaded();

            Log.Comment(ConsoleColor.Cyan, "Type 'help' for help, or 'help <cmd>' for a command's usage");
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

            Log.Verbose("Game Execution Finished");
            Log.Verbose("Shutting Down...");
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
            Application.ThreadException += Log.Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomain_UnhandledException;

            try
            {
                gamePtr = new SGame();
                Log.Verbose("Patching SDV Graphics Profile...");
                Game1.graphics.GraphicsProfile = GraphicsProfile.HiDef;
                LoadMods();

                StardewForm = Control.FromHandle(Program.gamePtr.Window.Handle).FindForm();
                StardewForm.Closing += StardewForm_Closing;

                ready = true;

                StardewGameInfo.SetValue(StardewProgramType, gamePtr);
                gamePtr.Run();

                #region deprecated
                if (false)
                {
                    //Nope, I can't get it to work. I depend on Game1 being an SGame, and can't cast a parent to a child
                    //I'm leaving this here in case the community is interested
                    //StardewInjectorMod.Entry(true);
                    Type gt = StardewAssembly.GetType("StardewValley.Game1", true);
                    gamePtr = (SGame)Activator.CreateInstance(gt);

                    ready = true;

                    StardewGameInfo.SetValue(StardewProgramType, gamePtr);
                    gamePtr.Run();
                }
                #endregion
            }
            catch (Exception ex)
            {
                Log.Error("Game failed to start: " + ex);
            }
        }

        static void StardewForm_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            if (true || MessageBox.Show("Are you sure you would like to quit Stardew Valley?\nUnsaved progress will be lost!", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                gamePtr.Exit();
                gamePtr.Dispose();
                StardewForm.Hide();
                ready = false;
            }
        }

        public static void LoadMods()
        {
            Log.Verbose("LOADING MODS");
            int loadedMods = 0;
            foreach (string ModPath in ModPaths)
            {
                foreach (String s in Directory.GetFiles(ModPath, "*.dll"))
                {
                    if (s.Contains("StardewInjector"))
                        continue;
                    Log.Success("Found DLL: " + s);
                    try
                    {
                        Assembly mod = Assembly.UnsafeLoadFrom(s); //to combat internet-downloaded DLLs

                        if (mod.DefinedTypes.Count(x => x.BaseType == typeof(Mod)) > 0)
                        {
                            Log.Verbose("Loading Mod DLL...");
                            TypeInfo tar = mod.DefinedTypes.First(x => x.BaseType == typeof(Mod));
                            Mod m = (Mod)mod.CreateInstance(tar.ToString());
                            Console.WriteLine("LOADED MOD: {0} by {1} - Version {2} | Description: {3}", m.Name, m.Authour, m.Version, m.Description);
                            loadedMods += 1;
                            m.Entry();
                        }
                        else
                        {
                            Log.Error("Invalid Mod DLL");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Failed to load mod '{0}'. Exception details:\n" + ex, s);
                    }
                }
            }
            Log.Success("LOADED {0} MODS", loadedMods);
        }

        public static void ConsoleInputThread()
        {
            string input = string.Empty;

            while (true)
            {
                Command.CallCommand(Console.ReadLine());
            }
        }

        static void Events_LoadContent(object o, EventArgs e)
        {
            Log.Info("Initializing Debug Assets...");
            DebugPixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            DebugPixel.SetData(new Color[] { Color.White });

#if DEBUG
            Log.Verbose("REGISTERING BASE CUSTOM ITEM");
            SObject so = new SObject();
            so.Name = "Mario Block";
            so.CategoryName = "SMAPI Test Mod";
            so.Description = "It's a block from Mario!\nLoaded in realtime by SMAPI.";
            so.Texture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, new FileStream(ModContentPaths[0] + "\\Test.png", FileMode.Open));
            so.IsPassable = true;
            so.IsPlaceable = true;
            Log.Verbose("REGISTERED WITH ID OF: " + SGame.RegisterModItem(so));

            Log.Verbose("REGISTERING SECOND CUSTOM ITEM");
            SObject so2 = new SObject();
            so2.Name = "Mario Painting";
            so2.CategoryName = "SMAPI Test Mod";
            so2.Description = "It's a painting of a creature from Mario!\nLoaded in realtime by SMAPI.";
            so2.Texture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, new FileStream(ModContentPaths[0] + "\\PaintingTest.png", FileMode.Open));
            so2.IsPassable = true;
            so2.IsPlaceable = true;
            Log.Verbose("REGISTERED WITH ID OF: " + SGame.RegisterModItem(so2));

            Command.CallCommand("load");
#endif
        }

        static void Events_KeyPressed(object o, EventArgsKeyPressed e)
        {

        }

        static void Events_MenuChanged(IClickableMenu newMenu)
        {
            Log.Verbose("NEW MENU: " + newMenu.GetType());
            if (newMenu is GameMenu)
            {
                Game1.activeClickableMenu = SGameMenu.ConstructFromBaseClass(Game1.activeClickableMenu as GameMenu);
            }
        }


        static void Events_LocationsChanged(List<GameLocation> newLocations)
        {
#if DEBUG
            SGame.ModLocations = SGameLocation.ConstructFromBaseClasses(Game1.locations);
#endif
        }

        static void Events_CurrentLocationChanged(GameLocation newLocation)
        {
            //SGame.CurrentLocation = null;
            //System.Threading.Thread.Sleep(10);
#if DEBUG
            Console.WriteLine(newLocation.name);
            SGame.CurrentLocation = SGame.LoadOrCreateSGameLocationFromName(newLocation.name);
#endif
            //Game1.currentLocation = SGame.CurrentLocation;
            //Log.LogComment(((SGameLocation) newLocation).name);
            //Log.LogComment("LOC CHANGED: " + SGame.currentLocation.name);
        }

        public static void StardewInvoke(Action a)
        {
            StardewForm.Invoke(a);
        }

        static void help_CommandFired(object o, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                Command fnd = Command.FindCommand(e.Command.CalledArgs[0]);
                if (fnd == null)
                    Log.Error("The command specified could not be found");
                else
                {
                    if (fnd.CommandArgs.Length > 0)
                        Log.Info("{0}: {1} - {2}", fnd.CommandName, fnd.CommandDesc, fnd.CommandArgs.ToSingular());
                    else
                        Log.Info("{0}: {1}", fnd.CommandName, fnd.CommandDesc);
                }
            }
            else
                Log.Info("Commands: " + Command.RegisteredCommands.Select(x => x.CommandName).ToSingular());
        }
    }
}