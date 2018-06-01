using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework.Events;
using StardewModdingAPI.Framework.Input;
using StardewModdingAPI.Framework.Reflection;
using StardewModdingAPI.Framework.StateTracking;
using StardewModdingAPI.Framework.StateTracking.FieldWatchers;
using StardewModdingAPI.Framework.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;
using xTile.Layers;
using Object = StardewValley.Object;

namespace StardewModdingAPI.Framework
{
    /// <summary>SMAPI's extension of the game's core <see cref="Game1"/>, used to inject events.</summary>
    internal class SGame : Game1
    {
        /*********
        ** Properties
        *********/
        /****
        ** Constructor hack
        ****/
        /// <summary>A static instance of <see cref="Monitor"/> to use while <see cref="Game1"/> is initialising, which happens before the <see cref="SGame"/> constructor runs.</summary>
        internal static IMonitor MonitorDuringInitialisation;

        /// <summary>A static instance of <see cref="Reflection"/> to use while <see cref="Game1"/> is initialising, which happens before the <see cref="SGame"/> constructor runs.</summary>
        internal static Reflector ReflectorDuringInitialisation;


        /****
        ** SMAPI state
        ****/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Manages SMAPI events for mods.</summary>
        private readonly EventManager Events;

        /// <summary>Manages input visible to the game.</summary>
        private SInputState Input => (SInputState)Game1.input;

        /// <summary>The maximum number of consecutive attempts SMAPI should make to recover from a draw error.</summary>
        private readonly Countdown DrawCrashTimer = new Countdown(60); // 60 ticks = roughly one second

        /// <summary>The maximum number of consecutive attempts SMAPI should make to recover from an update error.</summary>
        private readonly Countdown UpdateCrashTimer = new Countdown(60); // 60 ticks = roughly one second

        /// <summary>The number of ticks until SMAPI should notify mods that the game has loaded.</summary>
        /// <remarks>Skipping a few frames ensures the game finishes initialising the world before mods try to change it.</remarks>
        private int AfterLoadTimer = 5;

        /// <summary>Whether the after-load events were raised for this session.</summary>
        private bool RaisedAfterLoadEvent;

        /// <summary>Whether the game is saving and SMAPI has already raised <see cref="SaveEvents.BeforeSave"/>.</summary>
        private bool IsBetweenSaveEvents;

        /// <summary>Whether the game is creating the save file and SMAPI has already raised <see cref="SaveEvents.BeforeCreate"/>.</summary>
        private bool IsBetweenCreateEvents;

        /****
        ** Game state
        ****/
        /// <summary>The underlying watchers for convenience. These are accessible individually as separate properties.</summary>
        private readonly List<IWatcher> Watchers = new List<IWatcher>();

        /// <summary>Tracks changes to the window size.</summary>
        private readonly IValueWatcher<Point> WindowSizeWatcher;

        /// <summary>Tracks changes to the current player.</summary>
        private PlayerTracker CurrentPlayerTracker;

        /// <summary>Tracks changes to the time of day (in 24-hour military format).</summary>
        private readonly IValueWatcher<int> TimeWatcher;

        /// <summary>Tracks changes to the save ID.</summary>
        private readonly IValueWatcher<ulong> SaveIdWatcher;

        /// <summary>Tracks changes to the game's locations.</summary>
        private readonly WorldLocationsTracker LocationsWatcher;

        /// <summary>Tracks changes to <see cref="Game1.activeClickableMenu"/>.</summary>
        private readonly IValueWatcher<IClickableMenu> ActiveMenuWatcher;

        /// <summary>The previous content locale.</summary>
        private LocalizedContentManager.LanguageCode? PreviousLocale;

        /// <summary>An index incremented on every tick and reset every 60th tick (0–59).</summary>
        private int CurrentUpdateTick;

        /// <summary>Whether this is the very first update tick since the game started.</summary>
        private bool FirstUpdate;

        /// <summary>A callback to invoke after the game finishes initialising.</summary>
        private readonly Action OnGameInitialised;

        /// <summary>A callback to invoke when the game exits.</summary>
        private readonly Action OnGameExiting;

        /// <summary>Simplifies access to private game code.</summary>
        private readonly Reflector Reflection;

        /// <summary>Whether the next content manager requested by the game will be for <see cref="Game1.content"/>.</summary>
        private bool NextContentManagerIsMain;


        /*********
        ** Accessors
        *********/
        /// <summary>SMAPI's content manager.</summary>
        public ContentCoordinator ContentCore { get; private set; }

        /// <summary>The game's core multiplayer utility.</summary>
        public SMultiplayer Multiplayer => (SMultiplayer)Game1.multiplayer;

        /// <summary>Whether SMAPI should log more information about the game context.</summary>
        public bool VerboseLogging { get; set; }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="eventManager">Manages SMAPI events for mods.</param>
        /// <param name="onGameInitialised">A callback to invoke after the game finishes initialising.</param>
        /// <param name="onGameExiting">A callback to invoke when the game exits.</param>
        internal SGame(IMonitor monitor, Reflector reflection, EventManager eventManager, Action onGameInitialised, Action onGameExiting)
        {
            // check expectations
            if (this.ContentCore == null)
                throw new InvalidOperationException($"The game didn't initialise its first content manager before SMAPI's {nameof(SGame)} constructor. This indicates an incompatible lifecycle change.");

            // init XNA
            Game1.graphics.GraphicsProfile = GraphicsProfile.HiDef;

            // init SMAPI
            this.Monitor = monitor;
            this.Events = eventManager;
            this.FirstUpdate = true;
            this.Reflection = reflection;
            this.OnGameInitialised = onGameInitialised;
            this.OnGameExiting = onGameExiting;
            Game1.input = new SInputState();
            Game1.multiplayer = new SMultiplayer(monitor, eventManager);

            // init watchers
            Game1.locations = new ObservableCollection<GameLocation>();
            this.SaveIdWatcher = WatcherFactory.ForEquatable(() => Game1.hasLoadedGame ? Game1.uniqueIDForThisGame : 0);
            this.WindowSizeWatcher = WatcherFactory.ForEquatable(() => new Point(Game1.viewport.Width, Game1.viewport.Height));
            this.TimeWatcher = WatcherFactory.ForEquatable(() => Game1.timeOfDay);
            this.ActiveMenuWatcher = WatcherFactory.ForReference(() => Game1.activeClickableMenu);
            this.LocationsWatcher = new WorldLocationsTracker((ObservableCollection<GameLocation>)Game1.locations);
            this.Watchers.AddRange(new IWatcher[]
            {
                this.SaveIdWatcher,
                this.WindowSizeWatcher,
                this.TimeWatcher,
                this.ActiveMenuWatcher,
                this.LocationsWatcher
            });
        }

        /// <summary>Perform cleanup logic when the game exits.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event args.</param>
        /// <remarks>This overrides the logic in <see cref="Game1.exitEvent"/> to let SMAPI clean up before exit.</remarks>
        protected override void OnExiting(object sender, EventArgs args)
        {
            Game1.multiplayer.Disconnect();
            this.OnGameExiting?.Invoke();
        }

        /****
        ** Intercepted methods & events
        ****/
        /// <summary>Constructor a content manager to read XNB files.</summary>
        /// <param name="serviceProvider">The service provider to use to locate services.</param>
        /// <param name="rootDirectory">The root directory to search for content.</param>
        protected override LocalizedContentManager CreateContentManager(IServiceProvider serviceProvider, string rootDirectory)
        {
            // Game1._temporaryContent initialising from SGame constructor
            // NOTE: this method is called before the SGame constructor runs. Don't depend on anything being initialised at this point.
            if (this.ContentCore == null)
            {
                this.ContentCore = new ContentCoordinator(serviceProvider, rootDirectory, Thread.CurrentThread.CurrentUICulture, SGame.MonitorDuringInitialisation, SGame.ReflectorDuringInitialisation);
                SGame.MonitorDuringInitialisation = null;
                this.NextContentManagerIsMain = true;
                return this.ContentCore.CreateGameContentManager("Game1._temporaryContent");
            }

            // Game1.content initialising from LoadContent
            if (this.NextContentManagerIsMain)
            {
                this.NextContentManagerIsMain = false;
                return this.ContentCore.MainContentManager;
            }

            // any other content manager
            return this.ContentCore.CreateGameContentManager("(generated)");
        }

