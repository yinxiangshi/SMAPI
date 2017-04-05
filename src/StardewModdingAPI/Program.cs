using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
#if SMAPI_FOR_WINDOWS
using System.Management;
using System.Windows.Forms;
#endif
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI.AssemblyRewriters;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.Logging;
using StardewModdingAPI.Framework.Models;
using StardewModdingAPI.Framework.Serialisation;
using StardewValley;
using Monitor = StardewModdingAPI.Framework.Monitor;

namespace StardewModdingAPI
{
    /// <summary>The main entry point for SMAPI, responsible for hooking into and launching the game.</summary>
    internal class Program
    {
        /*********
        ** Properties
        *********/
        /// <summary>The log file to which to write messages.</summary>
        private readonly LogFileManager LogFile;

        /// <summary>Manages console output interception.</summary>
        private readonly ConsoleInterceptionManager ConsoleManager = new ConsoleInterceptionManager();

        /// <summary>The core logger for SMAPI.</summary>
        private readonly Monitor Monitor;

        /// <summary>The SMAPI configuration settings.</summary>
        private readonly SConfig Settings;

        /// <summary>Tracks whether the game should exit immediately and any pending initialisation should be cancelled.</summary>
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        /// <summary>Whether the game is currently running.</summary>
        private bool IsGameRunning;

        /// <summary>The underlying game instance.</summary>
        private SGame GameInstance;

        /// <summary>Tracks the installed mods.</summary>
        private readonly ModRegistry ModRegistry;

        /// <summary>Manages deprecation warnings.</summary>
        private readonly DeprecationManager DeprecationManager;

        /// <summary>Manages console commands.</summary>
        private readonly CommandManager CommandManager = new CommandManager();


