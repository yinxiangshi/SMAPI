using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;

namespace StardewModdingAPI.Framework.Events
{
    /// <summary>Manages SMAPI events.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Private fields are deliberately named to simplify organisation.")]
    internal class EventManager
    {
        /*********
        ** Events (new)
        *********/
        /****
        ** Game loop
        ****/
        /// <summary>Raised after the game is launched, right before the first update tick.</summary>
        public readonly ManagedEvent<GameLaunchedEventArgs> GameLaunched;

        /// <summary>Raised before the game performs its overall update tick (≈60 times per second).</summary>
        public readonly ManagedEvent<UpdateTickingEventArgs> UpdateTicking;

        /// <summary>Raised after the game performs its overall update tick (≈60 times per second).</summary>
        public readonly ManagedEvent<UpdateTickedEventArgs> UpdateTicked;

        /// <summary>Raised before the game creates the save file.</summary>
        public readonly ManagedEvent<SaveCreatingEventArgs> SaveCreating;

        /// <summary>Raised after the game finishes creating the save file.</summary>
        public readonly ManagedEvent<SaveCreatedEventArgs> SaveCreated;

        /// <summary>Raised before the game begins writes data to the save file (except the initial save creation).</summary>
        public readonly ManagedEvent<SavingEventArgs> Saving;

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        public readonly ManagedEvent<SavedEventArgs> Saved;

        /// <summary>Raised after the player loads a save slot.</summary>
        public readonly ManagedEvent<SaveLoadedEventArgs> SaveLoaded;

        /// <summary>Raised after the game begins a new day, including when loading a save.</summary>
        public readonly ManagedEvent<DayStartedEventArgs> DayStarted;

        /// <summary>Raised before the game ends the current day. This happens before it starts setting up the next day and before <see cref="Saving"/>.</summary>
        public readonly ManagedEvent<DayEndingEventArgs> DayEnding;

        /****
        ** Input
        ****/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        public readonly ManagedEvent<ButtonPressedEventArgs> ButtonPressed;

        /// <summary>Raised after the player released a button on the keyboard, controller, or mouse.</summary>
        public readonly ManagedEvent<ButtonReleasedEventArgs> ButtonReleased;

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        public readonly ManagedEvent<CursorMovedEventArgs> CursorMoved;

        /// <summary>Raised after the player scrolls the mouse wheel.</summary>
        public readonly ManagedEvent<MouseWheelScrolledEventArgs> MouseWheelScrolled;

        /****
        ** World
        ****/
        /// <summary>Raised after a game location is added or removed.</summary>
        public readonly ManagedEvent<LocationListChangedEventArgs> LocationListChanged;

        /// <summary>Raised after buildings are added or removed in a location.</summary>
        public readonly ManagedEvent<BuildingListChangedEventArgs> BuildingListChanged;

        /// <summary>Raised after debris are added or removed in a location.</summary>
        public readonly ManagedEvent<DebrisListChangedEventArgs> DebrisListChanged;

        /// <summary>Raised after large terrain features (like bushes) are added or removed in a location.</summary>
        public readonly ManagedEvent<LargeTerrainFeatureListChangedEventArgs> LargeTerrainFeatureListChanged;

        /// <summary>Raised after NPCs are added or removed in a location.</summary>
        public readonly ManagedEvent<NpcListChangedEventArgs> NpcListChanged;

        /// <summary>Raised after objects are added or removed in a location.</summary>
        public readonly ManagedEvent<ObjectListChangedEventArgs> ObjectListChanged;

        /// <summary>Raised after terrain features (like floors and trees) are added or removed in a location.</summary>
        public readonly ManagedEvent<TerrainFeatureListChangedEventArgs> TerrainFeatureListChanged;


        /*********
        ** Events (old)
        *********/
        /****
        ** ContentEvents
        ****/
        /// <summary>Raised after the content language changes.</summary>
        public readonly ManagedEvent<EventArgsValueChanged<string>> Legacy_LocaleChanged;