        /// <summary>The method called when the game is updating its state. This happens roughly 60 times per second.</summary>
        /// <param name="gameTime">A snapshot of the game timing state.</param>
        protected override void Update(GameTime gameTime)
        {
            try
            {
                /*********
                ** Update input
                *********/
                // This should *always* run, even when suppressing mod events, since the game uses
                // this too. For example, doing this after mod event suppression would prevent the
                // user from doing anything on the overnight shipping screen.
                SInputState previousInputState = this.Input.Clone();
                SInputState inputState = this.Input;
                if (this.IsActive)
                    inputState.TrueUpdate();

                /*********
                ** Load game synchronously
                *********/
                if (Game1.gameMode == Game1.loadingMode)
                {
                    this.Monitor.Log("Running game loader...", LogLevel.Trace);
                    while (Game1.gameMode == Game1.loadingMode)
                    {
                        base.Update(gameTime);
                        this.Events.Specialised_UnvalidatedUpdateTick.Raise();
                    }
                    this.Monitor.Log("Game loader OK.", LogLevel.Trace);
                }

                /*********
                ** Skip conditions
                *********/
                // SMAPI exiting, stop processing game updates
                if (this.Monitor.IsExiting)
                {
                    this.Monitor.Log("SMAPI shutting down: aborting update.", LogLevel.Trace);
                    return;
                }

                // While a background task is in progress, the game may make changes to the game
                // state while mods are running their code. This is risky, because data changes can
                // conflict (e.g. collection changed during enumeration errors) and data may change
                // unexpectedly from one mod instruction to the next.
                // 
                // Therefore we can just run Game1.Update here without raising any SMAPI events. There's
                // a small chance that the task will finish after we defer but before the game checks,
                // which means technically events should be raised, but the effects of missing one
                // update tick are neglible and not worth the complications of bypassing Game1.Update.
                if (Game1._newDayTask != null || Game1.gameMode == Game1.loadingMode)
                {
                    base.Update(gameTime);
                    this.Events.Specialised_UnvalidatedUpdateTick.Raise();
                    return;
                }

                /*********
                ** Save events + suppress events during save
                *********/
                // While the game is writing to the save file in the background, mods can unexpectedly
                // fail since they don't have exclusive access to resources (e.g. collection changed
                // during enumeration errors). To avoid problems, events are not invoked while a save
                // is in progress. It's safe to raise SaveEvents.BeforeSave as soon as the menu is
                // opened (since the save hasn't started yet), but all other events should be suppressed.
                if (Context.IsSaving)
                {
                    // raise before-create
                    if (!Context.IsWorldReady && !this.IsBetweenCreateEvents)
                    {
                        this.IsBetweenCreateEvents = true;
                        this.Monitor.Log("Context: before save creation.", LogLevel.Trace);
                        this.Events.Save_BeforeCreate.Raise();
                    }

                    // raise before-save
                    if (Context.IsWorldReady && !this.IsBetweenSaveEvents)
                    {
                        this.IsBetweenSaveEvents = true;
                        this.Monitor.Log("Context: before save.", LogLevel.Trace);
                        this.Events.Save_BeforeSave.Raise();
                    }

                    // suppress non-save events
                    base.Update(gameTime);
                    this.Events.Specialised_UnvalidatedUpdateTick.Raise();
                    return;
                }
                if (this.IsBetweenCreateEvents)
                {
                    // raise after-create
                    this.IsBetweenCreateEvents = false;
                    this.Monitor.Log($"Context: after save creation, starting {Game1.currentSeason} {Game1.dayOfMonth} Y{Game1.year}.", LogLevel.Trace);
                    this.Events.Save_AfterCreate.Raise();
                }
                if (this.IsBetweenSaveEvents)
                {
                    // raise after-save
                    this.IsBetweenSaveEvents = false;
                    this.Monitor.Log($"Context: after save, starting {Game1.currentSeason} {Game1.dayOfMonth} Y{Game1.year}.", LogLevel.Trace);
                    this.Events.Save_AfterSave.Raise();
                    this.Events.Time_AfterDayStarted.Raise();
                }

                /*********
                ** Notify SMAPI that game is initialised
                *********/
                if (this.FirstUpdate)
                    this.OnGameInitialised();

                /*********
                ** Update context
                *********/
                if (Context.IsWorldReady && !Context.IsSaveLoaded)
                    this.MarkWorldNotReady();
                else if (Context.IsSaveLoaded && !SaveGame.IsProcessing /*still loading save*/ && this.AfterLoadTimer >= 0 && Game1.currentLocation != null)
                {
                    if (Game1.dayOfMonth != 0) // wait until new-game intro finishes (world not fully initialised yet)
                        this.AfterLoadTimer--;
                    Context.IsWorldReady = this.AfterLoadTimer <= 0;
                }

                /*********
                ** Update watchers
                *********/
                // reset player
                if (Context.IsWorldReady)
                {
                    if (this.CurrentPlayerTracker == null || this.CurrentPlayerTracker.Player != Game1.player)
                    {
                        this.CurrentPlayerTracker?.Dispose();
                        this.CurrentPlayerTracker = new PlayerTracker(Game1.player);
                    }
                }
                else
                {
                    if (this.CurrentPlayerTracker != null)
                    {
                        this.CurrentPlayerTracker.Dispose();
                        this.CurrentPlayerTracker = null;
                    }
                }

                // update values
                foreach (IWatcher watcher in this.Watchers)
                    watcher.Update();
                this.CurrentPlayerTracker?.Update();
                this.LocationsWatcher.Update();

                /*********
                ** Locale changed events
                *********/
                if (this.PreviousLocale != LocalizedContentManager.CurrentLanguageCode)
                {
                    var oldValue = this.PreviousLocale;
                    var newValue = LocalizedContentManager.CurrentLanguageCode;

                    this.Monitor.Log($"Context: locale set to {newValue}.", LogLevel.Trace);

                    if (oldValue != null)
                        this.Events.Content_LocaleChanged.Raise(new EventArgsValueChanged<string>(oldValue.ToString(), newValue.ToString()));

                    this.PreviousLocale = newValue;
                }

                /*********
                ** After load events
                *********/
                if (!this.RaisedAfterLoadEvent && Context.IsWorldReady)
                {
                    // print context
                    string context = $"Context: loaded saved game '{Constants.SaveFolderName}', starting {Game1.currentSeason} {Game1.dayOfMonth} Y{Game1.year}.";
                    if (Context.IsMultiplayer)
                    {
                        int onlineCount = Game1.getOnlineFarmers().Count();
                        context += $" {(Context.IsMainPlayer ? "Main player" : "Farmhand")} with {onlineCount} {(onlineCount == 1 ? "player" : "players")} online.";
                    }
                    else
                        context += " Single-player.";
                    this.Monitor.Log(context, LogLevel.Trace);

                    // raise events
                    this.RaisedAfterLoadEvent = true;
                    this.Events.Save_AfterLoad.Raise();
                    this.Events.Time_AfterDayStarted.Raise();
                }

                /*********
                ** Exit to title events
                *********/
                if (Game1.exitToTitle)
                {
                    this.Monitor.Log("Context: returned to title", LogLevel.Trace);
                    this.Events.Save_AfterReturnToTitle.Raise();
                }

                /*********
                ** Window events
                *********/
                // Here we depend on the game's viewport instead of listening to the Window.Resize
                // event because we need to notify mods after the game handles the resize, so the
                // game's metadata (like Game1.viewport) are updated. That's a bit complicated
                // since the game adds & removes its own handler on the fly.
                if (this.WindowSizeWatcher.IsChanged)
                {
                    if (this.VerboseLogging)
                        this.Monitor.Log($"Context: window size changed to {this.WindowSizeWatcher.CurrentValue}.", LogLevel.Trace);
                    this.Events.Graphics_Resize.Raise();
                    this.WindowSizeWatcher.Reset();
                }

                /*********
                ** Input events (if window has focus)
                *********/
                if (this.IsActive)
                {
                    // raise events
                    bool isChatInput = Game1.IsChatting || (Context.IsMultiplayer && Context.IsWorldReady && Game1.activeClickableMenu == null && Game1.currentMinigame == null && inputState.IsAnyDown(Game1.options.chatButton));
                    if (!isChatInput)
                    {
                        // get cursor position
                        ICursorPosition cursor;
                        {
                            // cursor position
                            Vector2 screenPixels = new Vector2(Game1.getMouseX(), Game1.getMouseY());
                            Vector2 tile = new Vector2((int)((Game1.viewport.X + screenPixels.X) / Game1.tileSize), (int)((Game1.viewport.Y + screenPixels.Y) / Game1.tileSize));
                            Vector2 grabTile = (Game1.mouseCursorTransparency > 0 && Utility.tileWithinRadiusOfPlayer((int)tile.X, (int)tile.Y, 1, Game1.player)) // derived from Game1.pressActionButton
                                ? tile
                                : Game1.player.GetGrabTile();
                            cursor = new CursorPosition(screenPixels, tile, grabTile);
                        }

                        // raise input events
                        foreach (var pair in inputState.ActiveButtons)
                        {
                            SButton button = pair.Key;
                            InputStatus status = pair.Value;

                            if (status == InputStatus.Pressed)
                            {
                                this.Events.Input_ButtonPressed.Raise(new EventArgsInput(button, cursor, button.IsActionButton(), button.IsUseToolButton(), inputState.SuppressButtons));

                                // legacy events
                                if (button.TryGetKeyboard(out Keys key))
                                {
                                    if (key != Keys.None)
                                        this.Events.Control_KeyPressed.Raise(new EventArgsKeyPressed(key));
                                }
                                else if (button.TryGetController(out Buttons controllerButton))
                                {
                                    if (controllerButton == Buttons.LeftTrigger || controllerButton == Buttons.RightTrigger)
                                        this.Events.Control_ControllerTriggerPressed.Raise(new EventArgsControllerTriggerPressed(PlayerIndex.One, controllerButton, controllerButton == Buttons.LeftTrigger ? inputState.RealController.Triggers.Left : inputState.RealController.Triggers.Right));
                                    else
                                        this.Events.Control_ControllerButtonPressed.Raise(new EventArgsControllerButtonPressed(PlayerIndex.One, controllerButton));
                                }
                            }
                            else if (status == InputStatus.Released)
                            {
                                this.Events.Input_ButtonReleased.Raise(new EventArgsInput(button, cursor, button.IsActionButton(), button.IsUseToolButton(), inputState.SuppressButtons));

                                // legacy events
                                if (button.TryGetKeyboard(out Keys key))
                                {
                                    if (key != Keys.None)
                                        this.Events.Control_KeyReleased.Raise(new EventArgsKeyPressed(key));
                                }
                                else if (button.TryGetController(out Buttons controllerButton))
                                {
                                    if (controllerButton == Buttons.LeftTrigger || controllerButton == Buttons.RightTrigger)
                                        this.Events.Control_ControllerTriggerReleased.Raise(new EventArgsControllerTriggerReleased(PlayerIndex.One, controllerButton, controllerButton == Buttons.LeftTrigger ? inputState.RealController.Triggers.Left : inputState.RealController.Triggers.Right));
                                    else
                                        this.Events.Control_ControllerButtonReleased.Raise(new EventArgsControllerButtonReleased(PlayerIndex.One, controllerButton));
                                }
                            }
                        }

                        // raise legacy state-changed events
                        if (inputState.RealKeyboard != previousInputState.RealKeyboard)
                            this.Events.Control_KeyboardChanged.Raise(new EventArgsKeyboardStateChanged(previousInputState.RealKeyboard, inputState.RealKeyboard));
                        if (inputState.RealMouse != previousInputState.RealMouse)
                            this.Events.Control_MouseChanged.Raise(new EventArgsMouseStateChanged(previousInputState.RealMouse, inputState.RealMouse, previousInputState.MousePosition, inputState.MousePosition));
                    }
                }

                /*********
                ** Menu events
                *********/
                if (this.ActiveMenuWatcher.IsChanged)
                {
                    IClickableMenu previousMenu = this.ActiveMenuWatcher.PreviousValue;
                    IClickableMenu newMenu = this.ActiveMenuWatcher.CurrentValue;
                    this.ActiveMenuWatcher.Reset(); // reset here so a mod changing the menu will be raised as a new event afterwards

                    if (this.VerboseLogging)
                        this.Monitor.Log($"Context: menu changed from {previousMenu?.GetType().FullName ?? "none"} to {newMenu?.GetType().FullName ?? "none"}.", LogLevel.Trace);

                    // raise menu events
                    if (newMenu != null)
                        this.Events.Menu_Changed.Raise(new EventArgsClickableMenuChanged(previousMenu, newMenu));
                    else
                        this.Events.Menu_Closed.Raise(new EventArgsClickableMenuClosed(previousMenu));
                }

                /*********
                ** World & player events
                *********/
                if (Context.IsWorldReady)
                {
                    bool raiseWorldEvents = !this.SaveIdWatcher.IsChanged; // don't report changes from unloaded => loaded

                    // raise location changes
                    if (this.LocationsWatcher.IsChanged)
                    {
                        // location list changes
                        if (this.LocationsWatcher.IsLocationListChanged)
                        {
                            GameLocation[] added = this.LocationsWatcher.Added.ToArray();
                            GameLocation[] removed = this.LocationsWatcher.Removed.ToArray();
                            this.LocationsWatcher.ResetLocationList();

                            if (this.VerboseLogging)
                            {
                                string addedText = this.LocationsWatcher.Added.Any() ? string.Join(", ", added.Select(p => p.Name)) : "none";
                                string removedText = this.LocationsWatcher.Removed.Any() ? string.Join(", ", removed.Select(p => p.Name)) : "none";
                                this.Monitor.Log($"Context: location list changed (added {addedText}; removed {removedText}).", LogLevel.Trace);
                            }

                            this.Events.World_LocationListChanged.Raise(new WorldLocationListChangedEventArgs(added, removed));
                            this.Events.Location_LocationsChanged.Raise(new EventArgsLocationsChanged(added, removed));
                        }

                        // raise location contents changed
                        if (raiseWorldEvents)
                        {
                            foreach (LocationTracker watcher in this.LocationsWatcher.Locations)
                            {
                                // objects changed
                                if (watcher.ObjectsWatcher.IsChanged)
                                {
                                    GameLocation location = watcher.Location;
                                    KeyValuePair<Vector2, Object>[] added = watcher.ObjectsWatcher.Added.ToArray();
                                    KeyValuePair<Vector2, Object>[] removed = watcher.ObjectsWatcher.Removed.ToArray();
                                    watcher.ObjectsWatcher.Reset();

                                    this.Events.World_ObjectListChanged.Raise(new WorldObjectListChangedEventArgs(location, added, removed));
                                    this.Events.Location_ObjectsChanged.Raise(new EventArgsLocationObjectsChanged(location, added, removed));
                                }

                                // buildings changed
                                if (watcher.BuildingsWatcher.IsChanged)
                                {
                                    GameLocation location = watcher.Location;
                                    Building[] added = watcher.BuildingsWatcher.Added.ToArray();
                                    Building[] removed = watcher.BuildingsWatcher.Removed.ToArray();
                                    watcher.BuildingsWatcher.Reset();

                                    this.Events.World_BuildingListChanged.Raise(new WorldBuildingListChangedEventArgs(location, added, removed));
                                    this.Events.Location_BuildingsChanged.Raise(new EventArgsLocationBuildingsChanged(location, added, removed));
                                }

                                // terrain features changed
                                if (watcher.TerrainFeaturesWatcher.IsChanged)
                                {
                                    GameLocation location = watcher.Location;
                                    KeyValuePair<Vector2, TerrainFeature>[] added = watcher.TerrainFeaturesWatcher.Added.ToArray();
                                    KeyValuePair<Vector2, TerrainFeature>[] removed = watcher.TerrainFeaturesWatcher.Removed.ToArray();
                                    watcher.TerrainFeaturesWatcher.Reset();

                                    this.Events.World_TerrainFeatureListChanged.Raise(new WorldTerrainFeatureListChangedEventArgs(location, added, removed));
                                }
                            }
                        }
                        else
                            this.LocationsWatcher.Reset();
                    }

                    // raise time changed
                    if (raiseWorldEvents && this.TimeWatcher.IsChanged)
                    {
                        int was = this.TimeWatcher.PreviousValue;
                        int now = this.TimeWatcher.CurrentValue;
                        this.TimeWatcher.Reset();

                        if (this.VerboseLogging)
                            this.Monitor.Log($"Context: time changed from {was} to {now}.", LogLevel.Trace);

                        this.Events.Time_TimeOfDayChanged.Raise(new EventArgsIntChanged(was, now));
                    }
                    else
                        this.TimeWatcher.Reset();

                    // raise player events
                    if (raiseWorldEvents)
                    {
                        PlayerTracker curPlayer = this.CurrentPlayerTracker;

                        // raise current location changed
                        if (curPlayer.TryGetNewLocation(out GameLocation newLocation))
                        {
                            if (this.VerboseLogging)
                                this.Monitor.Log($"Context: set location to {newLocation.Name}.", LogLevel.Trace);
                            this.Events.Player_Warped.Raise(new EventArgsPlayerWarped(curPlayer.LocationWatcher.PreviousValue, newLocation));
                        }

                        // raise player leveled up a skill
                        foreach (KeyValuePair<EventArgsLevelUp.LevelType, IValueWatcher<int>> pair in curPlayer.GetChangedSkills())
                        {
                            if (this.VerboseLogging)
                                this.Monitor.Log($"Context: player skill '{pair.Key}' changed from {pair.Value.PreviousValue} to {pair.Value.CurrentValue}.", LogLevel.Trace);
                            this.Events.Player_LeveledUp.Raise(new EventArgsLevelUp(pair.Key, pair.Value.CurrentValue));
                        }

                        // raise player inventory changed
                        ItemStackChange[] changedItems = curPlayer.GetInventoryChanges().ToArray();
                        if (changedItems.Any())
                        {
                            if (this.VerboseLogging)
                                this.Monitor.Log("Context: player inventory changed.", LogLevel.Trace);
                            this.Events.Player_InventoryChanged.Raise(new EventArgsInventoryChanged(Game1.player.Items, changedItems.ToList()));
                        }

                        // raise mine level changed
                        if (curPlayer.TryGetNewMineLevel(out int mineLevel))
                        {
                            if (this.VerboseLogging)
                                this.Monitor.Log($"Context: mine level changed to {mineLevel}.", LogLevel.Trace);
                            this.Events.Mine_LevelChanged.Raise(new EventArgsMineLevelChanged(curPlayer.MineLevelWatcher.PreviousValue, mineLevel));
                        }
                    }
                    this.CurrentPlayerTracker?.Reset();
                }

                // update save ID watcher
                this.SaveIdWatcher.Reset();

                /*********
                ** Game update
                *********/
                this.Input.UpdateSuppression();
                try
                {
                    base.Update(gameTime);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"An error occured in the base update loop: {ex.GetLogSummary()}", LogLevel.Error);
                }

                /*********
                ** Update events
                *********/
                this.Events.Specialised_UnvalidatedUpdateTick.Raise();
                if (this.FirstUpdate)
                {
                    this.FirstUpdate = false;
                    this.Events.Game_FirstUpdateTick.Raise();
                }
                this.Events.Game_UpdateTick.Raise();
                if (this.CurrentUpdateTick % 2 == 0)
                    this.Events.Game_SecondUpdateTick.Raise();
                if (this.CurrentUpdateTick % 4 == 0)
                    this.Events.Game_FourthUpdateTick.Raise();
                if (this.CurrentUpdateTick % 8 == 0)
                    this.Events.Game_EighthUpdateTick.Raise();
                if (this.CurrentUpdateTick % 15 == 0)
                    this.Events.Game_QuarterSecondTick.Raise();
                if (this.CurrentUpdateTick % 30 == 0)
                    this.Events.Game_HalfSecondTick.Raise();
                if (this.CurrentUpdateTick % 60 == 0)
                    this.Events.Game_OneSecondTick.Raise();
                this.CurrentUpdateTick += 1;
                if (this.CurrentUpdateTick >= 60)
                    this.CurrentUpdateTick = 0;

                this.UpdateCrashTimer.Reset();
            }
            catch (Exception ex)
            {
                // log error
                this.Monitor.Log($"An error occured in the overridden update loop: {ex.GetLogSummary()}", LogLevel.Error);

                // exit if irrecoverable
                if (!this.UpdateCrashTimer.Decrement())
                    this.Monitor.ExitGameImmediately("the game crashed when updating, and SMAPI was unable to recover the game.");
            }
        }