        /*********
        ** Public methods
        *********/
        /// <summary>The main entry point which hooks into and launches the game.</summary>
        /// <param name="args">The command-line arguments.</param>
        private static void Main(string[] args)
        {
            // get flags from arguments
            bool writeToConsole = !args.Contains("--no-terminal");

            // get log path from arguments
            string logPath = null;
            {
                int pathIndex = Array.LastIndexOf(args, "--log-path") + 1;
                if (pathIndex >= 1 && args.Length >= pathIndex)
                {
                    logPath = args[pathIndex];
                    if (!Path.IsPathRooted(logPath))
                        logPath = Path.Combine(Constants.LogDir, logPath);
                }
            }
            if (string.IsNullOrWhiteSpace(logPath))
                logPath = Constants.DefaultLogPath;

            // load SMAPI
            new Program(writeToConsole, logPath)
                .LaunchInteractively();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="writeToConsole">Whether to output log messages to the console.</param>
        /// <param name="logPath">The full file path to which to write log messages.</param>
        internal Program(bool writeToConsole, string logPath)
        {
            // load settings
            this.Settings = JsonConvert.DeserializeObject<SConfig>(File.ReadAllText(Constants.ApiConfigPath));

            // initialise
            this.LogFile = new LogFileManager(logPath);
            this.Monitor = new Monitor("SMAPI", this.ConsoleManager, this.LogFile, this.ExitGameImmediately) { WriteToConsole = writeToConsole };
            this.ModRegistry = new ModRegistry(this.Settings.ModCompatibility);
            this.DeprecationManager = new DeprecationManager(this.Monitor, this.ModRegistry);
        }

        /// <summary>Launch SMAPI.</summary>
        internal void LaunchInteractively()
        {
            // initialise logging
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB"); // for consistent log formatting
            this.Monitor.Log($"SMAPI {Constants.ApiVersion} with Stardew Valley {Constants.GetGameDisplayVersion(Constants.GameVersion)} on {this.GetFriendlyPlatformName()}", LogLevel.Info);
            Console.Title = $"SMAPI {Constants.ApiVersion} - running Stardew Valley {Constants.GetGameDisplayVersion(Constants.GameVersion)}";

            // inject compatibility shims
#pragma warning disable 618
            Command.Shim(this.CommandManager, this.DeprecationManager, this.ModRegistry);
            Config.Shim(this.DeprecationManager);
            InternalExtensions.Shim(this.ModRegistry);
            Log.Shim(this.DeprecationManager, this.GetSecondaryMonitor("legacy mod"), this.ModRegistry);
            Mod.Shim(this.DeprecationManager);
            ContentEvents.Shim(this.ModRegistry, this.Monitor);
            PlayerEvents.Shim(this.DeprecationManager);
            TimeEvents.Shim(this.DeprecationManager);
#pragma warning restore 618

            // redirect direct console output
            {
                Monitor monitor = this.GetSecondaryMonitor("Console.Out");
                monitor.WriteToFile = false; // not useful for troubleshooting mods per discussion
                if (monitor.WriteToConsole)
                    this.ConsoleManager.OnLineIntercepted += line => monitor.Log(line, LogLevel.Trace);
            }

            // add warning headers
            if (this.Settings.DeveloperMode)
            {
                this.Monitor.ShowTraceInConsole = true;
                this.Monitor.Log($"You configured SMAPI to run in developer mode. The console may be much more verbose. You can disable developer mode by installing the non-developer version of SMAPI, or by editing {Constants.ApiConfigPath}.", LogLevel.Warn);
            }
            if (!this.Settings.CheckForUpdates)
                this.Monitor.Log($"You configured SMAPI to not check for updates. Running an old version of SMAPI is not recommended. You can enable update checks by reinstalling SMAPI or editing {Constants.ApiConfigPath}.", LogLevel.Warn);
            if (!this.Monitor.WriteToConsole)
                this.Monitor.Log("Writing to the terminal is disabled because the --no-terminal argument was received. This usually means launching the terminal failed.", LogLevel.Warn);

            // print file paths
            this.Monitor.Log($"Mods go here: {Constants.ModPath}");

            // hook into & launch the game
            try
            {
                // verify version
                if (Constants.GameVersion.IsOlderThan(Constants.MinimumGameVersion))
                {
                    this.Monitor.Log($"Oops! You're running Stardew Valley {Constants.GetGameDisplayVersion(Constants.GameVersion)}, but the oldest supported version is {Constants.GetGameDisplayVersion(Constants.MinimumGameVersion)}. Please update your game before using SMAPI. If you have the beta version on Steam, you may need to opt out to get the latest non-beta updates.", LogLevel.Error);
                    this.PressAnyKeyToExit();
                    return;
                }
                if (Constants.MaximumGameVersion != null && Constants.GameVersion.IsNewerThan(Constants.MaximumGameVersion))
                {
                    this.Monitor.Log($"Oops! You're running Stardew Valley {Constants.GetGameDisplayVersion(Constants.GameVersion)}, but this version of SMAPI is only compatible up to Stardew Valley {Constants.GetGameDisplayVersion(Constants.MaximumGameVersion)}. Please check for a newer version of SMAPI.", LogLevel.Error);
                    this.PressAnyKeyToExit();
                    return;
                }

                // initialise folders
                this.Monitor.Log("Loading SMAPI...");
                this.VerifyPath(Constants.ModPath);
                this.VerifyPath(Constants.LogDir);

                // check for update when game loads
                if (this.Settings.CheckForUpdates)
                    GameEvents.GameLoaded += (sender, e) => this.CheckForUpdateAsync();

                // launch game
                this.StartGame();
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Critical error: {ex.GetLogSummary()}", LogLevel.Error);
            }
            this.PressAnyKeyToExit();
        }

        /// <summary>Immediately exit the game without saving. This should only be invoked when an irrecoverable fatal error happens that risks save corruption or game-breaking bugs.</summary>
        /// <param name="module">The module which requested an immediate exit.</param>
        /// <param name="reason">The reason provided for the shutdown.</param>
        internal void ExitGameImmediately(string module, string reason)
        {
            this.Monitor.LogFatal($"{module} requested an immediate game shutdown: {reason}");
            this.CancellationTokenSource.Cancel();
            if (this.IsGameRunning)
            {
                this.GameInstance.Exiting += (sender, e) => this.PressAnyKeyToExit();
                this.GameInstance.Exit();
            }
        }

        /// <summary>Get a monitor for legacy code which doesn't have one passed in.</summary>
        [Obsolete("This method should only be used when needed for backwards compatibility.")]
        internal IMonitor GetLegacyMonitorForMod()
        {
            string modName = this.ModRegistry.GetModFromStack() ?? "unknown";
            return this.GetSecondaryMonitor(modName);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Asynchronously check for a new version of SMAPI, and print a message to the console if an update is available.</summary>
        private void CheckForUpdateAsync()
        {
            new Thread(() =>
            {
                try
                {
                    GitRelease release = UpdateHelper.GetLatestVersionAsync(Constants.GitHubRepository).Result;
                    ISemanticVersion latestVersion = new SemanticVersion(release.Tag);
                    if (latestVersion.IsNewerThan(Constants.ApiVersion))
                        this.Monitor.Log($"You can update SMAPI from version {Constants.ApiVersion} to {latestVersion}", LogLevel.Alert);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Couldn't check for a new version of SMAPI. This won't affect your game, but you may not be notified of new versions if this keeps happening.\n{ex.GetLogSummary()}");
                }
            }).Start();
        }

        /// <summary>Hook into Stardew Valley and launch the game.</summary>
        private void StartGame()
        {
            try
            {
                this.Monitor.Log("Loading game...");

                // add error handlers
#if SMAPI_FOR_WINDOWS
                Application.ThreadException += (sender, e) => this.Monitor.Log($"Critical thread exception: {e.Exception.GetLogSummary()}", LogLevel.Error);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
#endif
                AppDomain.CurrentDomain.UnhandledException += (sender, e) => this.Monitor.Log($"Critical app domain exception: {e.ExceptionObject}", LogLevel.Error);

                // override Game1 instance
                this.GameInstance = new SGame(this.Monitor);
                this.GameInstance.Exiting += (sender, e) => this.IsGameRunning = false;
                this.GameInstance.Window.ClientSizeChanged += (sender, e) => GraphicsEvents.InvokeResize(this.Monitor, sender, e);
                this.GameInstance.Window.Title = $"Stardew Valley {Constants.GameVersion} with SMAPI {Constants.ApiVersion}";
                {
                    Type type = typeof(Game1).Assembly.GetType("StardewValley.Program", true);
                    type.GetField("gamePtr").SetValue(null, this.GameInstance);
                }

                // configure
                Game1.graphics.GraphicsProfile = GraphicsProfile.HiDef;

                // load mods
                this.LoadMods();
                if (this.CancellationTokenSource.IsCancellationRequested)
                {
                    this.Monitor.Log("Shutdown requested; interrupting initialisation.", LogLevel.Error);
                    return;
                }

                // initialise console after game launches
                new Thread(() =>
                {
                    // wait for the game to load up
                    while (!this.IsGameRunning)
                        Thread.Sleep(1000);

                    // register help command
                    this.CommandManager.Add("SMAPI", "help", "Lists all commands | 'help <cmd>' returns command description", this.HandleHelpCommand);

                    // listen for command line input
                    this.Monitor.Log("Starting console...");
                    this.Monitor.Log("Type 'help' for help, or 'help <cmd>' for a command's usage", LogLevel.Info);
                    Thread consoleInputThread = new Thread(this.ConsoleInputLoop);
                    consoleInputThread.Start();
                    while (this.IsGameRunning)
                        Thread.Sleep(1000 / 10); // Check if the game is still running 10 times a second

                    // abort the console thread, we're closing
                    if (consoleInputThread.ThreadState == ThreadState.Running)
                        consoleInputThread.Abort();
                }).Start();

                // start game loop
                this.Monitor.Log("Starting game...");
                if (this.CancellationTokenSource.IsCancellationRequested)
                {
                    this.Monitor.Log("Shutdown requested; interrupting initialisation.", LogLevel.Error);
                    return;
                }
                try
                {
                    this.IsGameRunning = true;
                    this.GameInstance.Run();
                }
                finally
                {
                    this.IsGameRunning = false;
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"The game encountered a fatal error:\n{ex.GetLogSummary()}", LogLevel.Error);
            }
        }

        /// <summary>Create a directory path if it doesn't exist.</summary>
        /// <param name="path">The directory path.</param>
        private void VerifyPath(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Couldn't create a path: {path}\n\n{ex.GetLogSummary()}", LogLevel.Error);
            }
        }

        /// <summary>Load and hook up all mods in the mod directory.</summary>
        private void LoadMods()
        {
            this.Monitor.Log("Loading mods...");

            // get JSON helper
            JsonHelper jsonHelper = new JsonHelper();

            // get assembly loader
            AssemblyLoader modAssemblyLoader = new AssemblyLoader(Constants.TargetPlatform, this.Monitor);
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => modAssemblyLoader.ResolveAssembly(e.Name);

            // load mod assemblies
            int modsLoaded = 0;
            List<Action> deprecationWarnings = new List<Action>(); // queue up deprecation warnings to show after mod list
            foreach (string directoryPath in Directory.GetDirectories(Constants.ModPath))
            {
                // passthrough empty directories
                DirectoryInfo directory = new DirectoryInfo(directoryPath);
                while (!directory.GetFiles().Any() && directory.GetDirectories().Length == 1)
                    directory = directory.GetDirectories().First();

                // check for cancellation
                if (this.CancellationTokenSource.IsCancellationRequested)
                {
                    this.Monitor.Log("Shutdown requested; interrupting mod loading.", LogLevel.Error);
                    return;
                }

                // get manifest path
                string manifestPath = Path.Combine(directory.FullName, "manifest.json");
                if (!File.Exists(manifestPath))
                {
                    this.Monitor.Log($"Ignored folder \"{directory.Name}\" which doesn't have a manifest.json.", LogLevel.Warn);
                    continue;
                }
                string skippedPrefix = $"Skipped {manifestPath.Replace(Constants.ModPath, "").Trim('/', '\\')}";

                // read manifest
                Manifest manifest;
                try
                {
                    // read manifest text
                    string json = File.ReadAllText(manifestPath);
                    if (string.IsNullOrEmpty(json))
                    {
                        this.Monitor.Log($"{skippedPrefix} because the manifest is empty.", LogLevel.Error);
                        continue;
                    }

                    // deserialise manifest
                    manifest = jsonHelper.ReadJsonFile<Manifest>(Path.Combine(directory.FullName, "manifest.json"));
                    if (manifest == null)
                    {
                        this.Monitor.Log($"{skippedPrefix} because its manifest is invalid.", LogLevel.Error);
                        continue;
                    }
                    if (string.IsNullOrEmpty(manifest.EntryDll))
                    {
                        this.Monitor.Log($"{skippedPrefix} because its manifest doesn't specify an entry DLL.", LogLevel.Error);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"{skippedPrefix} because manifest parsing failed.\n{ex.GetLogSummary()}", LogLevel.Error);
                    continue;
                }
                if (!string.IsNullOrWhiteSpace(manifest.Name))
                    skippedPrefix = $"Skipped {manifest.Name}";

                // validate compatibility
                ModCompatibility compatibility = this.ModRegistry.GetCompatibilityRecord(manifest);
                if (compatibility?.Compatibility == ModCompatibilityType.AssumeBroken)
                {
                    bool hasOfficialUrl = !string.IsNullOrWhiteSpace(compatibility.UpdateUrl);
                    bool hasUnofficialUrl = !string.IsNullOrWhiteSpace(compatibility.UnofficialUpdateUrl);

                    string reasonPhrase = compatibility.ReasonPhrase ?? "it's not compatible with the latest version of the game";
                    string warning = $"{skippedPrefix} because {reasonPhrase}. Please check for a version newer than {compatibility.UpperVersion} here:";
                    if (hasOfficialUrl)
                        warning += !hasUnofficialUrl ? $" {compatibility.UpdateUrl}" : $"{Environment.NewLine}- official mod: {compatibility.UpdateUrl}";
                    if (hasUnofficialUrl)
                        warning += $"{Environment.NewLine}- unofficial update: {compatibility.UnofficialUpdateUrl}";

                    this.Monitor.Log(warning, LogLevel.Error);
                    continue;
                }

                // validate SMAPI version
                if (!string.IsNullOrWhiteSpace(manifest.MinimumApiVersion))
                {
                    try
                    {
                        ISemanticVersion minVersion = new SemanticVersion(manifest.MinimumApiVersion);
                        if (minVersion.IsNewerThan(Constants.ApiVersion))
                        {
                            this.Monitor.Log($"{skippedPrefix} because it needs SMAPI {minVersion} or later. Please update SMAPI to the latest version to use this mod.", LogLevel.Error);
                            continue;
                        }
                    }
                    catch (FormatException ex) when (ex.Message.Contains("not a valid semantic version"))
                    {
                        this.Monitor.Log($"{skippedPrefix} because it has an invalid minimum SMAPI version '{manifest.MinimumApiVersion}'. This should be a semantic version number like {Constants.ApiVersion}.", LogLevel.Error);
                        continue;
                    }
                }

                // create per-save directory
                if (manifest.PerSaveConfigs)
                {
                    deprecationWarnings.Add(() => this.DeprecationManager.Warn(manifest.Name, $"{nameof(Manifest)}.{nameof(Manifest.PerSaveConfigs)}", "1.0", DeprecationLevel.Info));
                    try
                    {
                        string psDir = Path.Combine(directory.FullName, "psconfigs");
                        Directory.CreateDirectory(psDir);
                        if (!Directory.Exists(psDir))
                        {
                            this.Monitor.Log($"{skippedPrefix} because it requires per-save configuration files ('psconfigs') which couldn't be created for some reason.", LogLevel.Error);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log($"{skippedPrefix} because it requires per-save configuration files ('psconfigs') which couldn't be created:\n{ex.GetLogSummary()}", LogLevel.Error);
                        continue;
                    }
                }

                // validate mod path to simplify errors
                string assemblyPath = Path.Combine(directory.FullName, manifest.EntryDll);
                if (!File.Exists(assemblyPath))
                {
                    this.Monitor.Log($"{skippedPrefix} because its DLL '{manifest.EntryDll}' doesn't exist.", LogLevel.Error);
                    continue;
                }

                // preprocess & load mod assembly
                Assembly modAssembly;
                try
                {
                    modAssembly = modAssemblyLoader.Load(assemblyPath, assumeCompatible: compatibility?.Compatibility == ModCompatibilityType.AssumeCompatible);
                }
                catch (IncompatibleInstructionException ex)
                {
                    this.Monitor.Log($"{skippedPrefix} because it's not compatible with the latest version of the game (detected {ex.NounPhrase}). Please check for a newer version of the mod (you have v{manifest.Version}).", LogLevel.Error);
                    continue;
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"{skippedPrefix} because its DLL '{manifest.EntryDll}' couldn't be loaded.\n{ex.GetLogSummary()}", LogLevel.Error);
                    continue;
                }

                // validate assembly
                try
                {
                    int modEntries = modAssembly.DefinedTypes.Count(type => typeof(Mod).IsAssignableFrom(type) && !type.IsAbstract);
                    if (modEntries == 0)
                    {
                        this.Monitor.Log($"{skippedPrefix} because its DLL has no '{nameof(Mod)}' subclass.", LogLevel.Error);
                        continue;
                    }
                    if (modEntries > 1)
                    {
                        this.Monitor.Log($"{skippedPrefix} because its DLL contains multiple '{nameof(Mod)}' subclasses.", LogLevel.Error);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"{skippedPrefix} because its DLL couldn't be loaded.\n{ex.GetLogSummary()}", LogLevel.Error);
                    continue;
                }

                // initialise mod
                try
                {
                    // get implementation
                    TypeInfo modEntryType = modAssembly.DefinedTypes.First(type => typeof(Mod).IsAssignableFrom(type) && !type.IsAbstract);
                    Mod mod = (Mod)modAssembly.CreateInstance(modEntryType.ToString());
                    if (mod == null)
                    {
                        this.Monitor.Log($"{skippedPrefix} because its entry class couldn't be instantiated.");
                        continue;
                    }

                    // inject data
                    // get helper
                    mod.ModManifest = manifest;
                    mod.Helper = new ModHelper(manifest.Name, directory.FullName, jsonHelper, this.ModRegistry, this.CommandManager);
                    mod.Monitor = this.GetSecondaryMonitor(manifest.Name);
                    mod.PathOnDisk = directory.FullName;

                    // track mod
                    this.ModRegistry.Add(mod);
                    modsLoaded += 1;
                    this.Monitor.Log($"Loaded {manifest.Name} by {manifest.Author}, v{manifest.Version} | {manifest.Description}", LogLevel.Info);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"{skippedPrefix} because initialisation failed:\n{ex.GetLogSummary()}", LogLevel.Error);
                }
            }

            // initialise mods
            foreach (Mod mod in this.ModRegistry.GetMods())
            {
                try
                {
                    // call entry methods
                    mod.Entry(); // deprecated since 1.0
                    mod.Entry(mod.Helper);

                    // raise deprecation warning for old Entry() methods
                    if (this.DeprecationManager.IsVirtualMethodImplemented(mod.GetType(), typeof(Mod), nameof(Mod.Entry), new[] { typeof(object[]) }))
                        deprecationWarnings.Add(() => this.DeprecationManager.Warn(mod.ModManifest.Name, $"{nameof(Mod)}.{nameof(Mod.Entry)}(object[]) instead of {nameof(Mod)}.{nameof(Mod.Entry)}({nameof(IModHelper)})", "1.0", DeprecationLevel.Info));
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"The {mod.ModManifest.Name} mod failed on entry initialisation. It will still be loaded, but may not function correctly.\n{ex.GetLogSummary()}", LogLevel.Warn);
                }
            }

