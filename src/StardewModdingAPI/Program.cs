using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
#if SMAPI_FOR_WINDOWS
using System.Windows.Forms;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Inheritance;
using StardewValley;

namespace StardewModdingAPI
{
    /// <summary>The main entry point for SMAPI, responsible for hooking into and launching the game.</summary>
    public class Program
    {
        /*********
        ** Properties
        *********/
        /// <summary>The full path to the Stardew Valley executable.</summary>
        private static readonly string GameExecutablePath = File.Exists(Path.Combine(Constants.ExecutionPath, "StardewValley.exe"))
            ? Path.Combine(Constants.ExecutionPath, "StardewValley.exe") // Linux or Mac
            : Path.Combine(Constants.ExecutionPath, "Stardew Valley.exe"); // Windows

        /// <summary>The full path to the folder containing mods.</summary>
        private static readonly string ModPath = Path.Combine(Constants.ExecutionPath, "Mods");

        /*********
        ** Accessors
        *********/
        /// <summary>The number of mods currently loaded by SMAPI.</summary>
        public static int ModsLoaded;

        /// <summary>The underlying game instance.</summary>
        public static SGame gamePtr;

        /// <summary>Whether the game is currently running.</summary>
        public static bool ready;

        /// <summary>The underlying game assembly.</summary>
        public static Assembly StardewAssembly;

        /// <summary>The underlying <see cref="StardewValley.Program"/> type.</summary>
        public static Type StardewProgramType;

        /// <summary>The field containing game's main instance.</summary>
        public static FieldInfo StardewGameInfo;

        // unused?
        public static Thread gameThread;

        /// <summary>The thread running the console input thread.</summary>
        public static Thread consoleInputThread;

        /// <summary>A pixel which can be stretched and colourised for display.</summary>
        public static Texture2D DebugPixel { get; private set; }

        // ReSharper disable once PossibleNullReferenceException
        /// <summary>The game's build type (i.e. GOG vs Steam).</summary>
        public static int BuildType => (int)Program.StardewProgramType.GetField("buildType", BindingFlags.Public | BindingFlags.Static).GetValue(null);