        /****
        ** ControlEvents
        ****/
        /// <summary>Raised when the <see cref="KeyboardState"/> changes. That happens when the player presses or releases a key.</summary>
        public readonly ManagedEvent<EventArgsKeyboardStateChanged> Legacy_KeyboardChanged;

        /// <summary>Raised after the player presses a keyboard key.</summary>
        public readonly ManagedEvent<EventArgsKeyPressed> Legacy_KeyPressed;

        /// <summary>Raised after the player releases a keyboard key.</summary>
        public readonly ManagedEvent<EventArgsKeyPressed> Legacy_KeyReleased;

        /// <summary>Raised when the <see cref="MouseState"/> changes. That happens when the player moves the mouse, scrolls the mouse wheel, or presses/releases a button.</summary>
        public readonly ManagedEvent<EventArgsMouseStateChanged> Legacy_MouseChanged;

        /// <summary>The player pressed a controller button. This event isn't raised for trigger buttons.</summary>
        public readonly ManagedEvent<EventArgsControllerButtonPressed> Legacy_ControllerButtonPressed;

        /// <summary>The player released a controller button. This event isn't raised for trigger buttons.</summary>
        public readonly ManagedEvent<EventArgsControllerButtonReleased> Legacy_ControllerButtonReleased;

        /// <summary>The player pressed a controller trigger button.</summary>
        public readonly ManagedEvent<EventArgsControllerTriggerPressed> Legacy_ControllerTriggerPressed;

        /// <summary>The player released a controller trigger button.</summary>
        public readonly ManagedEvent<EventArgsControllerTriggerReleased> Legacy_ControllerTriggerReleased;

        /****
        ** GameEvents
        ****/
        /// <summary>Raised once after the game initialises and all <see cref="IMod.Entry"/> methods have been called.</summary>
        public readonly ManagedEvent Legacy_FirstUpdateTick;

        /// <summary>Raised when the game updates its state (≈60 times per second).</summary>
        public readonly ManagedEvent Legacy_UpdateTick;

        /// <summary>Raised every other tick (≈30 times per second).</summary>
        public readonly ManagedEvent Legacy_SecondUpdateTick;

        /// <summary>Raised every fourth tick (≈15 times per second).</summary>
        public readonly ManagedEvent Legacy_FourthUpdateTick;

        /// <summary>Raised every eighth tick (≈8 times per second).</summary>
        public readonly ManagedEvent Legacy_EighthUpdateTick;

        /// <summary>Raised every 15th tick (≈4 times per second).</summary>
        public readonly ManagedEvent Legacy_QuarterSecondTick;

        /// <summary>Raised every 30th tick (≈twice per second).</summary>
        public readonly ManagedEvent Legacy_HalfSecondTick;

        /// <summary>Raised every 60th tick (≈once per second).</summary>
        public readonly ManagedEvent Legacy_OneSecondTick;

        /****
        ** GraphicsEvents
        ****/
        /// <summary>Raised after the game window is resized.</summary>
        public readonly ManagedEvent Legacy_Resize;

        /// <summary>Raised before drawing the world to the screen.</summary>
        public readonly ManagedEvent Legacy_OnPreRenderEvent;

        /// <summary>Raised after drawing the world to the screen.</summary>
        public readonly ManagedEvent Legacy_OnPostRenderEvent;

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The HUD is available at this point, but not necessarily visible. (For example, the event is raised even if a menu is open.)</summary>
        public readonly ManagedEvent Legacy_OnPreRenderHudEvent;

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the screen. The HUD is available at this point, but not necessarily visible. (For example, the event is raised even if a menu is open.)</summary>
        public readonly ManagedEvent Legacy_OnPostRenderHudEvent;

        /// <summary>Raised before drawing a menu to the screen during a draw loop. This includes the game's internal menus like the title screen.</summary>
        public readonly ManagedEvent Legacy_OnPreRenderGuiEvent;

        /// <summary>Raised after drawing a menu to the screen during a draw loop. This includes the game's internal menus like the title screen.</summary>
        public readonly ManagedEvent Legacy_OnPostRenderGuiEvent;