            // print result
            this.Monitor.Log($"Loaded {modsLoaded} mods.");
            foreach (Action warning in deprecationWarnings)
                warning();
            Console.Title = $"SMAPI {Constants.ApiVersion} - running Stardew Valley {Constants.GetGameDisplayVersion(Constants.GameVersion)} with {modsLoaded} mods";
        }

        /// <summary>Run a loop handling console input.</summary>
        [SuppressMessage("ReSharper", "FunctionNeverReturns", Justification = "The thread is aborted when the game exits.")]
        private void ConsoleInputLoop()
        {
            while (true)
            {
                string input = Console.ReadLine();
                try
                {
                    if (!string.IsNullOrWhiteSpace(input) && !this.CommandManager.Trigger(input))
                        this.Monitor.Log("Unknown command; type 'help' for a list of available commands.", LogLevel.Error);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"The handler registered for that command failed:\n{ex.GetLogSummary()}", LogLevel.Error);
                }
            }
        }

        /// <summary>The method called when the user submits the help command in the console.</summary>
        /// <param name="name">The command name.</param>
        /// <param name="arguments">The command arguments.</param>
        private void HandleHelpCommand(string name, string[] arguments)
        {
            if (arguments.Any())
            {
                Framework.Command result = this.CommandManager.Get(arguments[0]);
                if (result == null)
                    this.Monitor.Log("There's no command with that name.", LogLevel.Error);
                else
                    this.Monitor.Log($"{result.Name}: {result.Documentation}\n(Added by {result.ModName}.)", LogLevel.Info);
            }
            else
            {
                this.Monitor.Log("The following commands are registered: " + string.Join(", ", this.CommandManager.GetAll().Select(p => p.Name)) + ".", LogLevel.Info);
                this.Monitor.Log("For more information about a command, type 'help command_name'.", LogLevel.Info);
            }
        }

        /// <summary>Show a 'press any key to exit' message, and exit when they press a key.</summary>
        private void PressAnyKeyToExit()
        {
            this.Monitor.Log("Game has ended. Press any key to exit.", LogLevel.Info);
            Thread.Sleep(100);
            Console.ReadKey();
            Environment.Exit(0);
        }

        /// <summary>Get a monitor instance derived from SMAPI's current settings.</summary>
        /// <param name="name">The name of the module which will log messages with this instance.</param>
        private Monitor GetSecondaryMonitor(string name)
        {
            return new Monitor(name, this.ConsoleManager, this.LogFile, this.ExitGameImmediately) { WriteToConsole = this.Monitor.WriteToConsole, ShowTraceInConsole = this.Settings.DeveloperMode };
        }

        /// <summary>Get a human-readable name for the current platform.</summary>
        [SuppressMessage("ReSharper", "EmptyGeneralCatchClause", Justification = "Error suppressed deliberately to fallback to default behaviour.")]
        private string GetFriendlyPlatformName()
        {
#if SMAPI_FOR_WINDOWS
            try
            {
                return new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem")
                    .Get()
                    .Cast<ManagementObject>()
                    .Select(entry => entry.GetPropertyValue("Caption").ToString())
                    .FirstOrDefault();
            }
            catch { }
#endif
            return Environment.OSVersion.ToString();
        }
    }
}