        /*********
        ** Public methods
        *********/
        /// <summary>The main entry point which hooks into and launches the game.</summary>
        /// <param name="args">The command-line arguments.</param>
        private static void Main(string[] args)
        {
            // set thread culture for consistent log formatting
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

            // hook into & launch the game
            try
            {
                Log.AsyncY($"SMAPI {Constants.Version}");
                Log.AsyncY($"Stardew Valley {Game1.version} on {Environment.OSVersion}");
                Program.ConfigureConsoleWindow();
                Program.CheckForUpdateAsync();
                Program.CreateDirectories();
                Program.StartGame();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadKey();
                Log.AsyncR($"Critical error: {ex}");
            }

            // print message when game ends
            Log.AsyncY("The API will now terminate. Press any key to continue...");
            Console.ReadKey();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Configure the console window.</summary>
        private static void ConfigureConsoleWindow()
        {
            Console.Title = Constants.ConsoleTitle;
#if DEBUG
            Console.Title += " - DEBUG IS NOT FALSE, AUTHOUR NEEDS TO REUPLOAD THIS VERSION";
#endif
        }

        /// <summary>Create and verify the SMAPI directories.</summary>
        private static void CreateDirectories()
        {
            Log.AsyncY("Validating file paths...");
            Program.VerifyPath(Program.ModPath);
            Program.VerifyPath(Constants.LogDir);
            if (!File.Exists(Program.GameExecutablePath))
                throw new FileNotFoundException($"Could not find executable: {Program.GameExecutablePath}");
        }

        /// <summary>Asynchronously check for a new version of SMAPI, and print a message to the console if an update is available.</summary>
        private static void CheckForUpdateAsync()
        {
            new Thread(() =>
            {
                try
                {
                    GitRelease release = UpdateHelper.GetLatestVersionAsync(Constants.GitHubRepository).Result;
                    Version latestVersion = new Version(release.Tag);
                    if (latestVersion.CompareTo(Constants.Version) > 0)
                        Log.AsyncColour($"You can update SMAPI from version {Constants.Version} to {latestVersion}", ConsoleColor.Magenta);
                }
                catch (Exception ex)
                {
                    Log.Debug($"Couldn't check for a new version of SMAPI. This won't affect your game, but you may not be notified of new versions if this keeps happening.\n{ex}");
                }
            }).Start();
        }

        /// <summary>Hook into Stardew Valley and launch the game.</summary>
        private static void StartGame()
        {
            // load the game assembly (ignore security)
            Log.AsyncY("Initializing SDV Assembly...");
            Program.StardewAssembly = Assembly.UnsafeLoadFrom(Program.GameExecutablePath);
            Program.StardewProgramType = Program.StardewAssembly.GetType("StardewValley.Program", true);
            Program.StardewGameInfo = Program.StardewProgramType.GetField("gamePtr");

            // change the game's version
            Log.AsyncY("Injecting New SDV Version...");
            Game1.version += $"-Z_MODDED | SMAPI {Constants.Version}";

            // add error interceptors
#if SMAPI_FOR_WINDOWS
            Application.ThreadException += Log.Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
#endif
            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomain_UnhandledException;

            // initialise game
            try
            {
                Log.AsyncY("Initializing SDV...");
                Program.gamePtr = new SGame();

                // hook events
                Program.gamePtr.Exiting += (sender, e) => Program.ready = false;
                Program.gamePtr.Window.ClientSizeChanged += GraphicsEvents.InvokeResize;

                // patch graphics
                Log.AsyncY("Patching SDV Graphics Profile...");
                Game1.graphics.GraphicsProfile = GraphicsProfile.HiDef;

                // load mods
                Program.LoadMods();

                // initialise
                Program.StardewGameInfo.SetValue(Program.StardewProgramType, Program.gamePtr);
                Log.AsyncY("Applying Final SDV Tweaks...");
                Program.gamePtr.IsMouseVisible = false;
                Program.gamePtr.Window.Title = $"Stardew Valley - Version {Game1.version}";
            }
            catch (Exception ex)
            {
                Log.AsyncR($"Game failed to initialise: {ex}");
                return;
            }

            // initialise after game launches
            new Thread(() =>
            {
                // wait for the game to load up
                while (!Program.ready) Thread.Sleep(1000);

                // Create definition to listen for input
                Log.AsyncY("Initializing Console Input Thread...");
                Program.consoleInputThread = new Thread(Program.ConsoleInputLoop);

                // register help command
                Command.RegisterCommand("help", "Lists all commands | 'help <cmd>' returns command description").CommandFired += Program.help_CommandFired;

                // subscribe to events
                GameEvents.LoadContent += Program.Events_LoadContent;

                // raise game loaded event
                Log.AsyncY("Game Loaded");
                GameEvents.InvokeGameLoaded();

                // listen for command line input
                Log.AsyncY("Type 'help' for help, or 'help <cmd>' for a command's usage");
                Program.consoleInputThread.Start();
                while (Program.ready)
                    Thread.Sleep(1000 / 10); // Check if the game is still running 10 times a second

                // Abort the thread, we're closing
                if (Program.consoleInputThread != null && Program.consoleInputThread.ThreadState == ThreadState.Running)
                    Program.consoleInputThread.Abort();

                Log.AsyncY("Game Execution Finished");
                Log.AsyncY("Shutting Down...");
                Thread.Sleep(100);
                Environment.Exit(0);
            }).Start();

            // start game loop
            Log.AsyncY("Starting SDV...");
            try
            {
                Program.ready = true;
                Program.gamePtr.Run();
            }
            catch (Exception ex)
            {
                Program.ready = false;
                Log.AsyncR($"Game failed to start: {ex}");
            }
        }

        /// <summary>Create a directory path if it doesn't exist.</summary>
        /// <param name="path">The directory path.</param>
        private static void VerifyPath(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Log.AsyncR($"Could not create a path: {path}\n\n{ex}");
            }
        }