        /****
        ** InputEvents
        ****/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        public readonly ManagedEvent<EventArgsInput> Legacy_ButtonPressed;

        /// <summary>Raised after the player releases a keyboard key on the keyboard, controller, or mouse.</summary>
        public readonly ManagedEvent<EventArgsInput> Legacy_ButtonReleased;

        /****
        ** LocationEvents
        ****/
        /// <summary>Raised after a game location is added or removed.</summary>
        public readonly ManagedEvent<EventArgsLocationsChanged> Legacy_LocationsChanged;

        /// <summary>Raised after buildings are added or removed in a location.</summary>
        public readonly ManagedEvent<EventArgsLocationBuildingsChanged> Legacy_BuildingsChanged;

        /// <summary>Raised after objects are added or removed in a location.</summary>
        public readonly ManagedEvent<EventArgsLocationObjectsChanged> Legacy_ObjectsChanged;

        /****
        ** MenuEvents
        ****/
        /// <summary>Raised after a game menu is opened or replaced with another menu. This event is not invoked when a menu is closed.</summary>
        public readonly ManagedEvent<EventArgsClickableMenuChanged> Legacy_MenuChanged;

        /// <summary>Raised after a game menu is closed.</summary>
        public readonly ManagedEvent<EventArgsClickableMenuClosed> Legacy_MenuClosed;

        /****
        ** MultiplayerEvents
        ****/
        /// <summary>Raised before the game syncs changes from other players.</summary>
        public readonly ManagedEvent Legacy_BeforeMainSync;

        /// <summary>Raised after the game syncs changes from other players.</summary>
        public readonly ManagedEvent Legacy_AfterMainSync;

        /// <summary>Raised before the game broadcasts changes to other players.</summary>
        public readonly ManagedEvent Legacy_BeforeMainBroadcast;

        /// <summary>Raised after the game broadcasts changes to other players.</summary>
        public readonly ManagedEvent Legacy_AfterMainBroadcast;

        /****
        ** MineEvents
        ****/
        /// <summary>Raised after the player warps to a new level of the mine.</summary>
        public readonly ManagedEvent<EventArgsMineLevelChanged> Legacy_MineLevelChanged;

        /****
        ** PlayerEvents
        ****/
        /// <summary>Raised after the player's inventory changes in any way (added or removed item, sorted, etc).</summary>
        public readonly ManagedEvent<EventArgsInventoryChanged> Legacy_InventoryChanged;

        /// <summary> Raised after the player levels up a skill. This happens as soon as they level up, not when the game notifies the player after their character goes to bed.</summary>
        public readonly ManagedEvent<EventArgsLevelUp> Legacy_LeveledUp;

        /// <summary>Raised after the player warps to a new location.</summary>
        public readonly ManagedEvent<EventArgsPlayerWarped> Legacy_PlayerWarped;


        /****
        ** SaveEvents
        ****/
        /// <summary>Raised before the game creates the save file.</summary>
        public readonly ManagedEvent Legacy_BeforeCreateSave;

        /// <summary>Raised after the game finishes creating the save file.</summary>
        public readonly ManagedEvent Legacy_AfterCreateSave;

        /// <summary>Raised before the game begins writes data to the save file.</summary>
        public readonly ManagedEvent Legacy_BeforeSave;

        /// <summary>Raised after the game finishes writing data to the save file.</summary>
        public readonly ManagedEvent Legacy_AfterSave;

        /// <summary>Raised after the player loads a save slot.</summary>
        public readonly ManagedEvent Legacy_AfterLoad;

        /// <summary>Raised after the game returns to the title screen.</summary>
        public readonly ManagedEvent Legacy_AfterReturnToTitle;

        /****
        ** SpecialisedEvents
        ****/
        /// <summary>Raised when the game updates its state (≈60 times per second), regardless of normal SMAPI validation. This event is not thread-safe and may be invoked while game logic is running asynchronously. Changes to game state in this method may crash the game or corrupt an in-progress save. Do not use this event unless you're fully aware of the context in which your code will be run. Mods using this method will trigger a stability warning in the SMAPI console.</summary>
        public readonly ManagedEvent Legacy_UnvalidatedUpdateTick;

