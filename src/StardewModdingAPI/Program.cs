using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
#if SMAPI_FOR_WINDOWS
using System.Windows.Forms;
#endif
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
                Log.SyncColour($"Launching SMAPI {Constants.Version} with Stardew Valley {Game1.version} on {Environment.OSVersion}", ConsoleColor.DarkGray); // make sure this is the first line, to simplify troubleshooting instructions
                Log.Debug("Initialising...");

                // set window title
                Console.Title = Constants.ConsoleTitle;

                // verify paths
                Program.VerifyPath(Program.ModPath);
                Program.VerifyPath(Constants.LogDir);
                if (!File.Exists(Program.GameExecutablePath))
                {
                    Log.Error($"Couldn't find executable: {Program.GameExecutablePath}");
                    Log.Debug("Shutting down. Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                // check for update
                Program.CheckForUpdateAsync();

                // launch game
                Program.StartGame();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadKey();
                Log.Error($"Critical error: {ex}");
            }

            // print message when game ends
            Log.Info("Shutting down. Press any key to continue...");
            Console.ReadKey();
        }


        /*********
        ** Private methods
        *********/
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
            Log.Debug("Preparing game...");
            Program.StardewAssembly = Assembly.UnsafeLoadFrom(Program.GameExecutablePath);
            Program.StardewProgramType = Program.StardewAssembly.GetType("StardewValley.Program", true);
            Program.StardewGameInfo = Program.StardewProgramType.GetField("gamePtr");

            // change the game's version
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
                Log.Debug("Patching game...");
                Program.gamePtr = new SGame();

                // hook events
                Program.gamePtr.Exiting += (sender, e) => Program.ready = false;
                Program.gamePtr.Window.ClientSizeChanged += GraphicsEvents.InvokeResize;

                // patch graphics
                Game1.graphics.GraphicsProfile = GraphicsProfile.HiDef;

                // load mods
                Program.LoadMods();

                // initialise
                Log.Debug("Tweaking game...");
                Program.StardewGameInfo.SetValue(Program.StardewProgramType, Program.gamePtr);
                Program.gamePtr.IsMouseVisible = false;
                Program.gamePtr.Window.Title = $"Stardew Valley - Version {Game1.version}";
            }
            catch (Exception ex)
            {
                Log.Error($"Game failed to initialise: {ex}");
                return;
            }

            // initialise after game launches
            new Thread(() =>
            {
                // wait for the game to load up
                while (!Program.ready) Thread.Sleep(1000);

                // register help command
                Command.RegisterCommand("help", "Lists all commands | 'help <cmd>' returns command description").CommandFired += Program.help_CommandFired;

                // raise game loaded event
                GameEvents.InvokeGameLoaded();

                // listen for command line input
                Log.Debug("Starting console...");
                Log.Info("Type 'help' for help, or 'help <cmd>' for a command's usage");
                Thread consoleInputThread = new Thread(Program.ConsoleInputLoop);
                consoleInputThread.Start();
                while (Program.ready)
                    Thread.Sleep(1000 / 10); // Check if the game is still running 10 times a second

                // abort the console thread, we're closing
                if (consoleInputThread.ThreadState == ThreadState.Running)
                    consoleInputThread.Abort();

                Log.Debug("Game has ended. Shutting down...");
                Thread.Sleep(100);
                Environment.Exit(0);
            }).Start();

            // start game loop
            Log.Debug("Starting Stardew Valley...");
            try
            {
                Program.ready = true;
                Program.gamePtr.Run();
            }
            catch (Exception ex)
            {
                Program.ready = false;
                Log.Error($"Game failed to start: {ex}");
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
                Log.Error($"Couldn't create a path: {path}\n\n{ex}");
            }
        }

        /// <summary>Load and hook up all mods in the mod directory.</summary>
        private static void LoadMods()
        {
            Log.Debug("Loading mods...");
            foreach (string directory in Directory.GetDirectories(Program.ModPath))
            {
                foreach (string manifestPath in Directory.GetFiles(directory, "manifest.json"))
                {
                    // error format
                    string errorPrefix = $"Couldn't load mod for manifest '{manifestPath}'";

                    // read manifest
                    Manifest manifest = new Manifest();
                    try
                    {
                        // read manifest text
                        string json = File.ReadAllText(manifestPath);
                        if (string.IsNullOrEmpty(json))
                        {
                            Log.Error($"{errorPrefix}: manifest is empty.");
                            continue;
                        }

                        // deserialise manifest
                        manifest = manifest.InitializeConfig(manifestPath);
                        if (string.IsNullOrEmpty(manifest.EntryDll))
                        {
                            Log.Error($"{errorPrefix}: manifest doesn't specify an entry DLL.");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"{errorPrefix}: manifest parsing failed.\n{ex}");
                        continue;
                    }

                    // create per-save directory
                    string targDir = Path.GetDirectoryName(manifestPath);
                    string psDir = Path.Combine(targDir, "psconfigs");
                    try
                    {
                        if (manifest.PerSaveConfigs)
                        {
                            Directory.CreateDirectory(psDir);
                            if (!Directory.Exists(psDir))
                            {
                                Log.Error($"{errorPrefix}: couldn't create the per-save configuration directory ('psconfigs') requested by this mod. The failure reason is unknown.");
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"{errorPrefix}: couldm't create the per-save configuration directory ('psconfigs') requested by this mod.\n{ex}");
                        continue;
                    }

                    // load DLL & hook up mod
                    string targDll = string.Empty;
                    try
                    {
                        targDll = Path.Combine(targDir, manifest.EntryDll);
                        if (!File.Exists(targDll))
                        {
                            Log.Error($"{errorPrefix}: target DLL '{targDll}' does not exist.");
                            continue;
                        }

                        Assembly modAssembly = Assembly.UnsafeLoadFrom(targDll);
                        if (modAssembly.DefinedTypes.Count(x => x.BaseType == typeof(Mod)) > 0)
                        {
                            TypeInfo tar = modAssembly.DefinedTypes.First(x => x.BaseType == typeof(Mod));
                            Mod modEntry = (Mod)modAssembly.CreateInstance(tar.ToString());
                            if (modEntry != null)
                            {
                                modEntry.PathOnDisk = targDir;
                                modEntry.Manifest = manifest;
                                Log.Info($"Loaded mod: {modEntry.Manifest.Name} by {modEntry.Manifest.Author}, v{modEntry.Manifest.Version} | {modEntry.Manifest.Description}\n@ {targDll}");
                                Program.ModsLoaded += 1;
                                modEntry.Entry();
                            }
                        }
                        else
                            Log.Error($"{errorPrefix}: the mod DLL does not contain an implementation of the 'Mod' class.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"{errorPrefix}: an error occurred while loading the target DLL.\n{ex}");
                    }
                }
            }

            // print result
            Log.Debug($"Loaded {Program.ModsLoaded} mods.");
            Console.Title = Constants.ConsoleTitle;
        }

        /// <summary>Run a loop handling console input.</summary>
        private static void ConsoleInputLoop()
        {
            while (true)
                Command.CallCommand(Console.ReadLine());
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
                    Log.Error("The specified command could't be found");
                else
                    Log.Info(command.CommandArgs.Length > 0 ? $"{command.CommandName}: {command.CommandDesc} - {string.Join(", ", command.CommandArgs)}" : $"{command.CommandName}: {command.CommandDesc}");
            }
            else
                Log.Info("Commands: " + string.Join(", ", Command.RegisteredCommands.Select(x => x.CommandName)));
        }
    }
}
