using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Threading;
#if SMAPI_FOR_WINDOWS
using System.Windows.Forms;
#endif
using Newtonsoft.Json;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework.Events;
using StardewModdingAPI.Framework.Exceptions;
using StardewModdingAPI.Framework.Logging;
using StardewModdingAPI.Framework.Models;
using StardewModdingAPI.Framework.ModHelpers;
using StardewModdingAPI.Framework.ModLoading;
using StardewModdingAPI.Framework.Patching;
using StardewModdingAPI.Framework.PerformanceMonitoring;
using StardewModdingAPI.Framework.Reflection;
using StardewModdingAPI.Framework.Serialization;
using StardewModdingAPI.Patches;
using StardewModdingAPI.Toolkit;
using StardewModdingAPI.Toolkit.Framework.Clients.WebApi;
using StardewModdingAPI.Toolkit.Framework.ModData;
using StardewModdingAPI.Toolkit.Serialization;
using StardewModdingAPI.Toolkit.Utilities;
using StardewModdingAPI.Utilities;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewModdingAPI.Framework
{
    /// <summary>The core class which initializes and manages SMAPI.</summary>
    internal class SCore : IDisposable
    {
        /*********
        ** Fields
        *********/
        /// <summary>Manages the SMAPI console window and log file.</summary>
        private readonly LogManager LogManager;

        /// <summary>The core logger and monitor for SMAPI.</summary>
        private Monitor Monitor => this.LogManager.Monitor;

        /// <summary>Tracks whether the game should exit immediately and any pending initialization should be cancelled.</summary>
        private readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();

        /// <summary>Simplifies access to private game code.</summary>
        private readonly Reflector Reflection = new Reflector();

        /// <summary>Encapsulates access to SMAPI core translations.</summary>
        private readonly Translator Translator = new Translator();

        /// <summary>The SMAPI configuration settings.</summary>
        private readonly SConfig Settings;

        /// <summary>The underlying game instance.</summary>
        private SGame GameInstance;

        /// <summary>The underlying content manager.</summary>
        private ContentCoordinator ContentCore => this.GameInstance.ContentCore;

        /// <summary>Tracks the installed mods.</summary>
        /// <remarks>This is initialized after the game starts.</remarks>
        private readonly ModRegistry ModRegistry = new ModRegistry();

        /// <summary>Manages SMAPI events for mods.</summary>
        private readonly EventManager EventManager;

        /// <summary>Whether the game is currently running.</summary>
        private bool IsGameRunning;

        /// <summary>Whether the program has been disposed.</summary>
        private bool IsDisposed;

        /// <summary>The mod toolkit used for generic mod interactions.</summary>
        private readonly ModToolkit Toolkit = new ModToolkit();

        /// <summary>The path to search for mods.</summary>
        private string ModsPath => Constants.ModsPath;


        /*********
        ** Accessors
        *********/
        /// <summary>Manages deprecation warnings.</summary>
        /// <remarks>This is initialized after the game starts. This is accessed directly because it's not part of the normal class model.</remarks>
        internal static DeprecationManager DeprecationManager { get; private set; }

        /// <summary>Manages performance counters.</summary>
        /// <remarks>This is initialized after the game starts. This is non-private for use by Console Commands.</remarks>
        internal static PerformanceMonitor PerformanceMonitor { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modsPath">The path to search for mods.</param>
        /// <param name="writeToConsole">Whether to output log messages to the console.</param>
        public SCore(string modsPath, bool writeToConsole)
        {
            // init paths
            this.VerifyPath(modsPath);
            this.VerifyPath(Constants.LogDir);
            Constants.ModsPath = modsPath;

            // init log file
            this.PurgeNormalLogs();
            string logPath = this.GetLogPath();

            // init basics
            this.Settings = JsonConvert.DeserializeObject<SConfig>(File.ReadAllText(Constants.ApiConfigPath));
            if (File.Exists(Constants.ApiUserConfigPath))
                JsonConvert.PopulateObject(File.ReadAllText(Constants.ApiUserConfigPath), this.Settings);

            this.LogManager = new LogManager(logPath: logPath, colorConfig: this.Settings.ConsoleColors, writeToConsole: writeToConsole, isVerbose: this.Settings.VerboseLogging, isDeveloperMode: this.Settings.DeveloperMode);

            SCore.PerformanceMonitor = new PerformanceMonitor(this.Monitor);
            this.EventManager = new EventManager(this.ModRegistry, SCore.PerformanceMonitor);
            SCore.PerformanceMonitor.InitializePerformanceCounterCollections(this.EventManager);

            SCore.DeprecationManager = new DeprecationManager(this.Monitor, this.ModRegistry);

            SDate.Translations = this.Translator;

            // log SMAPI/OS info
            this.LogManager.LogIntro(modsPath, this.Settings.GetCustomSettings());

            // validate platform
#if SMAPI_FOR_WINDOWS
            if (Constants.Platform != Platform.Windows)
            {
                this.Monitor.Log("Oops! You're running Windows, but this version of SMAPI is for Linux or Mac. Please reinstall SMAPI to fix this.", LogLevel.Error);
                this.LogManager.PressAnyKeyToExit();
            }
#else
            if (Constants.Platform == Platform.Windows)
            {
                this.Monitor.Log($"Oops! You're running {Constants.Platform}, but this version of SMAPI is for Windows. Please reinstall SMAPI to fix this.", LogLevel.Error);
                this.PressAnyKeyToExit();
            }
#endif
        }

        /// <summary>Launch SMAPI.</summary>
        [HandleProcessCorruptedStateExceptions, SecurityCritical] // let try..catch handle corrupted state exceptions
        public void RunInteractively()
        {
            // initialize SMAPI
            try
            {
                JsonConverter[] converters = {
                    new ColorConverter(),
                    new PointConverter(),
                    new Vector2Converter(),
                    new RectangleConverter()
                };
                foreach (JsonConverter converter in converters)
                    this.Toolkit.JsonHelper.JsonSettings.Converters.Add(converter);

                // add error handlers
#if SMAPI_FOR_WINDOWS
                Application.ThreadException += (sender, e) => this.Monitor.Log($"Critical thread exception: {e.Exception.GetLogSummary()}", LogLevel.Error);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
#endif
                AppDomain.CurrentDomain.UnhandledException += (sender, e) => this.Monitor.Log($"Critical app domain exception: {e.ExceptionObject}", LogLevel.Error);

                // add more lenient assembly resolver
                AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => AssemblyLoader.ResolveAssembly(e.Name);

                // hook locale event
                LocalizedContentManager.OnLanguageChange += locale => this.OnLocaleChanged();

                // override game
                SGame.ConstructorHack = new SGameConstructorHack(this.Monitor, this.Reflection, this.Toolkit.JsonHelper, this.InitializeBeforeFirstAssetLoaded);
                this.GameInstance = new SGame(
                    monitor: this.Monitor,
                    monitorForGame: this.LogManager.MonitorForGame,
                    reflection: this.Reflection,
                    translator: this.Translator,
                    eventManager: this.EventManager,
                    jsonHelper: this.Toolkit.JsonHelper,
                    modRegistry: this.ModRegistry,
                    deprecationManager: SCore.DeprecationManager,
                    performanceMonitor: SCore.PerformanceMonitor,
                    onGameInitialized: this.InitializeAfterGameStart,
                    onGameExiting: this.Dispose,
                    cancellationToken: this.CancellationToken,
                    logNetworkTraffic: this.Settings.LogNetworkTraffic
                );
                this.Translator.SetLocale(this.GameInstance.ContentCore.GetLocale(), this.GameInstance.ContentCore.Language);
                StardewValley.Program.gamePtr = this.GameInstance;

                // apply game patches
                new GamePatcher(this.Monitor).Apply(
                    new EventErrorPatch(this.LogManager.MonitorForGame),
                    new DialogueErrorPatch(this.LogManager.MonitorForGame, this.Reflection),
                    new ObjectErrorPatch(),
                    new LoadContextPatch(this.Reflection, this.GameInstance.OnLoadStageChanged),
                    new LoadErrorPatch(this.Monitor, this.GameInstance.OnSaveContentRemoved),
                    new ScheduleErrorPatch(this.LogManager.MonitorForGame)
                );

                // add exit handler
                new Thread(() =>
                {
                    this.CancellationToken.Token.WaitHandle.WaitOne();
                    if (this.IsGameRunning)
                    {
                        this.LogManager.WriteCrashLog();
                        this.GameInstance.Exit();
                    }
                }).Start();

                // set window titles
                this.GameInstance.Window.Title = $"Stardew Valley {Constants.GameVersion} - running SMAPI {Constants.ApiVersion}";
                this.LogManager.SetConsoleTitle($"SMAPI {Constants.ApiVersion} - running Stardew Valley {Constants.GameVersion}");
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"SMAPI failed to initialize: {ex.GetLogSummary()}", LogLevel.Error);
                this.LogManager.PressAnyKeyToExit();
                return;
            }

            // log basic info
            this.LogManager.HandleMarkerFiles();
            this.LogManager.LogSettingsHeader(this.Settings.DeveloperMode, this.Settings.CheckForUpdates);

            // set window titles
            this.GameInstance.Window.Title = $"Stardew Valley {Constants.GameVersion} - running SMAPI {Constants.ApiVersion}";
            this.LogManager.SetConsoleTitle($"SMAPI {Constants.ApiVersion} - running Stardew Valley {Constants.GameVersion}");

            // start game
            this.Monitor.Log("Starting game...", LogLevel.Debug);
            try
            {
                this.IsGameRunning = true;
                StardewValley.Program.releaseBuild = true; // game's debug logic interferes with SMAPI opening the game window
                this.GameInstance.Run();
            }
            catch (Exception ex)
            {
                this.LogManager.LogFatalLaunchError(ex);
                this.LogManager.PressAnyKeyToExit();
            }
            finally
            {
                this.Dispose();
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            // skip if already disposed
            if (this.IsDisposed)
                return;
            this.IsDisposed = true;
            this.Monitor.Log("Disposing...");

            // dispose mod data
            foreach (IModMetadata mod in this.ModRegistry.GetAll())
            {
                try
                {
                    (mod.Mod as IDisposable)?.Dispose();
                }
                catch (Exception ex)
                {
                    mod.LogAsMod($"Mod failed during disposal: {ex.GetLogSummary()}.", LogLevel.Warn);
                }
            }

            // dispose core components
            this.IsGameRunning = false;
            this.LogManager?.Dispose();
            this.ContentCore?.Dispose();
            this.CancellationToken?.Dispose();
            this.GameInstance?.Dispose();

            // end game (moved from Game1.OnExiting to let us clean up first)
            Process.GetCurrentProcess().Kill();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Initialize mods before the first game asset is loaded. At this point the core content managers are loaded (so mods can load their own assets), but the game is mostly uninitialized.</summary>
        private void InitializeBeforeFirstAssetLoaded()
        {
            if (this.CancellationToken.IsCancellationRequested)
            {
                this.Monitor.Log("SMAPI shutting down: aborting initialization.", LogLevel.Warn);
                return;
            }

            // init TMX support
            xTile.Format.FormatManager.Instance.RegisterMapFormat(new TMXTile.TMXFormat(Game1.tileSize / Game1.pixelZoom, Game1.tileSize / Game1.pixelZoom, Game1.pixelZoom, Game1.pixelZoom));

            // load mod data
            ModToolkit toolkit = new ModToolkit();
            ModDatabase modDatabase = toolkit.GetModDatabase(Constants.ApiMetadataPath);

            // load mods
            {
                this.Monitor.Log("Loading mod metadata...");
                ModResolver resolver = new ModResolver();

                // log loose files
                {
                    string[] looseFiles = new DirectoryInfo(this.ModsPath).GetFiles().Select(p => p.Name).ToArray();
                    if (looseFiles.Any())
                        this.Monitor.Log($"  Ignored loose files: {string.Join(", ", looseFiles.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))}");
                }

                // load manifests
                IModMetadata[] mods = resolver.ReadManifests(toolkit, this.ModsPath, modDatabase).ToArray();

                // filter out ignored mods
                foreach (IModMetadata mod in mods.Where(p => p.IsIgnored))
                    this.Monitor.Log($"  Skipped {mod.GetRelativePathWithRoot()} (folder name starts with a dot).");
                mods = mods.Where(p => !p.IsIgnored).ToArray();

                // load mods
                resolver.ValidateManifests(mods, Constants.ApiVersion, toolkit.GetUpdateUrl);
                mods = resolver.ProcessDependencies(mods, modDatabase).ToArray();
                this.LoadMods(mods, this.Toolkit.JsonHelper, this.ContentCore, modDatabase);

                // check for updates
                this.CheckForUpdatesAsync(mods);
            }

            // update window titles
            int modsLoaded = this.ModRegistry.GetAll().Count();
            this.GameInstance.Window.Title = $"Stardew Valley {Constants.GameVersion} - running SMAPI {Constants.ApiVersion} with {modsLoaded} mods";
            this.LogManager.SetConsoleTitle($"SMAPI {Constants.ApiVersion} - running Stardew Valley {Constants.GameVersion} with {modsLoaded} mods");
        }

        /// <summary>Initialize SMAPI and mods after the game starts.</summary>
        private void InitializeAfterGameStart()
        {
            // validate XNB integrity
            if (!this.ValidateContentIntegrity())
                this.Monitor.Log("SMAPI found problems in your game's content files which are likely to cause errors or crashes. Consider uninstalling XNB mods or reinstalling the game.", LogLevel.Error);

            // start SMAPI console
            new Thread(
                () => this.LogManager.RunConsoleInputLoop(
                    commandManager: this.GameInstance.CommandManager,
                    reloadTranslations: this.ReloadTranslations,
                    handleInput: input => this.GameInstance.CommandQueue.Enqueue(input),
                    continueWhile: () => this.IsGameRunning && !this.CancellationToken.IsCancellationRequested
                )
            ).Start();
        }

        /// <summary>Handle the game changing locale.</summary>
        private void OnLocaleChanged()
        {
            this.ContentCore.OnLocaleChanged();

            // get locale
            string locale = this.ContentCore.GetLocale();
            LocalizedContentManager.LanguageCode languageCode = this.ContentCore.Language;

            // update core translations
            this.Translator.SetLocale(locale, languageCode);

            // update mod translation helpers
            foreach (IModMetadata mod in this.ModRegistry.GetAll())
                mod.Translations.SetLocale(locale, languageCode);
        }

        /// <summary>Look for common issues with the game's XNB content, and log warnings if anything looks broken or outdated.</summary>
        /// <returns>Returns whether all integrity checks passed.</returns>
        private bool ValidateContentIntegrity()
        {
            this.Monitor.Log("Detecting common issues...");
            bool issuesFound = false;

            // object format (commonly broken by outdated files)
            {
                // detect issues
                bool hasObjectIssues = false;
                void LogIssue(int id, string issue) => this.Monitor.Log($@"Detected issue: item #{id} in Content\Data\ObjectInformation.xnb is invalid ({issue}).");
                foreach (KeyValuePair<int, string> entry in Game1.objectInformation)
                {
                    // must not be empty
                    if (string.IsNullOrWhiteSpace(entry.Value))
                    {
                        LogIssue(entry.Key, "entry is empty");
                        hasObjectIssues = true;
                        continue;
                    }

                    // require core fields
                    string[] fields = entry.Value.Split('/');
                    if (fields.Length < Object.objectInfoDescriptionIndex + 1)
                    {
                        LogIssue(entry.Key, "too few fields for an object");
                        hasObjectIssues = true;
                        continue;
                    }

                    // check min length for specific types
                    switch (fields[Object.objectInfoTypeIndex].Split(new[] { ' ' }, 2)[0])
                    {
                        case "Cooking":
                            if (fields.Length < Object.objectInfoBuffDurationIndex + 1)
                            {
                                LogIssue(entry.Key, "too few fields for a cooking item");
                                hasObjectIssues = true;
                            }
                            break;
                    }
                }

                // log error
                if (hasObjectIssues)
                {
                    issuesFound = true;
                    this.Monitor.Log(@"Your Content\Data\ObjectInformation.xnb file seems to be broken or outdated.", LogLevel.Warn);
                }
            }

            return !issuesFound;
        }

        /// <summary>Asynchronously check for a new version of SMAPI and any installed mods, and print alerts to the console if an update is available.</summary>
        /// <param name="mods">The mods to include in the update check (if eligible).</param>
        private void CheckForUpdatesAsync(IModMetadata[] mods)
        {
            if (!this.Settings.CheckForUpdates)
                return;

            new Thread(() =>
            {
                // create client
                string url = this.Settings.WebApiBaseUrl;
#if !SMAPI_FOR_WINDOWS
                url = url.Replace("https://", "http://"); // workaround for OpenSSL issues with the game's bundled Mono on Linux/Mac
#endif
                WebApiClient client = new WebApiClient(url, Constants.ApiVersion);
                this.Monitor.Log("Checking for updates...");

                // check SMAPI version
                ISemanticVersion updateFound = null;
                try
                {
                    // fetch update check
                    ModEntryModel response = client.GetModInfo(new[] { new ModSearchEntryModel("Pathoschild.SMAPI", Constants.ApiVersion, new[] { $"GitHub:{this.Settings.GitHubProjectName}" }) }, apiVersion: Constants.ApiVersion, gameVersion: Constants.GameVersion, platform: Constants.Platform).Single().Value;
                    if (response.SuggestedUpdate != null)
                        this.Monitor.Log($"You can update SMAPI to {response.SuggestedUpdate.Version}: {Constants.HomePageUrl}", LogLevel.Alert);
                    else
                        this.Monitor.Log("   SMAPI okay.");

                    updateFound = response.SuggestedUpdate?.Version;

                    // show errors
                    if (response.Errors.Any())
                    {
                        this.Monitor.Log("Couldn't check for a new version of SMAPI. This won't affect your game, but you may not be notified of new versions if this keeps happening.", LogLevel.Warn);
                        this.Monitor.Log($"Error: {string.Join("\n", response.Errors)}");
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log("Couldn't check for a new version of SMAPI. This won't affect your game, but you won't be notified of new versions if this keeps happening.", LogLevel.Warn);
                    this.Monitor.Log(ex is WebException && ex.InnerException == null
                        ? $"Error: {ex.Message}"
                        : $"Error: {ex.GetLogSummary()}"
                    );
                }

                // show update message on next launch
                if (updateFound != null)
                    this.LogManager.WriteUpdateMarker(updateFound.ToString());

                // check mod versions
                if (mods.Any())
                {
                    try
                    {
                        HashSet<string> suppressUpdateChecks = new HashSet<string>(this.Settings.SuppressUpdateChecks, StringComparer.OrdinalIgnoreCase);

                        // prepare search model
                        List<ModSearchEntryModel> searchMods = new List<ModSearchEntryModel>();
                        foreach (IModMetadata mod in mods)
                        {
                            if (!mod.HasID() || suppressUpdateChecks.Contains(mod.Manifest.UniqueID))
                                continue;

                            string[] updateKeys = mod
                                .GetUpdateKeys(validOnly: true)
                                .Select(p => p.ToString())
                                .ToArray();
                            searchMods.Add(new ModSearchEntryModel(mod.Manifest.UniqueID, mod.Manifest.Version, updateKeys.ToArray(), isBroken: mod.Status == ModMetadataStatus.Failed));
                        }

                        // fetch results
                        this.Monitor.Log($"   Checking for updates to {searchMods.Count} mods...");
                        IDictionary<string, ModEntryModel> results = client.GetModInfo(searchMods.ToArray(), apiVersion: Constants.ApiVersion, gameVersion: Constants.GameVersion, platform: Constants.Platform);

                        // extract update alerts & errors
                        var updates = new List<Tuple<IModMetadata, ISemanticVersion, string>>();
                        var errors = new StringBuilder();
                        foreach (IModMetadata mod in mods.OrderBy(p => p.DisplayName))
                        {
                            // link to update-check data
                            if (!mod.HasID() || !results.TryGetValue(mod.Manifest.UniqueID, out ModEntryModel result))
                                continue;
                            mod.SetUpdateData(result);

                            // handle errors
                            if (result.Errors != null && result.Errors.Any())
                            {
                                errors.AppendLine(result.Errors.Length == 1
                                    ? $"   {mod.DisplayName}: {result.Errors[0]}"
                                    : $"   {mod.DisplayName}:\n      - {string.Join("\n      - ", result.Errors)}"
                                );
                            }

                            // handle update
                            if (result.SuggestedUpdate != null)
                                updates.Add(Tuple.Create(mod, result.SuggestedUpdate.Version, result.SuggestedUpdate.Url));
                        }

                        // show update errors
                        if (errors.Length != 0)
                            this.Monitor.Log("Got update-check errors for some mods:\n" + errors.ToString().TrimEnd());

                        // show update alerts
                        if (updates.Any())
                        {
                            this.Monitor.Newline();
                            this.Monitor.Log($"You can update {updates.Count} mod{(updates.Count != 1 ? "s" : "")}:", LogLevel.Alert);
                            foreach (var entry in updates)
                            {
                                IModMetadata mod = entry.Item1;
                                ISemanticVersion newVersion = entry.Item2;
                                string newUrl = entry.Item3;
                                this.Monitor.Log($"   {mod.DisplayName} {newVersion}: {newUrl}", LogLevel.Alert);
                            }
                        }
                        else
                            this.Monitor.Log("   All mods up to date.");
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log("Couldn't check for new mod versions. This won't affect your game, but you won't be notified of mod updates if this keeps happening.", LogLevel.Warn);
                        this.Monitor.Log(ex is WebException && ex.InnerException == null
                            ? ex.Message
                            : ex.ToString()
                        );
                    }
                }
            }).Start();
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
                // note: this happens before this.Monitor is initialized
                Console.WriteLine($"Couldn't create a path: {path}\n\n{ex.GetLogSummary()}");
            }
        }

        /// <summary>Load and hook up the given mods.</summary>
        /// <param name="mods">The mods to load.</param>
        /// <param name="jsonHelper">The JSON helper with which to read mods' JSON files.</param>
        /// <param name="contentCore">The content manager to use for mod content.</param>
        /// <param name="modDatabase">Handles access to SMAPI's internal mod metadata list.</param>
        private void LoadMods(IModMetadata[] mods, JsonHelper jsonHelper, ContentCoordinator contentCore, ModDatabase modDatabase)
        {
            this.Monitor.Log("Loading mods...");

            // load mods
            IList<IModMetadata> skippedMods = new List<IModMetadata>();
            using (AssemblyLoader modAssemblyLoader = new AssemblyLoader(Constants.Platform, this.Monitor, this.Settings.ParanoidWarnings))
            {
                // init
                HashSet<string> suppressUpdateChecks = new HashSet<string>(this.Settings.SuppressUpdateChecks, StringComparer.OrdinalIgnoreCase);
                InterfaceProxyFactory proxyFactory = new InterfaceProxyFactory();

                // load mods
                foreach (IModMetadata mod in mods)
                {
                    if (!this.TryLoadMod(mod, mods, modAssemblyLoader, proxyFactory, jsonHelper, contentCore, modDatabase, suppressUpdateChecks, out string errorPhrase, out string errorDetails))
                    {
                        mod.SetStatus(ModMetadataStatus.Failed, errorPhrase, errorDetails);
                        skippedMods.Add(mod);
                    }
                }
            }
            IModMetadata[] loaded = this.ModRegistry.GetAll().ToArray();
            IModMetadata[] loadedContentPacks = loaded.Where(p => p.IsContentPack).ToArray();
            IModMetadata[] loadedMods = loaded.Where(p => !p.IsContentPack).ToArray();

            // unlock content packs
            this.ModRegistry.AreAllModsLoaded = true;

            // log mod info
            this.LogManager.LogModInfo(loaded, loadedContentPacks, loadedMods, skippedMods.ToArray(), this.Settings.ParanoidWarnings);

            // initialize translations
            this.ReloadTranslations(loaded);

            // initialize loaded non-content-pack mods
            foreach (IModMetadata metadata in loadedMods)
            {
                // add interceptors
                if (metadata.Mod.Helper.Content is ContentHelper helper)
                {
                    // ReSharper disable SuspiciousTypeConversion.Global
                    if (metadata.Mod is IAssetEditor editor)
                        this.ContentCore.Editors.Add(new ModLinked<IAssetEditor>(metadata, editor));
                    if (metadata.Mod is IAssetLoader loader)
                        this.ContentCore.Loaders.Add(new ModLinked<IAssetLoader>(metadata, loader));
                    // ReSharper restore SuspiciousTypeConversion.Global

                    helper.ObservableAssetEditors.CollectionChanged += (sender, e) => this.OnInterceptorsChanged(metadata, e.NewItems?.Cast<IAssetEditor>(), e.OldItems?.Cast<IAssetEditor>(), this.ContentCore.Editors);
                    helper.ObservableAssetLoaders.CollectionChanged += (sender, e) => this.OnInterceptorsChanged(metadata, e.NewItems?.Cast<IAssetLoader>(), e.OldItems?.Cast<IAssetLoader>(), this.ContentCore.Loaders);
                }

                // call entry method
                try
                {
                    IMod mod = metadata.Mod;
                    mod.Entry(mod.Helper);
                }
                catch (Exception ex)
                {
                    metadata.LogAsMod($"Mod crashed on entry and might not work correctly. Technical details:\n{ex.GetLogSummary()}", LogLevel.Error);
                }

                // get mod API
                try
                {
                    object api = metadata.Mod.GetApi();
                    if (api != null && !api.GetType().IsPublic)
                    {
                        api = null;
                        this.Monitor.Log($"{metadata.DisplayName} provides an API instance with a non-public type. This isn't currently supported, so the API won't be available to other mods.", LogLevel.Warn);
                    }

                    if (api != null)
                        this.Monitor.Log($"   Found mod-provided API ({api.GetType().FullName}).");
                    metadata.SetApi(api);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Failed loading mod-provided API for {metadata.DisplayName}. Integrations with other mods may not work. Error: {ex.GetLogSummary()}", LogLevel.Error);
                }
            }

            // invalidate cache entries when needed
            // (These listeners are registered after Entry to avoid repeatedly reloading assets as mods initialize.)
            foreach (IModMetadata metadata in loadedMods)
            {
                if (metadata.Mod.Helper.Content is ContentHelper helper)
                {
                    helper.ObservableAssetEditors.CollectionChanged += (sender, e) => this.GameInstance.OnAssetInterceptorsChanged(metadata, e.NewItems, e.OldItems);
                    helper.ObservableAssetLoaders.CollectionChanged += (sender, e) => this.GameInstance.OnAssetInterceptorsChanged(metadata, e.NewItems, e.OldItems);
                }
            }

            // unlock mod integrations
            this.ModRegistry.AreAllModsInitialized = true;
        }

        /// <summary>Handle a mod adding or removing asset interceptors.</summary>
        /// <typeparam name="T">The asset interceptor type (one of <see cref="IAssetEditor"/> or <see cref="IAssetLoader"/>).</typeparam>
        /// <param name="mod">The mod metadata.</param>
        /// <param name="added">The interceptors that were added.</param>
        /// <param name="removed">The interceptors that were removed.</param>
        /// <param name="list">The list to update.</param>
        private void OnInterceptorsChanged<T>(IModMetadata mod, IEnumerable<T> added, IEnumerable<T> removed, IList<ModLinked<T>> list)
        {
            foreach (T interceptor in added ?? new T[0])
                list.Add(new ModLinked<T>(mod, interceptor));

            foreach (T interceptor in removed ?? new T[0])
            {
                foreach (ModLinked<T> entry in list.Where(p => p.Mod == mod && object.ReferenceEquals(p.Data, interceptor)).ToArray())
                    list.Remove(entry);
            }
        }

        /// <summary>Load a given mod.</summary>
        /// <param name="mod">The mod to load.</param>
        /// <param name="mods">The mods being loaded.</param>
        /// <param name="assemblyLoader">Preprocesses and loads mod assemblies</param>
        /// <param name="proxyFactory">Generates proxy classes to access mod APIs through an arbitrary interface.</param>
        /// <param name="jsonHelper">The JSON helper with which to read mods' JSON files.</param>
        /// <param name="contentCore">The content manager to use for mod content.</param>
        /// <param name="modDatabase">Handles access to SMAPI's internal mod metadata list.</param>
        /// <param name="suppressUpdateChecks">The mod IDs to ignore when validating update keys.</param>
        /// <param name="errorReasonPhrase">The user-facing reason phrase explaining why the mod couldn't be loaded (if applicable).</param>
        /// <param name="errorDetails">More detailed details about the error intended for developers (if any).</param>
        /// <returns>Returns whether the mod was successfully loaded.</returns>
        private bool TryLoadMod(IModMetadata mod, IModMetadata[] mods, AssemblyLoader assemblyLoader, InterfaceProxyFactory proxyFactory, JsonHelper jsonHelper, ContentCoordinator contentCore, ModDatabase modDatabase, HashSet<string> suppressUpdateChecks, out string errorReasonPhrase, out string errorDetails)
        {
            errorDetails = null;

            // log entry
            {
                string relativePath = mod.GetRelativePathWithRoot();
                if (mod.IsContentPack)
                    this.Monitor.Log($"   {mod.DisplayName} (from {relativePath}) [content pack]...");
                else if (mod.Manifest?.EntryDll != null)
                    this.Monitor.Log($"   {mod.DisplayName} (from {relativePath}{Path.DirectorySeparatorChar}{mod.Manifest.EntryDll})..."); // don't use Path.Combine here, since EntryDLL might not be valid
                else
                    this.Monitor.Log($"   {mod.DisplayName} (from {relativePath})...");
            }

            // add warning for missing update key
            if (mod.HasID() && !suppressUpdateChecks.Contains(mod.Manifest.UniqueID) && !mod.HasValidUpdateKeys())
                mod.SetWarning(ModWarning.NoUpdateKeys);

            // validate status
            if (mod.Status == ModMetadataStatus.Failed)
            {
                this.Monitor.Log($"      Failed: {mod.Error}");
                errorReasonPhrase = mod.Error;
                return false;
            }

            // validate dependencies
            // Although dependencies are validated before mods are loaded, a dependency may have failed to load.
            if (mod.Manifest.Dependencies?.Any() == true)
            {
                foreach (IManifestDependency dependency in mod.Manifest.Dependencies.Where(p => p.IsRequired))
                {
                    if (this.ModRegistry.Get(dependency.UniqueID) == null)
                    {
                        string dependencyName = mods
                            .FirstOrDefault(otherMod => otherMod.HasID(dependency.UniqueID))
                            ?.DisplayName ?? dependency.UniqueID;
                        errorReasonPhrase = $"it needs the '{dependencyName}' mod, which couldn't be loaded.";
                        return false;
                    }
                }
            }

            // load as content pack
            if (mod.IsContentPack)
            {
                IManifest manifest = mod.Manifest;
                IMonitor monitor = this.LogManager.GetMonitor(mod.DisplayName);
                IContentHelper contentHelper = new ContentHelper(this.ContentCore, mod.DirectoryPath, manifest.UniqueID, mod.DisplayName, monitor);
                TranslationHelper translationHelper = new TranslationHelper(manifest.UniqueID, contentCore.GetLocale(), contentCore.Language);
                IContentPack contentPack = new ContentPack(mod.DirectoryPath, manifest, contentHelper, translationHelper, jsonHelper);
                mod.SetMod(contentPack, monitor, translationHelper);
                this.ModRegistry.Add(mod);

                errorReasonPhrase = null;
                return true;
            }

            // load as mod
            else
            {
                IManifest manifest = mod.Manifest;

                // load mod
                string assemblyPath = manifest?.EntryDll != null
                    ? Path.Combine(mod.DirectoryPath, manifest.EntryDll)
                    : null;
                Assembly modAssembly;
                try
                {
                    modAssembly = assemblyLoader.Load(mod, assemblyPath, assumeCompatible: mod.DataRecord?.Status == ModStatus.AssumeCompatible);
                    this.ModRegistry.TrackAssemblies(mod, modAssembly);
                }
                catch (IncompatibleInstructionException) // details already in trace logs
                {
                    string[] updateUrls = new[] { modDatabase.GetModPageUrlFor(manifest.UniqueID), "https://smapi.io/mods" }.Where(p => p != null).ToArray();
                    errorReasonPhrase = $"it's no longer compatible. Please check for a new version at {string.Join(" or ", updateUrls)}";
                    return false;
                }
                catch (SAssemblyLoadFailedException ex)
                {
                    errorReasonPhrase = $"its DLL couldn't be loaded: {ex.Message}";
                    return false;
                }
                catch (Exception ex)
                {
                    errorReasonPhrase = "its DLL couldn't be loaded.";
                    errorDetails = $"Error: {ex.GetLogSummary()}";
                    return false;
                }

                // initialize mod
                try
                {
                    // get mod instance
                    if (!this.TryLoadModEntry(modAssembly, out Mod modEntry, out errorReasonPhrase))
                        return false;

                    // get content packs
                    IContentPack[] GetContentPacks()
                    {
                        if (!this.ModRegistry.AreAllModsLoaded)
                            throw new InvalidOperationException("Can't access content packs before SMAPI finishes loading mods.");

                        return this.ModRegistry
                            .GetAll(assemblyMods: false)
                            .Where(p => p.IsContentPack && mod.HasID(p.Manifest.ContentPackFor.UniqueID))
                            .Select(p => p.ContentPack)
                            .ToArray();
                    }

                    // init mod helpers
                    IMonitor monitor = this.LogManager.GetMonitor(mod.DisplayName);
                    TranslationHelper translationHelper = new TranslationHelper(manifest.UniqueID, contentCore.GetLocale(), contentCore.Language);
                    IModHelper modHelper;
                    {
                        IContentPack CreateFakeContentPack(string packDirPath, IManifest packManifest)
                        {
                            IMonitor packMonitor = this.LogManager.GetMonitor(packManifest.Name);
                            IContentHelper packContentHelper = new ContentHelper(contentCore, packDirPath, packManifest.UniqueID, packManifest.Name, packMonitor);
                            ITranslationHelper packTranslationHelper = new TranslationHelper(packManifest.UniqueID, contentCore.GetLocale(), contentCore.Language);
                            return new ContentPack(packDirPath, packManifest, packContentHelper, packTranslationHelper, this.Toolkit.JsonHelper);
                        }

                        IModEvents events = new ModEvents(mod, this.EventManager);
                        ICommandHelper commandHelper = new CommandHelper(mod, this.GameInstance.CommandManager);
                        IContentHelper contentHelper = new ContentHelper(contentCore, mod.DirectoryPath, manifest.UniqueID, mod.DisplayName, monitor);
                        IContentPackHelper contentPackHelper = new ContentPackHelper(manifest.UniqueID, new Lazy<IContentPack[]>(GetContentPacks), CreateFakeContentPack);
                        IDataHelper dataHelper = new DataHelper(manifest.UniqueID, mod.DirectoryPath, jsonHelper);
                        IReflectionHelper reflectionHelper = new ReflectionHelper(manifest.UniqueID, mod.DisplayName, this.Reflection);
                        IModRegistry modRegistryHelper = new ModRegistryHelper(manifest.UniqueID, this.ModRegistry, proxyFactory, monitor);
                        IMultiplayerHelper multiplayerHelper = new MultiplayerHelper(manifest.UniqueID, this.GameInstance.Multiplayer);

                        modHelper = new ModHelper(manifest.UniqueID, mod.DirectoryPath, this.GameInstance.Input, events, contentHelper, contentPackHelper, commandHelper, dataHelper, modRegistryHelper, reflectionHelper, multiplayerHelper, translationHelper);
                    }

                    // init mod
                    modEntry.ModManifest = manifest;
                    modEntry.Helper = modHelper;
                    modEntry.Monitor = monitor;

                    // track mod
                    mod.SetMod(modEntry, translationHelper);
                    this.ModRegistry.Add(mod);
                    return true;
                }
                catch (Exception ex)
                {
                    errorReasonPhrase = $"initialization failed:\n{ex.GetLogSummary()}";
                    return false;
                }
            }
        }

        /// <summary>Load a mod's entry class.</summary>
        /// <param name="modAssembly">The mod assembly.</param>
        /// <param name="mod">The loaded instance.</param>
        /// <param name="error">The error indicating why loading failed (if applicable).</param>
        /// <returns>Returns whether the mod entry class was successfully loaded.</returns>
        private bool TryLoadModEntry(Assembly modAssembly, out Mod mod, out string error)
        {
            mod = null;

            // find type
            TypeInfo[] modEntries = modAssembly.DefinedTypes.Where(type => typeof(Mod).IsAssignableFrom(type) && !type.IsAbstract).Take(2).ToArray();
            if (modEntries.Length == 0)
            {
                error = $"its DLL has no '{nameof(Mod)}' subclass.";
                return false;
            }
            if (modEntries.Length > 1)
            {
                error = $"its DLL contains multiple '{nameof(Mod)}' subclasses.";
                return false;
            }

            // get implementation
            mod = (Mod)modAssembly.CreateInstance(modEntries[0].ToString());
            if (mod == null)
            {
                error = "its entry class couldn't be instantiated.";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>Reload translations for all mods.</summary>
        private void ReloadTranslations()
        {
            this.ReloadTranslations(this.ModRegistry.GetAll());
        }

        /// <summary>Reload translations for the given mods.</summary>
        /// <param name="mods">The mods for which to reload translations.</param>
        private void ReloadTranslations(IEnumerable<IModMetadata> mods)
        {
            // core SMAPI translations
            {
                var translations = this.ReadTranslationFiles(Path.Combine(Constants.InternalFilesPath, "i18n"), out IList<string> errors);
                if (errors.Any() || !translations.Any())
                {
                    this.Monitor.Log("SMAPI couldn't load some core translations. You may need to reinstall SMAPI.", LogLevel.Warn);
                    foreach (string error in errors)
                        this.Monitor.Log($"  - {error}", LogLevel.Warn);
                }
                this.Translator.SetTranslations(translations);
            }

            // mod translations
            foreach (IModMetadata metadata in mods)
            {
                var translations = this.ReadTranslationFiles(Path.Combine(metadata.DirectoryPath, "i18n"), out IList<string> errors);
                if (errors.Any())
                {
                    metadata.LogAsMod("Mod couldn't load some translation files:", LogLevel.Warn);
                    foreach (string error in errors)
                        metadata.LogAsMod($"  - {error}", LogLevel.Warn);
                }
                metadata.Translations.SetTranslations(translations);
            }
        }

        /// <summary>Read translations from a directory containing JSON translation files.</summary>
        /// <param name="folderPath">The folder path to search.</param>
        /// <param name="errors">The errors indicating why translation files couldn't be parsed, indexed by translation filename.</param>
        private IDictionary<string, IDictionary<string, string>> ReadTranslationFiles(string folderPath, out IList<string> errors)
        {
            JsonHelper jsonHelper = this.Toolkit.JsonHelper;

            // read translation files
            var translations = new Dictionary<string, IDictionary<string, string>>();
            errors = new List<string>();
            DirectoryInfo translationsDir = new DirectoryInfo(folderPath);
            if (translationsDir.Exists)
            {
                foreach (FileInfo file in translationsDir.EnumerateFiles("*.json"))
                {
                    string locale = Path.GetFileNameWithoutExtension(file.Name.ToLower().Trim());
                    try
                    {
                        if (!jsonHelper.ReadJsonFileIfExists(file.FullName, out IDictionary<string, string> data))
                        {
                            errors.Add($"{file.Name} file couldn't be read"); // should never happen, since we're iterating files that exist
                            continue;
                        }

                        translations[locale] = data;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{file.Name} file couldn't be parsed: {ex.GetLogSummary()}");
                        continue;
                    }
                }
            }

            // validate translations
            foreach (string locale in translations.Keys.ToArray())
            {
                // handle duplicates
                HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                HashSet<string> duplicateKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string key in translations[locale].Keys.ToArray())
                {
                    if (!keys.Add(key))
                    {
                        duplicateKeys.Add(key);
                        translations[locale].Remove(key);
                    }
                }
                if (duplicateKeys.Any())
                    errors.Add($"{locale}.json has duplicate translation keys: [{string.Join(", ", duplicateKeys)}]. Keys are case-insensitive.");
            }

            return translations;
        }

        /// <summary>Get the absolute path to the next available log file.</summary>
        private string GetLogPath()
        {
            // default path
            {
                FileInfo defaultFile = new FileInfo(Path.Combine(Constants.LogDir, $"{Constants.LogFilename}.{Constants.LogExtension}"));
                if (!defaultFile.Exists)
                    return defaultFile.FullName;
            }

            // get first disambiguated path
            for (int i = 2; i < int.MaxValue; i++)
            {
                FileInfo file = new FileInfo(Path.Combine(Constants.LogDir, $"{Constants.LogFilename}.player-{i}.{Constants.LogExtension}"));
                if (!file.Exists)
                    return file.FullName;
            }

            // should never happen
            throw new InvalidOperationException("Could not find an available log path.");
        }

        /// <summary>Delete normal (non-crash) log files created by SMAPI.</summary>
        private void PurgeNormalLogs()
        {
            DirectoryInfo logsDir = new DirectoryInfo(Constants.LogDir);
            if (!logsDir.Exists)
                return;

            foreach (FileInfo logFile in logsDir.EnumerateFiles())
            {
                // skip non-SMAPI file
                if (!logFile.Name.StartsWith(Constants.LogNamePrefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                // skip crash log
                if (logFile.FullName == Constants.FatalCrashLog)
                    continue;

                // delete file
                try
                {
                    FileUtilities.ForceDelete(logFile);
                }
                catch (IOException)
                {
                    // ignore file if it's in use
                }
            }
        }
    }
}