        /****
        ** TimeEvents
        ****/
        /// <summary>Raised after the game begins a new day, including when loading a save.</summary>
        public readonly ManagedEvent Legacy_AfterDayStarted;

        /// <summary>Raised after the in-game clock changes.</summary>
        public readonly ManagedEvent<EventArgsIntChanged> Legacy_TimeOfDayChanged;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Writes messages to the log.</param>
        /// <param name="modRegistry">The mod registry with which to identify mods.</param>
        public EventManager(IMonitor monitor, ModRegistry modRegistry)
        {
            // create shortcut initialisers
            ManagedEvent<TEventArgs> ManageEventOf<TEventArgs>(string typeName, string eventName) => new ManagedEvent<TEventArgs>($"{typeName}.{eventName}", monitor, modRegistry);
            ManagedEvent ManageEvent(string typeName, string eventName) => new ManagedEvent($"{typeName}.{eventName}", monitor, modRegistry);

            // init events (new)
            this.GameLaunched = ManageEventOf<GameLaunchedEventArgs>(nameof(IModEvents.GameLoop), nameof(IGameLoopEvents.GameLaunched));
            this.UpdateTicking = ManageEventOf<UpdateTickingEventArgs>(nameof(IModEvents.GameLoop), nameof(IGameLoopEvents.UpdateTicking));
            this.UpdateTicked = ManageEventOf<UpdateTickedEventArgs>(nameof(IModEvents.GameLoop), nameof(IGameLoopEvents.UpdateTicked));
            this.SaveCreating = ManageEventOf<SaveCreatingEventArgs>(nameof(IModEvents.GameLoop), nameof(IGameLoopEvents.SaveCreating));
            this.SaveCreated = ManageEventOf<SaveCreatedEventArgs>(nameof(IModEvents.GameLoop), nameof(IGameLoopEvents.SaveCreated));
            this.Saving = ManageEventOf<SavingEventArgs>(nameof(IModEvents.GameLoop), nameof(IGameLoopEvents.Saving));
            this.Saved = ManageEventOf<SavedEventArgs>(nameof(IModEvents.GameLoop), nameof(IGameLoopEvents.Saved));
            this.SaveLoaded = ManageEventOf<SaveLoadedEventArgs>(nameof(IModEvents.GameLoop), nameof(IGameLoopEvents.SaveLoaded));
            this.DayStarted = ManageEventOf<DayStartedEventArgs>(nameof(IModEvents.GameLoop), nameof(IGameLoopEvents.DayStarted));
            this.DayEnding = ManageEventOf<DayEndingEventArgs>(nameof(IModEvents.GameLoop), nameof(IGameLoopEvents.DayEnding));

            this.ButtonPressed = ManageEventOf<ButtonPressedEventArgs>(nameof(IModEvents.Input), nameof(IInputEvents.ButtonPressed));
            this.ButtonReleased = ManageEventOf<ButtonReleasedEventArgs>(nameof(IModEvents.Input), nameof(IInputEvents.ButtonReleased));
            this.CursorMoved = ManageEventOf<CursorMovedEventArgs>(nameof(IModEvents.Input), nameof(IInputEvents.CursorMoved));
            this.MouseWheelScrolled = ManageEventOf<MouseWheelScrolledEventArgs>(nameof(IModEvents.Input), nameof(IInputEvents.MouseWheelScrolled));

            this.BuildingListChanged = ManageEventOf<BuildingListChangedEventArgs>(nameof(IModEvents.World), nameof(IWorldEvents.LocationListChanged));
            this.DebrisListChanged = ManageEventOf<DebrisListChangedEventArgs>(nameof(IModEvents.World), nameof(IWorldEvents.DebrisListChanged));
            this.LargeTerrainFeatureListChanged = ManageEventOf<LargeTerrainFeatureListChangedEventArgs>(nameof(IModEvents.World), nameof(IWorldEvents.LargeTerrainFeatureListChanged));
            this.LocationListChanged = ManageEventOf<LocationListChangedEventArgs>(nameof(IModEvents.World), nameof(IWorldEvents.BuildingListChanged));
            this.NpcListChanged = ManageEventOf<NpcListChangedEventArgs>(nameof(IModEvents.World), nameof(IWorldEvents.NpcListChanged));
            this.ObjectListChanged = ManageEventOf<ObjectListChangedEventArgs>(nameof(IModEvents.World), nameof(IWorldEvents.ObjectListChanged));
            this.TerrainFeatureListChanged = ManageEventOf<TerrainFeatureListChangedEventArgs>(nameof(IModEvents.World), nameof(IWorldEvents.TerrainFeatureListChanged));

            // init events (old)
            this.Legacy_LocaleChanged = ManageEventOf<EventArgsValueChanged<string>>(nameof(ContentEvents), nameof(ContentEvents.AfterLocaleChanged));

            this.Legacy_ControllerButtonPressed = ManageEventOf<EventArgsControllerButtonPressed>(nameof(ControlEvents), nameof(ControlEvents.ControllerButtonPressed));
            this.Legacy_ControllerButtonReleased = ManageEventOf<EventArgsControllerButtonReleased>(nameof(ControlEvents), nameof(ControlEvents.ControllerButtonReleased));
            this.Legacy_ControllerTriggerPressed = ManageEventOf<EventArgsControllerTriggerPressed>(nameof(ControlEvents), nameof(ControlEvents.ControllerTriggerPressed));
            this.Legacy_ControllerTriggerReleased = ManageEventOf<EventArgsControllerTriggerReleased>(nameof(ControlEvents), nameof(ControlEvents.ControllerTriggerReleased));
            this.Legacy_KeyboardChanged = ManageEventOf<EventArgsKeyboardStateChanged>(nameof(ControlEvents), nameof(ControlEvents.KeyboardChanged));
            this.Legacy_KeyPressed = ManageEventOf<EventArgsKeyPressed>(nameof(ControlEvents), nameof(ControlEvents.KeyPressed));
            this.Legacy_KeyReleased = ManageEventOf<EventArgsKeyPressed>(nameof(ControlEvents), nameof(ControlEvents.KeyReleased));
            this.Legacy_MouseChanged = ManageEventOf<EventArgsMouseStateChanged>(nameof(ControlEvents), nameof(ControlEvents.MouseChanged));

            this.Legacy_FirstUpdateTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.FirstUpdateTick));
            this.Legacy_UpdateTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.UpdateTick));
            this.Legacy_SecondUpdateTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.SecondUpdateTick));
            this.Legacy_FourthUpdateTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.FourthUpdateTick));
            this.Legacy_EighthUpdateTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.EighthUpdateTick));
            this.Legacy_QuarterSecondTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.QuarterSecondTick));
            this.Legacy_HalfSecondTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.HalfSecondTick));
            this.Legacy_OneSecondTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.OneSecondTick));

            this.Legacy_Resize = ManageEvent(nameof(GraphicsEvents), nameof(GraphicsEvents.Resize));
            this.Legacy_OnPreRenderEvent = ManageEvent(nameof(GraphicsEvents), nameof(GraphicsEvents.OnPreRenderEvent));
            this.Legacy_OnPostRenderEvent = ManageEvent(nameof(GraphicsEvents), nameof(GraphicsEvents.OnPostRenderEvent));
            this.Legacy_OnPreRenderHudEvent = ManageEvent(nameof(GraphicsEvents), nameof(GraphicsEvents.OnPreRenderHudEvent));
            this.Legacy_OnPostRenderHudEvent = ManageEvent(nameof(GraphicsEvents), nameof(GraphicsEvents.OnPostRenderHudEvent));
            this.Legacy_OnPreRenderGuiEvent = ManageEvent(nameof(GraphicsEvents), nameof(GraphicsEvents.OnPreRenderGuiEvent));
            this.Legacy_OnPostRenderGuiEvent = ManageEvent(nameof(GraphicsEvents), nameof(GraphicsEvents.OnPostRenderGuiEvent));

            this.Legacy_ButtonPressed = ManageEventOf<EventArgsInput>(nameof(InputEvents), nameof(InputEvents.ButtonPressed));
            this.Legacy_ButtonReleased = ManageEventOf<EventArgsInput>(nameof(InputEvents), nameof(InputEvents.ButtonReleased));

            this.Legacy_LocationsChanged = ManageEventOf<EventArgsLocationsChanged>(nameof(LocationEvents), nameof(LocationEvents.LocationsChanged));
            this.Legacy_BuildingsChanged = ManageEventOf<EventArgsLocationBuildingsChanged>(nameof(LocationEvents), nameof(LocationEvents.BuildingsChanged));
            this.Legacy_ObjectsChanged = ManageEventOf<EventArgsLocationObjectsChanged>(nameof(LocationEvents), nameof(LocationEvents.ObjectsChanged));

            this.Legacy_MenuChanged = ManageEventOf<EventArgsClickableMenuChanged>(nameof(MenuEvents), nameof(MenuEvents.MenuChanged));
            this.Legacy_MenuClosed = ManageEventOf<EventArgsClickableMenuClosed>(nameof(MenuEvents), nameof(MenuEvents.MenuClosed));

            this.Legacy_BeforeMainBroadcast = ManageEvent(nameof(MultiplayerEvents), nameof(MultiplayerEvents.BeforeMainBroadcast));
            this.Legacy_AfterMainBroadcast = ManageEvent(nameof(MultiplayerEvents), nameof(MultiplayerEvents.AfterMainBroadcast));
            this.Legacy_BeforeMainSync = ManageEvent(nameof(MultiplayerEvents), nameof(MultiplayerEvents.BeforeMainSync));
            this.Legacy_AfterMainSync = ManageEvent(nameof(MultiplayerEvents), nameof(MultiplayerEvents.AfterMainSync));

            this.Legacy_MineLevelChanged = ManageEventOf<EventArgsMineLevelChanged>(nameof(MineEvents), nameof(MineEvents.MineLevelChanged));

            this.Legacy_InventoryChanged = ManageEventOf<EventArgsInventoryChanged>(nameof(PlayerEvents), nameof(PlayerEvents.InventoryChanged));
            this.Legacy_LeveledUp = ManageEventOf<EventArgsLevelUp>(nameof(PlayerEvents), nameof(PlayerEvents.LeveledUp));
            this.Legacy_PlayerWarped = ManageEventOf<EventArgsPlayerWarped>(nameof(PlayerEvents), nameof(PlayerEvents.Warped));

            this.Legacy_BeforeCreateSave = ManageEvent(nameof(SaveEvents), nameof(SaveEvents.BeforeCreate));
            this.Legacy_AfterCreateSave = ManageEvent(nameof(SaveEvents), nameof(SaveEvents.AfterCreate));
            this.Legacy_BeforeSave = ManageEvent(nameof(SaveEvents), nameof(SaveEvents.BeforeSave));
            this.Legacy_AfterSave = ManageEvent(nameof(SaveEvents), nameof(SaveEvents.AfterSave));
            this.Legacy_AfterLoad = ManageEvent(nameof(SaveEvents), nameof(SaveEvents.AfterLoad));
            this.Legacy_AfterReturnToTitle = ManageEvent(nameof(SaveEvents), nameof(SaveEvents.AfterReturnToTitle));

            this.Legacy_UnvalidatedUpdateTick = ManageEvent(nameof(SpecialisedEvents), nameof(SpecialisedEvents.UnvalidatedUpdateTick));

            this.Legacy_AfterDayStarted = ManageEvent(nameof(TimeEvents), nameof(TimeEvents.AfterDayStarted));
            this.Legacy_TimeOfDayChanged = ManageEventOf<EventArgsIntChanged>(nameof(TimeEvents), nameof(TimeEvents.TimeOfDayChanged));
        }
    }
}