        /// <summary>The method called to draw everything to the screen.</summary>
        /// <param name="gameTime">A snapshot of the game timing state.</param>
        protected override void Draw(GameTime gameTime)
        {
            Context.IsInDrawLoop = true;
            try
            {
                this.DrawImpl(gameTime);
                this.DrawCrashTimer.Reset();
            }
            catch (Exception ex)
            {
                // log error
                this.Monitor.Log($"An error occured in the overridden draw loop: {ex.GetLogSummary()}", LogLevel.Error);

                // exit if irrecoverable
                if (!this.DrawCrashTimer.Decrement())
                {
                    this.Monitor.ExitGameImmediately("the game crashed when drawing, and SMAPI was unable to recover the game.");
                    return;
                }

                // recover sprite batch
                try
                {
                    if (Game1.spriteBatch.IsOpen(this.Reflection))
                    {
                        this.Monitor.Log("Recovering sprite batch from error...", LogLevel.Trace);
                        Game1.spriteBatch.End();
                    }
                }
                catch (Exception innerEx)
                {
                    this.Monitor.Log($"Could not recover sprite batch state: {innerEx.GetLogSummary()}", LogLevel.Error);
                }
            }
            Context.IsInDrawLoop = false;
        }

        /// <summary>Replicate the game's draw logic with some changes for SMAPI.</summary>
        /// <param name="gameTime">A snapshot of the game timing state.</param>
        /// <remarks>This implementation is identical to <see cref="Game1.Draw"/>, except for try..catch around menu draw code, private field references replaced by wrappers, and added events.</remarks>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "copied from game code as-is")]
        [SuppressMessage("ReSharper", "LocalVariableHidesMember", Justification = "copied from game code as-is")]
        [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "copied from game code as-is")]
        [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue", Justification = "copied from game code as-is")]
        [SuppressMessage("ReSharper", "RedundantCast", Justification = "copied from game code as-is")]
        [SuppressMessage("ReSharper", "RedundantExplicitNullableCreation", Justification = "copied from game code as-is")]
        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod", Justification = "copied from game code as-is")]
        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField", Justification = "copied from game code as-is")]
        [SuppressMessage("SMAPI.CommonErrors", "AvoidImplicitNetFieldCast", Justification = "copied from game code as-is")]
        private void DrawImpl(GameTime gameTime)
        {
            if (Game1.debugMode)
            {
                if (Game1._fpsStopwatch.IsRunning)
                {
                    float totalSeconds = (float)Game1._fpsStopwatch.Elapsed.TotalSeconds;
                    Game1._fpsList.Add(totalSeconds);
                    while (Game1._fpsList.Count >= 120)
                        Game1._fpsList.RemoveAt(0);
                    float num = 0.0f;
                    foreach (float fps in Game1._fpsList)
                        num += fps;
                    Game1._fps = (float)(1.0 / ((double)num / (double)Game1._fpsList.Count));
                }
                Game1._fpsStopwatch.Restart();
            }
            else
            {
                if (Game1._fpsStopwatch.IsRunning)
                    Game1._fpsStopwatch.Reset();
                Game1._fps = 0.0f;
                Game1._fpsList.Clear();
            }
            if (Game1._newDayTask != null)
            {
                this.GraphicsDevice.Clear(this.bgColor);
                //base.Draw(gameTime);
            }
            else
            {
                if ((double)Game1.options.zoomLevel != 1.0)
                    this.GraphicsDevice.SetRenderTarget(this.screen);
                if (this.IsSaving)
                {
                    this.GraphicsDevice.Clear(this.bgColor);
                    IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
                    if (activeClickableMenu != null)
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        try
                        {
                            this.Events.Graphics_OnPreRenderGuiEvent.Raise();
                            activeClickableMenu.draw(Game1.spriteBatch);
                            this.Events.Graphics_OnPostRenderGuiEvent.Raise();
                        }
                        catch (Exception ex)
                        {
                            this.Monitor.Log($"The {activeClickableMenu.GetType().FullName} menu crashed while drawing itself during save. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                            activeClickableMenu.exitThisMenu();
                        }
                        this.RaisePostRender();
                        Game1.spriteBatch.End();
                    }
                    if (Game1.overlayMenu != null)
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        Game1.overlayMenu.draw(Game1.spriteBatch);
                        Game1.spriteBatch.End();
                    }
                    //base.Draw(gameTime);
                    this.renderScreenBuffer();
                }
                else
                {
                    this.GraphicsDevice.Clear(this.bgColor);
                    if (Game1.activeClickableMenu != null && Game1.options.showMenuBackground && Game1.activeClickableMenu.showWithoutTransparencyIfOptionIsSet())
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        try
                        {
                            Game1.activeClickableMenu.drawBackground(Game1.spriteBatch);
                            this.Events.Graphics_OnPreRenderGuiEvent.Raise();
                            Game1.activeClickableMenu.draw(Game1.spriteBatch);
                            this.Events.Graphics_OnPostRenderGuiEvent.Raise();
                        }
                        catch (Exception ex)
                        {
                            this.Monitor.Log($"The {Game1.activeClickableMenu.GetType().FullName} menu crashed while drawing itself. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                            Game1.activeClickableMenu.exitThisMenu();
                        }
                        this.RaisePostRender();
                        Game1.spriteBatch.End();
                        if ((double)Game1.options.zoomLevel != 1.0)
                        {
                            this.GraphicsDevice.SetRenderTarget((RenderTarget2D)null);
                            this.GraphicsDevice.Clear(this.bgColor);
                            Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                            Game1.spriteBatch.Draw((Texture2D)this.screen, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(this.screen.Bounds), Color.White, 0.0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                            Game1.spriteBatch.End();
                        }
                        this.drawOverlays(Game1.spriteBatch);
                    }
                    else if ((int)Game1.gameMode == 11)
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3685"), new Vector2(16f, 16f), Color.HotPink);
                        Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3686"), new Vector2(16f, 32f), new Color(0, (int)byte.MaxValue, 0));
                        Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.parseText(Game1.errorMessage, Game1.dialogueFont, Game1.graphics.GraphicsDevice.Viewport.Width), new Vector2(16f, 48f), Color.White);
                        this.RaisePostRender();
                        Game1.spriteBatch.End();
                    }
                    else if (Game1.currentMinigame != null)
                    {
                        Game1.currentMinigame.draw(Game1.spriteBatch);
                        if (Game1.globalFade && !Game1.menuUp && (!Game1.nameSelectUp || Game1.messagePause))
                        {
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((int)Game1.gameMode == 0 ? 1f - Game1.fadeToBlackAlpha : Game1.fadeToBlackAlpha));
                            Game1.spriteBatch.End();
                        }
                        this.RaisePostRender(needsNewBatch: true);
                        if ((double)Game1.options.zoomLevel != 1.0)
                        {
                            this.GraphicsDevice.SetRenderTarget((RenderTarget2D)null);
                            this.GraphicsDevice.Clear(this.bgColor);
                            Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                            Game1.spriteBatch.Draw((Texture2D)this.screen, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(this.screen.Bounds), Color.White, 0.0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                            Game1.spriteBatch.End();
                        }
                        this.drawOverlays(Game1.spriteBatch);
                    }
                    else if (Game1.showingEndOfNightStuff)
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        if (Game1.activeClickableMenu != null)
                        {
                            try
                            {
                                this.Events.Graphics_OnPreRenderGuiEvent.Raise();
                                Game1.activeClickableMenu.draw(Game1.spriteBatch);
                                this.Events.Graphics_OnPostRenderGuiEvent.Raise();
                            }
                            catch (Exception ex)
                            {
                                this.Monitor.Log($"The {Game1.activeClickableMenu.GetType().FullName} menu crashed while drawing itself during end-of-night-stuff. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                                Game1.activeClickableMenu.exitThisMenu();
                            }
                        }
                        this.RaisePostRender();
                        Game1.spriteBatch.End();
                        if ((double)Game1.options.zoomLevel != 1.0)
                        {
                            this.GraphicsDevice.SetRenderTarget((RenderTarget2D)null);
                            this.GraphicsDevice.Clear(this.bgColor);
                            Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                            Game1.spriteBatch.Draw((Texture2D)this.screen, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(this.screen.Bounds), Color.White, 0.0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                            Game1.spriteBatch.End();
                        }
                        this.drawOverlays(Game1.spriteBatch);
                    }
                    else if ((int)Game1.gameMode == 6 || (int)Game1.gameMode == 3 && Game1.currentLocation == null)
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        string str1 = "";
                        for (int index = 0; (double)index < gameTime.TotalGameTime.TotalMilliseconds % 999.0 / 333.0; ++index)
                            str1 += ".";
                        string str2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3688");
                        string s = str2 + str1;
                        string str3 = str2 + "... ";
                        int widthOfString = SpriteText.getWidthOfString(str3);
                        int height = 64;
                        int x = 64;
                        int y = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - height;
                        SpriteText.drawString(Game1.spriteBatch, s, x, y, 999999, widthOfString, height, 1f, 0.88f, false, 0, str3, -1);
                        Game1.spriteBatch.End();
                        if ((double)Game1.options.zoomLevel != 1.0)
                        {
                            this.GraphicsDevice.SetRenderTarget((RenderTarget2D)null);
                            this.GraphicsDevice.Clear(this.bgColor);
                            Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                            Game1.spriteBatch.Draw((Texture2D)this.screen, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(this.screen.Bounds), Color.White, 0.0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                            Game1.spriteBatch.End();
                        }
                        this.drawOverlays(Game1.spriteBatch);
                        //base.Draw(gameTime);
                    }
                    else
                    {
                        Microsoft.Xna.Framework.Rectangle rectangle;
                        if ((int)Game1.gameMode == 0)
                        {
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        }
                        else
                        {
                            if (Game1.drawLighting)
                            {
                                this.GraphicsDevice.SetRenderTarget(Game1.lightmap);
                                this.GraphicsDevice.Clear(Color.White * 0.0f);
                                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                                Game1.spriteBatch.Draw(Game1.staminaRect, Game1.lightmap.Bounds, Game1.currentLocation.Name.StartsWith("UndergroundMine") ? Game1.mine.getLightingColor(gameTime) : (Game1.ambientLight.Equals(Color.White) || Game1.isRaining && (bool)((NetFieldBase<bool, NetBool>)Game1.currentLocation.isOutdoors) ? Game1.outdoorLight : Game1.ambientLight));
                                for (int index = 0; index < Game1.currentLightSources.Count; ++index)
                                {
                                    if (Utility.isOnScreen((Vector2)((NetFieldBase<Vector2, NetVector2>)Game1.currentLightSources.ElementAt<LightSource>(index).position), (int)((double)(float)((NetFieldBase<float, NetFloat>)Game1.currentLightSources.ElementAt<LightSource>(index).radius) * 64.0 * 4.0)))
                                        Game1.spriteBatch.Draw(Game1.currentLightSources.ElementAt<LightSource>(index).lightTexture, Game1.GlobalToLocal(Game1.viewport, (Vector2)((NetFieldBase<Vector2, NetVector2>)Game1.currentLightSources.ElementAt<LightSource>(index).position)) / (float)(Game1.options.lightingQuality / 2), new Microsoft.Xna.Framework.Rectangle?(Game1.currentLightSources.ElementAt<LightSource>(index).lightTexture.Bounds), (Color)((NetFieldBase<Color, NetColor>)Game1.currentLightSources.ElementAt<LightSource>(index).color), 0.0f, new Vector2((float)Game1.currentLightSources.ElementAt<LightSource>(index).lightTexture.Bounds.Center.X, (float)Game1.currentLightSources.ElementAt<LightSource>(index).lightTexture.Bounds.Center.Y), (float)((NetFieldBase<float, NetFloat>)Game1.currentLightSources.ElementAt<LightSource>(index).radius) / (float)(Game1.options.lightingQuality / 2), SpriteEffects.None, 0.9f);
                                }
                                Game1.spriteBatch.End();
                                this.GraphicsDevice.SetRenderTarget((double)Game1.options.zoomLevel == 1.0 ? (RenderTarget2D)null : this.screen);
                            }
                            if (Game1.bloomDay && Game1.bloom != null)
                                Game1.bloom.BeginDraw();
                            this.GraphicsDevice.Clear(this.bgColor);
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            this.Events.Graphics_OnPreRenderEvent.Raise();
                            if (Game1.background != null)
                                Game1.background.draw(Game1.spriteBatch);
                            Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                            Game1.currentLocation.Map.GetLayer("Back").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4);
                            Game1.currentLocation.drawWater(Game1.spriteBatch);
                            IEnumerable<Farmer> source = Game1.currentLocation.farmers;
                            if (Game1.currentLocation.currentEvent != null && !Game1.currentLocation.currentEvent.isFestival && Game1.currentLocation.currentEvent.farmerActors.Count > 0)
                                source = (IEnumerable<Farmer>)Game1.currentLocation.currentEvent.farmerActors;
                            IEnumerable<Farmer> farmers = source.Where<Farmer>((Func<Farmer, bool>)(farmer =>
                            {
                                if (!farmer.IsLocalPlayer)
                                    return !(bool)((NetFieldBase<bool, NetBool>)farmer.hidden);
                                return true;
                            }));
                            if (!Game1.currentLocation.shouldHideCharacters())
                            {
                                if (Game1.CurrentEvent == null)
                                {
                                    foreach (NPC character in Game1.currentLocation.characters)
                                    {
                                        if (!(bool)((NetFieldBase<bool, NetBool>)character.swimming) && !character.HideShadow && (!character.IsInvisible && !Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(character.getTileLocation())))
                                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, character.Position + new Vector2((float)(character.Sprite.SpriteWidth * 4) / 2f, (float)(character.GetBoundingBox().Height + (character.IsMonster ? 0 : 12)))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (float)(4.0 + (double)character.yJumpOffset / 40.0) * (float)((NetFieldBase<float, NetFloat>)character.scale), SpriteEffects.None, Math.Max(0.0f, (float)character.getStandingY() / 10000f) - 1E-06f);
                                    }
                                }
                                else
                                {
                                    foreach (NPC actor in Game1.CurrentEvent.actors)
                                    {
                                        if (!(bool)((NetFieldBase<bool, NetBool>)actor.swimming) && !actor.HideShadow && !Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(actor.getTileLocation()))
                                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, actor.Position + new Vector2((float)(actor.Sprite.SpriteWidth * 4) / 2f, (float)(actor.GetBoundingBox().Height + (actor.IsMonster ? 0 : (actor.Sprite.SpriteHeight <= 16 ? -4 : 12))))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (float)(4.0 + (double)actor.yJumpOffset / 40.0) * (float)((NetFieldBase<float, NetFloat>)actor.scale), SpriteEffects.None, Math.Max(0.0f, (float)actor.getStandingY() / 10000f) - 1E-06f);
                                    }
                                }
                                foreach (Farmer farmer in farmers)
                                {
                                    if (!(bool)((NetFieldBase<bool, NetBool>)farmer.swimming) && !farmer.isRidingHorse() && (Game1.currentLocation == null || !Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(farmer.getTileLocation())))
                                    {
                                        SpriteBatch spriteBatch = Game1.spriteBatch;
                                        Texture2D shadowTexture = Game1.shadowTexture;
                                        Vector2 local = Game1.GlobalToLocal(farmer.Position + new Vector2(32f, 24f));
                                        Microsoft.Xna.Framework.Rectangle? sourceRectangle = new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds);
                                        Color white = Color.White;
                                        double num1 = 0.0;
                                        Microsoft.Xna.Framework.Rectangle bounds = Game1.shadowTexture.Bounds;
                                        double x = (double)bounds.Center.X;
                                        bounds = Game1.shadowTexture.Bounds;
                                        double y = (double)bounds.Center.Y;
                                        Vector2 origin = new Vector2((float)x, (float)y);
                                        double num2 = 4.0 - (!farmer.running && !farmer.UsingTool || farmer.FarmerSprite.currentAnimationIndex <= 1 ? 0.0 : (double)Math.Abs(FarmerRenderer.featureYOffsetPerFrame[farmer.FarmerSprite.CurrentFrame]) * 0.5);
                                        int num3 = 0;
                                        double num4 = 0.0;
                                        spriteBatch.Draw(shadowTexture, local, sourceRectangle, white, (float)num1, origin, (float)num2, (SpriteEffects)num3, (float)num4);
                                    }
                                }
                            }
                            Game1.currentLocation.Map.GetLayer("Buildings").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4);
                            Game1.mapDisplayDevice.EndScene();
                            Game1.spriteBatch.End();
                            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            if (!Game1.currentLocation.shouldHideCharacters())
                            {
                                if (Game1.CurrentEvent == null)
                                {
                                    foreach (NPC character in Game1.currentLocation.characters)
                                    {
                                        if (!(bool)((NetFieldBase<bool, NetBool>)character.swimming) && !character.HideShadow && Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(character.getTileLocation()))
                                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, character.Position + new Vector2((float)(character.Sprite.SpriteWidth * 4) / 2f, (float)(character.GetBoundingBox().Height + (character.IsMonster ? 0 : 12)))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (float)(4.0 + (double)character.yJumpOffset / 40.0) * (float)((NetFieldBase<float, NetFloat>)character.scale), SpriteEffects.None, Math.Max(0.0f, (float)character.getStandingY() / 10000f) - 1E-06f);
                                    }
                                }
                                else
                                {
                                    foreach (NPC actor in Game1.CurrentEvent.actors)
                                    {
                                        if (!(bool)((NetFieldBase<bool, NetBool>)actor.swimming) && !actor.HideShadow && Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(actor.getTileLocation()))
                                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, actor.Position + new Vector2((float)(actor.Sprite.SpriteWidth * 4) / 2f, (float)(actor.GetBoundingBox().Height + (actor.IsMonster ? 0 : 12)))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (float)(4.0 + (double)actor.yJumpOffset / 40.0) * (float)((NetFieldBase<float, NetFloat>)actor.scale), SpriteEffects.None, Math.Max(0.0f, (float)actor.getStandingY() / 10000f) - 1E-06f);
                                    }
                                }
                                foreach (Farmer farmer in farmers)
                                {
                                    if (!(bool)((NetFieldBase<bool, NetBool>)farmer.swimming) && !farmer.isRidingHorse() && (Game1.currentLocation != null && Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(farmer.getTileLocation())))
                                    {
                                        SpriteBatch spriteBatch = Game1.spriteBatch;
                                        Texture2D shadowTexture = Game1.shadowTexture;
                                        Vector2 local = Game1.GlobalToLocal(farmer.Position + new Vector2(32f, 24f));
                                        Microsoft.Xna.Framework.Rectangle? sourceRectangle = new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds);
                                        Color white = Color.White;
                                        double num1 = 0.0;
                                        Microsoft.Xna.Framework.Rectangle bounds = Game1.shadowTexture.Bounds;
                                        double x = (double)bounds.Center.X;
                                        bounds = Game1.shadowTexture.Bounds;
                                        double y = (double)bounds.Center.Y;
                                        Vector2 origin = new Vector2((float)x, (float)y);
                                        double num2 = 4.0 - (!farmer.running && !farmer.UsingTool || farmer.FarmerSprite.currentAnimationIndex <= 1 ? 0.0 : (double)Math.Abs(FarmerRenderer.featureYOffsetPerFrame[farmer.FarmerSprite.CurrentFrame]) * 0.5);
                                        int num3 = 0;
                                        double num4 = 0.0;
                                        spriteBatch.Draw(shadowTexture, local, sourceRectangle, white, (float)num1, origin, (float)num2, (SpriteEffects)num3, (float)num4);
                                    }
                                }
                            }
                            if ((Game1.eventUp || Game1.killScreen) && (!Game1.killScreen && Game1.currentLocation.currentEvent != null))
                                Game1.currentLocation.currentEvent.draw(Game1.spriteBatch);
                            if (Game1.player.currentUpgrade != null && Game1.player.currentUpgrade.daysLeftTillUpgradeDone <= 3 && Game1.currentLocation.Name.Equals("Farm"))
                                Game1.spriteBatch.Draw(Game1.player.currentUpgrade.workerTexture, Game1.GlobalToLocal(Game1.viewport, Game1.player.currentUpgrade.positionOfCarpenter), new Microsoft.Xna.Framework.Rectangle?(Game1.player.currentUpgrade.getSourceRectangle()), Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, (float)(((double)Game1.player.currentUpgrade.positionOfCarpenter.Y + 48.0) / 10000.0));
                            Game1.currentLocation.draw(Game1.spriteBatch);
                            if (Game1.eventUp && Game1.currentLocation.currentEvent != null)
                            {
                                string messageToScreen = Game1.currentLocation.currentEvent.messageToScreen;
                            }
                            if (Game1.player.ActiveObject == null && (Game1.player.UsingTool || Game1.pickingTool) && (Game1.player.CurrentTool != null && (!Game1.player.CurrentTool.Name.Equals("Seeds") || Game1.pickingTool)))
                                Game1.drawTool(Game1.player);
                            if (Game1.currentLocation.Name.Equals("Farm"))
                                this.drawFarmBuildings();
                            if (Game1.tvStation >= 0)
                                Game1.spriteBatch.Draw(Game1.tvStationTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(400f, 160f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(Game1.tvStation * 24, 0, 24, 15)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
                            if (Game1.panMode)
                            {
                                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)Math.Floor((double)(Game1.getOldMouseX() + Game1.viewport.X) / 64.0) * 64 - Game1.viewport.X, (int)Math.Floor((double)(Game1.getOldMouseY() + Game1.viewport.Y) / 64.0) * 64 - Game1.viewport.Y, 64, 64), Color.Lime * 0.75f);
                                foreach (Warp warp in (NetList<Warp, NetRef<Warp>>)Game1.currentLocation.warps)
                                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(warp.X * 64 - Game1.viewport.X, warp.Y * 64 - Game1.viewport.Y, 64, 64), Color.Red * 0.75f);
                            }
                            Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                            Game1.currentLocation.Map.GetLayer("Front").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4);
                            Game1.mapDisplayDevice.EndScene();
                            Game1.currentLocation.drawAboveFrontLayer(Game1.spriteBatch);
                            Game1.spriteBatch.End();
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            if (Game1.displayFarmer && Game1.player.ActiveObject != null && ((bool)((NetFieldBase<bool, NetBool>)Game1.player.ActiveObject.bigCraftable) && this.checkBigCraftableBoundariesForFrontLayer()) && Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.getStandingX(), Game1.player.getStandingY()), Game1.viewport.Size) == null)
                                Game1.drawPlayerHeldObject(Game1.player);
                            else if (Game1.displayFarmer && Game1.player.ActiveObject != null)
                            {
                                if (Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location((int)Game1.player.Position.X, (int)Game1.player.Position.Y - 38), Game1.viewport.Size) == null || Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location((int)Game1.player.Position.X, (int)Game1.player.Position.Y - 38), Game1.viewport.Size).TileIndexProperties.ContainsKey("FrontAlways"))
                                {
                                    Layer layer1 = Game1.currentLocation.Map.GetLayer("Front");
                                    rectangle = Game1.player.GetBoundingBox();
                                    Location mapDisplayLocation1 = new Location(rectangle.Right, (int)Game1.player.Position.Y - 38);
                                    Size size1 = Game1.viewport.Size;
                                    if (layer1.PickTile(mapDisplayLocation1, size1) != null)
                                    {
                                        Layer layer2 = Game1.currentLocation.Map.GetLayer("Front");
                                        rectangle = Game1.player.GetBoundingBox();
                                        Location mapDisplayLocation2 = new Location(rectangle.Right, (int)Game1.player.Position.Y - 38);
                                        Size size2 = Game1.viewport.Size;
                                        if (layer2.PickTile(mapDisplayLocation2, size2).TileIndexProperties.ContainsKey("FrontAlways"))
                                            goto label_140;
                                    }
                                    else
                                        goto label_140;
                                }
                                Game1.drawPlayerHeldObject(Game1.player);
                            }
                            label_140:
                            if ((Game1.player.UsingTool || Game1.pickingTool) && Game1.player.CurrentTool != null && ((!Game1.player.CurrentTool.Name.Equals("Seeds") || Game1.pickingTool) && (Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.getStandingX(), (int)Game1.player.Position.Y - 38), Game1.viewport.Size) != null && Game1.currentLocation.Map.GetLayer("Front").PickTile(new Location(Game1.player.getStandingX(), Game1.player.getStandingY()), Game1.viewport.Size) == null)))
                                Game1.drawTool(Game1.player);
                            if (Game1.currentLocation.Map.GetLayer("AlwaysFront") != null)
                            {
                                Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                                Game1.currentLocation.Map.GetLayer("AlwaysFront").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4);
                                Game1.mapDisplayDevice.EndScene();
                            }
                            if ((double)Game1.toolHold > 400.0 && Game1.player.CurrentTool.UpgradeLevel >= 1 && Game1.player.canReleaseTool)
                            {
                                Color color = Color.White;
                                switch ((int)((double)Game1.toolHold / 600.0) + 2)
                                {
                                    case 1:
                                        color = Tool.copperColor;
                                        break;
                                    case 2:
                                        color = Tool.steelColor;
                                        break;
                                    case 3:
                                        color = Tool.goldColor;
                                        break;
                                    case 4:
                                        color = Tool.iridiumColor;
                                        break;
                                }
                                Game1.spriteBatch.Draw(Game1.littleEffect, new Microsoft.Xna.Framework.Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X - 2, (int)Game1.player.getLocalPosition(Game1.viewport).Y - (Game1.player.CurrentTool.Name.Equals("Watering Can") ? 0 : 64) - 2, (int)((double)Game1.toolHold % 600.0 * 0.0799999982118607) + 4, 12), Color.Black);
                                Game1.spriteBatch.Draw(Game1.littleEffect, new Microsoft.Xna.Framework.Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X, (int)Game1.player.getLocalPosition(Game1.viewport).Y - (Game1.player.CurrentTool.Name.Equals("Watering Can") ? 0 : 64), (int)((double)Game1.toolHold % 600.0 * 0.0799999982118607), 8), color);
                            }
                            if (Game1.isDebrisWeather && Game1.currentLocation.IsOutdoors && (!(bool)((NetFieldBase<bool, NetBool>)Game1.currentLocation.ignoreDebrisWeather) && !Game1.currentLocation.Name.Equals("Desert")) && Game1.viewport.X > -10)
                            {
                                foreach (WeatherDebris weatherDebris in Game1.debrisWeather)
                                    weatherDebris.draw(Game1.spriteBatch);
                            }
                            if (Game1.farmEvent != null)
                                Game1.farmEvent.draw(Game1.spriteBatch);
                            if ((double)Game1.currentLocation.LightLevel > 0.0 && Game1.timeOfDay < 2000)
                                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * Game1.currentLocation.LightLevel);
                            if (Game1.screenGlow)
                                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Game1.screenGlowColor * Game1.screenGlowAlpha);
                            Game1.currentLocation.drawAboveAlwaysFrontLayer(Game1.spriteBatch);
                            if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is FishingRod && ((Game1.player.CurrentTool as FishingRod).isTimingCast || (double)(Game1.player.CurrentTool as FishingRod).castingChosenCountdown > 0.0 || ((Game1.player.CurrentTool as FishingRod).fishCaught || (Game1.player.CurrentTool as FishingRod).showingTreasure)))
                                Game1.player.CurrentTool.draw(Game1.spriteBatch);
                            if (Game1.isRaining && Game1.currentLocation.IsOutdoors && (!Game1.currentLocation.Name.Equals("Desert") && !(Game1.currentLocation is Summit)) && (!Game1.eventUp || Game1.currentLocation.isTileOnMap(new Vector2((float)(Game1.viewport.X / 64), (float)(Game1.viewport.Y / 64)))))
                            {
                                for (int index = 0; index < Game1.rainDrops.Length; ++index)
                                    Game1.spriteBatch.Draw(Game1.rainTexture, Game1.rainDrops[index].position, new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.rainTexture, Game1.rainDrops[index].frame, -1, -1)), Color.White);
                            }
                            Game1.spriteBatch.End();
                            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            if (Game1.eventUp && Game1.currentLocation.currentEvent != null)
                            {
                                foreach (NPC actor in Game1.currentLocation.currentEvent.actors)
                                {
                                    if (actor.isEmoting)
                                    {
                                        Vector2 localPosition = actor.getLocalPosition(Game1.viewport);
                                        localPosition.Y -= 140f;
                                        if (actor.Age == 2)
                                            localPosition.Y += 32f;
                                        else if (actor.Gender == 1)
                                            localPosition.Y += 10f;
                                        Game1.spriteBatch.Draw(Game1.emoteSpriteSheet, localPosition, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(actor.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, actor.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)actor.getStandingY() / 10000f);
                                    }
                                }
                            }
                            Game1.spriteBatch.End();
                            if (Game1.drawLighting)
                            {
                                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, this.lightingBlend, SamplerState.LinearClamp, (DepthStencilState)null, (RasterizerState)null);
                                Game1.spriteBatch.Draw((Texture2D)Game1.lightmap, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(Game1.lightmap.Bounds), Color.White, 0.0f, Vector2.Zero, (float)(Game1.options.lightingQuality / 2), SpriteEffects.None, 1f);
                                if (Game1.isRaining && (bool)((NetFieldBase<bool, NetBool>)Game1.currentLocation.isOutdoors) && !(Game1.currentLocation is Desert))
                                    Game1.spriteBatch.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.OrangeRed * 0.45f);
                                Game1.spriteBatch.End();
                            }
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            if (Game1.drawGrid)
                            {
                                int x1 = -Game1.viewport.X % 64;
                                float num1 = (float)(-Game1.viewport.Y % 64);
                                int x2 = x1;
                                while (x2 < Game1.graphics.GraphicsDevice.Viewport.Width)
                                {
                                    Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(x2, (int)num1, 1, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Red * 0.5f);
                                    x2 += 64;
                                }
                                float num2 = num1;
                                while ((double)num2 < (double)Game1.graphics.GraphicsDevice.Viewport.Height)
                                {
                                    Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(x1, (int)num2, Game1.graphics.GraphicsDevice.Viewport.Width, 1), Color.Red * 0.5f);
                                    num2 += 64f;
                                }
                            }
                            if (Game1.currentBillboard != 0)
                                this.drawBillboard();
                            if ((Game1.displayHUD || Game1.eventUp) && (Game1.currentBillboard == 0 && (int)Game1.gameMode == 3) && (!Game1.freezeControls && !Game1.panMode && !Game1.HostPaused))
                            {
                                this.Events.Graphics_OnPreRenderHudEvent.Raise();
                                this.drawHUD();
                                this.Events.Graphics_OnPostRenderHudEvent.Raise();
                            }
                            else if (Game1.activeClickableMenu == null && Game1.farmEvent == null)
                                Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2((float)Game1.getOldMouseX(), (float)Game1.getOldMouseY()), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)(4.0 + (double)Game1.dialogueButtonScale / 150.0), SpriteEffects.None, 1f);
                            if (Game1.hudMessages.Count > 0 && (!Game1.eventUp || Game1.isFestival()))
                            {
                                for (int i = Game1.hudMessages.Count - 1; i >= 0; --i)
                                    Game1.hudMessages[i].draw(Game1.spriteBatch, i);
                            }
                        }
                        if (Game1.farmEvent != null)
                            Game1.farmEvent.draw(Game1.spriteBatch);
                        if (Game1.dialogueUp && !Game1.nameSelectUp && !Game1.messagePause && (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is DialogueBox)))
                            this.drawDialogueBox();
                        if (Game1.progressBar)
                        {
                            SpriteBatch spriteBatch1 = Game1.spriteBatch;
                            Texture2D fadeToBlackRect = Game1.fadeToBlackRect;
                            int x1 = (Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Game1.dialogueWidth) / 2;
                            rectangle = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
                            int y1 = rectangle.Bottom - 128;
                            int dialogueWidth = Game1.dialogueWidth;
                            int height1 = 32;
                            Microsoft.Xna.Framework.Rectangle destinationRectangle1 = new Microsoft.Xna.Framework.Rectangle(x1, y1, dialogueWidth, height1);
                            Color lightGray = Color.LightGray;
                            spriteBatch1.Draw(fadeToBlackRect, destinationRectangle1, lightGray);
                            SpriteBatch spriteBatch2 = Game1.spriteBatch;
                            Texture2D staminaRect = Game1.staminaRect;
                            int x2 = (Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Game1.dialogueWidth) / 2;
                            rectangle = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
                            int y2 = rectangle.Bottom - 128;
                            int width = (int)((double)Game1.pauseAccumulator / (double)Game1.pauseTime * (double)Game1.dialogueWidth);
                            int height2 = 32;
                            Microsoft.Xna.Framework.Rectangle destinationRectangle2 = new Microsoft.Xna.Framework.Rectangle(x2, y2, width, height2);
                            Color dimGray = Color.DimGray;
                            spriteBatch2.Draw(staminaRect, destinationRectangle2, dimGray);
                        }
                        if (Game1.eventUp && Game1.currentLocation != null && Game1.currentLocation.currentEvent != null)
                            Game1.currentLocation.currentEvent.drawAfterMap(Game1.spriteBatch);
                        if (Game1.isRaining && Game1.currentLocation != null && ((bool)((NetFieldBase<bool, NetBool>)Game1.currentLocation.isOutdoors) && !(Game1.currentLocation is Desert)))
                            Game1.spriteBatch.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Blue * 0.2f);
                        if ((Game1.fadeToBlack || Game1.globalFade) && !Game1.menuUp && (!Game1.nameSelectUp || Game1.messagePause))
                            Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((int)Game1.gameMode == 0 ? 1f - Game1.fadeToBlackAlpha : Game1.fadeToBlackAlpha));
                        else if ((double)Game1.flashAlpha > 0.0)
                        {
                            if (Game1.options.screenFlash)
                                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.White * Math.Min(1f, Game1.flashAlpha));
                            Game1.flashAlpha -= 0.1f;
                        }
                        if ((Game1.messagePause || Game1.globalFade) && Game1.dialogueUp)
                            this.drawDialogueBox();
                        foreach (TemporaryAnimatedSprite overlayTempSprite in Game1.screenOverlayTempSprites)
                            overlayTempSprite.draw(Game1.spriteBatch, true, 0, 0, 1f);
                        if (Game1.debugMode)
                        {
                            StringBuilder debugStringBuilder = Game1._debugStringBuilder;
                            debugStringBuilder.Clear();
                            if (Game1.panMode)
                            {
                                debugStringBuilder.Append((Game1.getOldMouseX() + Game1.viewport.X) / 64);
                                debugStringBuilder.Append(",");
                                debugStringBuilder.Append((Game1.getOldMouseY() + Game1.viewport.Y) / 64);
                            }
                            else
                            {
                                debugStringBuilder.Append("player: ");
                                debugStringBuilder.Append(Game1.player.getStandingX() / 64);
                                debugStringBuilder.Append(", ");
                                debugStringBuilder.Append(Game1.player.getStandingY() / 64);
                            }
                            debugStringBuilder.Append(" mouseTransparency: ");
                            debugStringBuilder.Append(Game1.mouseCursorTransparency);
                            debugStringBuilder.Append(" mousePosition: ");
                            debugStringBuilder.Append(Game1.getMouseX());
                            debugStringBuilder.Append(",");
                            debugStringBuilder.Append(Game1.getMouseY());
                            debugStringBuilder.Append(Environment.NewLine);
                            debugStringBuilder.Append("debugOutput: ");
                            debugStringBuilder.Append(Game1.debugOutput);
                            Game1.spriteBatch.DrawString(Game1.smallFont, debugStringBuilder, new Vector2((float)this.GraphicsDevice.Viewport.GetTitleSafeArea().X, (float)(this.GraphicsDevice.Viewport.GetTitleSafeArea().Y + Game1.smallFont.LineSpacing * 8)), Color.Red, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
                        }
                        if (Game1.showKeyHelp)
                            Game1.spriteBatch.DrawString(Game1.smallFont, Game1.keyHelpString, new Vector2(64f, (float)(Game1.viewport.Height - 64 - (Game1.dialogueUp ? 192 + (Game1.isQuestion ? Game1.questionChoices.Count * 64 : 0) : 0)) - Game1.smallFont.MeasureString(Game1.keyHelpString).Y), Color.LightGray, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
                        if (Game1.activeClickableMenu != null)
                        {
                            try
                            {
                                this.Events.Graphics_OnPreRenderGuiEvent.Raise();
                                Game1.activeClickableMenu.draw(Game1.spriteBatch);
                                this.Events.Graphics_OnPostRenderGuiEvent.Raise();
                            }
                            catch (Exception ex)
                            {
                                this.Monitor.Log($"The {Game1.activeClickableMenu.GetType().FullName} menu crashed while drawing itself. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                                Game1.activeClickableMenu.exitThisMenu();
                            }
                        }
                        else if (Game1.farmEvent != null)
                            Game1.farmEvent.drawAboveEverything(Game1.spriteBatch);
                        if (Game1.HostPaused)
                        {
                            string s = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10378");
                            SpriteText.drawStringWithScrollBackground(Game1.spriteBatch, s, 96, 32, "", 1f, -1);
                        }
                        this.RaisePostRender();
                        Game1.spriteBatch.End();
                        this.drawOverlays(Game1.spriteBatch);
                        this.renderScreenBuffer();
                        //base.Draw(gameTime);
                    }
                }
            }
        }

        /****
        ** Methods
        ****/
        /// <summary>Perform any cleanup needed when a save is unloaded.</summary>
        private void MarkWorldNotReady()
        {
            Context.IsWorldReady = false;
            this.AfterLoadTimer = 5;
            this.RaisedAfterLoadEvent = false;
        }

        /// <summary>Raise the <see cref="GraphicsEvents.OnPostRenderEvent"/> if there are any listeners.</summary>
        /// <param name="needsNewBatch">Whether to create a new sprite batch.</param>
        private void RaisePostRender(bool needsNewBatch = false)
        {
            if (this.Events.Graphics_OnPostRenderEvent.HasListeners())
            {
                if (needsNewBatch)
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                this.Events.Graphics_OnPostRenderEvent.Raise();
                if (needsNewBatch)
                    Game1.spriteBatch.End();
            }
        }
    }
}