        /// <summary>Load and hook up all mods in the mod directory.</summary>
        private static void LoadMods()
        {
            Log.AsyncY("LOADING MODS");
            foreach (string directory in Directory.GetDirectories(Program.ModPath))
            {
                foreach (string manifestPath in Directory.GetFiles(directory, "manifest.json"))
                {
                    if (manifestPath.Contains("StardewInjector"))
                        continue;

                    // read manifest
                    Log.AsyncG($"Found Manifest: {manifestPath}");
                    Manifest manifest = new Manifest();
                    try
                    {
                        // read manifest text
                        string json = File.ReadAllText(manifestPath);
                        if (string.IsNullOrEmpty(json))
                        {
                            Log.AsyncR($"Failed to read mod manifest '{manifestPath}'. Manifest is empty!");
                            continue;
                        }

                        // deserialise manifest
                        manifest = manifest.InitializeConfig(manifestPath);
                        if (string.IsNullOrEmpty(manifest.EntryDll))
                        {
                            Log.AsyncR($"Failed to read mod manifest '{manifestPath}'. EntryDll is empty!");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.AsyncR($"Failed to read mod manifest '{manifestPath}'. Exception details:\n{ex}");
                        continue;
                    }

                    // create per-save directory
                    string targDir = Path.GetDirectoryName(manifestPath);
                    string psDir = Path.Combine(targDir, "psconfigs");
                    Log.AsyncY($"Created psconfigs directory @{psDir}");
                    try
                    {
                        if (manifest.PerSaveConfigs)
                        {
                            if (!Directory.Exists(psDir))
                            {
                                Directory.CreateDirectory(psDir);
                                Log.AsyncY($"Created psconfigs directory @{psDir}");
                            }

                            if (!Directory.Exists(psDir))
                            {
                                Log.AsyncR($"Failed to create psconfigs directory '{psDir}'. No exception occured.");
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.AsyncR($"Failed to create psconfigs directory '{targDir}'. Exception details:\n{ex}");
                        continue;
                    }

                    // load DLL & hook up mod
                    string targDll = string.Empty;
                    try
                    {
                        targDll = Path.Combine(targDir, manifest.EntryDll);
                        if (!File.Exists(targDll))
                        {
                            Log.AsyncR($"Failed to load mod '{manifest.EntryDll}'. File {targDll} does not exist!");
                            continue;
                        }

                        Assembly modAssembly = Assembly.UnsafeLoadFrom(targDll);
                        if (modAssembly.DefinedTypes.Count(x => x.BaseType == typeof(Mod)) > 0)
                        {
                            Log.AsyncY("Loading Mod DLL...");
                            TypeInfo tar = modAssembly.DefinedTypes.First(x => x.BaseType == typeof(Mod));
                            Mod modEntry = (Mod)modAssembly.CreateInstance(tar.ToString());
                            if (modEntry != null)
                            {
                                modEntry.PathOnDisk = targDir;
                                modEntry.Manifest = manifest;
                                Log.AsyncG($"LOADED MOD: {modEntry.Manifest.Name} by {modEntry.Manifest.Author} - Version {modEntry.Manifest.Version} | Description: {modEntry.Manifest.Description} (@ {targDll})");
                                Program.ModsLoaded += 1;
                                modEntry.Entry();
                            }
                        }
                        else
                            Log.AsyncR("Invalid Mod DLL");
                    }
                    catch (Exception ex)
                    {
                        Log.AsyncR($"Failed to load mod '{targDll}'. Exception details:\n{ex}");
                    }
                }
            }

            // print result
            Log.AsyncG($"LOADED {Program.ModsLoaded} MODS");
            Console.Title = Constants.ConsoleTitle;
        }

        /// <summary>Run a loop handling console input.</summary>
        private static void ConsoleInputLoop()
        {
            while (true)
                Command.CallCommand(Console.ReadLine());
        }

        /// <summary>Raised before XNA loads or reloads graphics resources. Called during <see cref="Microsoft.Xna.Framework.Game.LoadContent"/>.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private static void Events_LoadContent(object sender, EventArgs e)
        {
            Log.AsyncY("Initializing Debug Assets...");
            Program.DebugPixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            Program.DebugPixel.SetData(new[] { Color.White });
        }

        /// <summary>The method called when the user submits the help command in the console.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private static void help_CommandFired(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                var command = Command.FindCommand(e.Command.CalledArgs[0]);
                if (command == null)
                    Log.AsyncR("The specified command could not be found");
                else
                    Log.AsyncY(command.CommandArgs.Length > 0 ? $"{command.CommandName}: {command.CommandDesc} - {string.Join(", ", command.CommandArgs)}" : $"{command.CommandName}: {command.CommandDesc}");
            }
            else
                Log.AsyncY("Commands: " + string.Join(", ", Command.RegisteredCommands.Select(x => x.CommandName)));
        }
    }
}
