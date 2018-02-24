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
        ** Properties
        *********/
        /****
        ** ContentEvents
        ****/
        /// <summary>Raised after the content language changes.</summary>
        public readonly ManagedEvent<EventArgsValueChanged<string>> Content_LocaleChanged;

        /****
        ** ControlEvents
        ****/
        /// <summary>Raised when the <see cref="KeyboardState"/> changes. That happens when the player presses or releases a key.</summary>
        public readonly ManagedEvent<EventArgsKeyboardStateChanged> Control_KeyboardChanged;

        /// <summary>Raised when the player presses a keyboard key.</summary>
        public readonly ManagedEvent<EventArgsKeyPressed> Control_KeyPressed;

        /// <summary>Raised when the player releases a keyboard key.</summary>
        public readonly ManagedEvent<EventArgsKeyPressed> Control_KeyReleased;

        /// <summary>Raised when the <see cref="MouseState"/> changes. That happens when the player moves the mouse, scrolls the mouse wheel, or presses/releases a button.</summary>
        public readonly ManagedEvent<EventArgsMouseStateChanged> Control_MouseChanged;

        /// <summary>The player pressed a controller button. This event isn't raised for trigger buttons.</summary>
        public readonly ManagedEvent<EventArgsControllerButtonPressed> Control_ControllerButtonPressed;

        /// <summary>The player released a controller button. This event isn't raised for trigger buttons.</summary>
        public readonly ManagedEvent<EventArgsControllerButtonReleased> Control_ControllerButtonReleased;

        /// <summary>The player pressed a controller trigger button.</summary>
        public readonly ManagedEvent<EventArgsControllerTriggerPressed> Control_ControllerTriggerPressed;

        /// <summary>The player released a controller trigger button.</summary>
        public readonly ManagedEvent<EventArgsControllerTriggerReleased> Control_ControllerTriggerReleased;

        /****
        ** GameEvents
        ****/
        /// <summary>Raised once after the game initialises and all <see cref="IMod.Entry"/> methods have been called.</summary>
        public readonly ManagedEvent Game_FirstUpdateTick;

        /// <summary>Raised when the game updates its state (≈60 times per second).</summary>
        public readonly ManagedEvent Game_UpdateTick;

        /// <summary>Raised every other tick (≈30 times per second).</summary>
        public readonly ManagedEvent Game_SecondUpdateTick;

        /// <summary>Raised every fourth tick (≈15 times per second).</summary>
        public readonly ManagedEvent Game_FourthUpdateTick;

        /// <summary>Raised every eighth tick (≈8 times per second).</summary>
        public readonly ManagedEvent Game_EighthUpdateTick;

        /// <summary>Raised every 15th tick (≈4 times per second).</summary>
        public readonly ManagedEvent Game_QuarterSecondTick;

        /// <summary>Raised every 30th tick (≈twice per second).</summary>
        public readonly ManagedEvent Game_HalfSecondTick;

        /// <summary>Raised every 60th tick (≈once per second).</summary>
        public readonly ManagedEvent Game_OneSecondTick;

        /****
        ** GraphicsEvents
        ****/
        /// <summary>Raised after the game window is resized.</summary>
        public readonly ManagedEvent Graphics_Resize;

        /// <summary>Raised before drawing the world to the screen.</summary>
        public readonly ManagedEvent Graphics_OnPreRenderEvent;

        /// <summary>Raised after drawing the world to the screen.</summary>
        public readonly ManagedEvent Graphics_OnPostRenderEvent;

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The HUD is available at this point, but not necessarily visible. (For example, the event is raised even if a menu is open.)</summary>
        public readonly ManagedEvent Graphics_OnPreRenderHudEvent;

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the screen. The HUD is available at this point, but not necessarily visible. (For example, the event is raised even if a menu is open.)</summary>
        public readonly ManagedEvent Graphics_OnPostRenderHudEvent;

        /// <summary>Raised before drawing a menu to the screen during a draw loop. This includes the game's internal menus like the title screen.</summary>
        public readonly ManagedEvent Graphics_OnPreRenderGuiEvent;

        /// <summary>Raised after drawing a menu to the screen during a draw loop. This includes the game's internal menus like the title screen.</summary>
        public readonly ManagedEvent Graphics_OnPostRenderGuiEvent;

        /****
        ** InputEvents
        ****/
        /// <summary>Raised when the player presses a button on the keyboard, controller, or mouse.</summary>
        public readonly ManagedEvent<EventArgsInput> Input_ButtonPressed;

        /// <summary>Raised when the player releases a keyboard key on the keyboard, controller, or mouse.</summary>
        public readonly ManagedEvent<EventArgsInput> Input_ButtonReleased;

        /****
        ** LocationEvents
        ****/
        /// <summary>Raised after the player warps to a new location.</summary>
        public readonly ManagedEvent<EventArgsCurrentLocationChanged> Location_CurrentLocationChanged;

        /// <summary>Raised after a game location is added or removed.</summary>
        public readonly ManagedEvent<EventArgsGameLocationsChanged> Location_LocationsChanged;

        /// <summary>Raised after the list of objects in the current location changes (e.g. an object is added or removed).</summary>
        public readonly ManagedEvent<EventArgsLocationObjectsChanged> Location_LocationObjectsChanged;

        /****
        ** MenuEvents
        ****/
        /// <summary>Raised after a game menu is opened or replaced with another menu. This event is not invoked when a menu is closed.</summary>
        public readonly ManagedEvent<EventArgsClickableMenuChanged> Menu_Changed;

        /// <summary>Raised after a game menu is closed.</summary>
        public readonly ManagedEvent<EventArgsClickableMenuClosed> Menu_Closed;

        /****
        ** MineEvents
        ****/
        /// <summary>Raised after the player warps to a new level of the mine.</summary>
        public readonly ManagedEvent<EventArgsMineLevelChanged> Mine_LevelChanged;

        /****
        ** PlayerEvents
        ****/
        /// <summary>Raised after the player's inventory changes in any way (added or removed item, sorted, etc).</summary>
        public readonly ManagedEvent<EventArgsInventoryChanged> Player_InventoryChanged;

        /// <summary> Raised after the player levels up a skill. This happens as soon as they level up, not when the game notifies the player after their character goes to bed.</summary>
        public readonly ManagedEvent<EventArgsLevelUp> Player_LeveledUp;

        /****
        ** SaveEvents
        ****/
        /// <summary>Raised before the game creates the save file.</summary>
        public readonly ManagedEvent Save_BeforeCreate;

        /// <summary>Raised after the game finishes creating the save file.</summary>
        public readonly ManagedEvent Save_AfterCreate;

        /// <summary>Raised before the game begins writes data to the save file.</summary>
        public readonly ManagedEvent Save_BeforeSave;

        /// <summary>Raised after the game finishes writing data to the save file.</summary>
        public readonly ManagedEvent Save_AfterSave;

        /// <summary>Raised after the player loads a save slot.</summary>
        public readonly ManagedEvent Save_AfterLoad;

        /// <summary>Raised after the game returns to the title screen.</summary>
        public readonly ManagedEvent Save_AfterReturnToTitle;

        /****
        ** SpecialisedEvents
        ****/
        /// <summary>Raised when the game updates its state (≈60 times per second), regardless of normal SMAPI validation. This event is not thread-safe and may be invoked while game logic is running asynchronously. Changes to game state in this method may crash the game or corrupt an in-progress save. Do not use this event unless you're fully aware of the context in which your code will be run. Mods using this method will trigger a stability warning in the SMAPI console.</summary>
        public readonly ManagedEvent Specialised_UnvalidatedUpdateTick;

        /****
        ** TimeEvents
        ****/
        /// <summary>Raised after the game begins a new day, including when loading a save.</summary>
        public readonly ManagedEvent Time_AfterDayStarted;

        /// <summary>Raised after the in-game clock changes.</summary>
        public readonly ManagedEvent<EventArgsIntChanged> Time_TimeOfDayChanged;


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

            // init events
            this.Content_LocaleChanged = ManageEventOf<EventArgsValueChanged<string>>(nameof(ContentEvents), nameof(ContentEvents.AfterLocaleChanged));

            this.Control_ControllerButtonPressed = ManageEventOf<EventArgsControllerButtonPressed>(nameof(ControlEvents), nameof(ControlEvents.ControllerButtonPressed));
            this.Control_ControllerButtonReleased = ManageEventOf<EventArgsControllerButtonReleased>(nameof(ControlEvents), nameof(ControlEvents.ControllerButtonReleased));
            this.Control_ControllerTriggerPressed = ManageEventOf<EventArgsControllerTriggerPressed>(nameof(ControlEvents), nameof(ControlEvents.ControllerTriggerPressed));
            this.Control_ControllerTriggerReleased = ManageEventOf<EventArgsControllerTriggerReleased>(nameof(ControlEvents), nameof(ControlEvents.ControllerTriggerReleased));
            this.Control_KeyboardChanged = ManageEventOf<EventArgsKeyboardStateChanged>(nameof(ControlEvents), nameof(ControlEvents.KeyboardChanged));
            this.Control_KeyPressed = ManageEventOf<EventArgsKeyPressed>(nameof(ControlEvents), nameof(ControlEvents.KeyPressed));
            this.Control_KeyReleased = ManageEventOf<EventArgsKeyPressed>(nameof(ControlEvents), nameof(ControlEvents.KeyReleased));
            this.Control_MouseChanged = ManageEventOf<EventArgsMouseStateChanged>(nameof(ControlEvents), nameof(ControlEvents.MouseChanged));

            this.Game_FirstUpdateTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.FirstUpdateTick));
            this.Game_UpdateTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.UpdateTick));
            this.Game_SecondUpdateTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.SecondUpdateTick));
            this.Game_FourthUpdateTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.FourthUpdateTick));
            this.Game_EighthUpdateTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.EighthUpdateTick));
            this.Game_QuarterSecondTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.QuarterSecondTick));
            this.Game_HalfSecondTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.HalfSecondTick));
            this.Game_OneSecondTick = ManageEvent(nameof(GameEvents), nameof(GameEvents.OneSecondTick));

            this.Graphics_Resize = ManageEvent(nameof(GraphicsEvents), nameof(GraphicsEvents.Resize));
            this.Graphics_OnPreRenderEvent = ManageEvent(nameof(GraphicsEvents), nameof(GraphicsEvents.OnPreRenderEvent));
            this.Graphics_OnPostRenderEvent = ManageEvent(nameof(GraphicsEvents), nameof(GraphicsEvents.OnPostRenderEvent));
            this.Graphics_OnPreRenderHudEvent = ManageEvent(nameof(GraphicsEvents), nameof(GraphicsEvents.OnPreRenderHudEvent));
            this.Graphics_OnPostRenderHudEvent = ManageEvent(nameof(GraphicsEvents), nameof(GraphicsEvents.OnPostRenderHudEvent));
            this.Graphics_OnPreRenderGuiEvent = ManageEvent(nameof(GraphicsEvents), nameof(GraphicsEvents.OnPreRenderGuiEvent));
            this.Graphics_OnPostRenderGuiEvent = ManageEvent(nameof(GraphicsEvents), nameof(GraphicsEvents.OnPostRenderGuiEvent));

            this.Input_ButtonPressed = ManageEventOf<EventArgsInput>(nameof(InputEvents), nameof(InputEvents.ButtonPressed));
            this.Input_ButtonReleased = ManageEventOf<EventArgsInput>(nameof(InputEvents), nameof(InputEvents.ButtonReleased));

            this.Location_CurrentLocationChanged = ManageEventOf<EventArgsCurrentLocationChanged>(nameof(LocationEvents), nameof(LocationEvents.CurrentLocationChanged));
            this.Location_LocationsChanged = ManageEventOf<EventArgsGameLocationsChanged>(nameof(LocationEvents), nameof(LocationEvents.LocationsChanged));
            this.Location_LocationObjectsChanged = ManageEventOf<EventArgsLocationObjectsChanged>(nameof(LocationEvents), nameof(LocationEvents.LocationObjectsChanged));

            this.Menu_Changed = ManageEventOf<EventArgsClickableMenuChanged>(nameof(MenuEvents), nameof(MenuEvents.MenuChanged));
            this.Menu_Closed = ManageEventOf<EventArgsClickableMenuClosed>(nameof(MenuEvents), nameof(MenuEvents.MenuClosed));

            this.Mine_LevelChanged = ManageEventOf<EventArgsMineLevelChanged>(nameof(MineEvents), nameof(MineEvents.MineLevelChanged));

            this.Player_InventoryChanged = ManageEventOf<EventArgsInventoryChanged>(nameof(PlayerEvents), nameof(PlayerEvents.InventoryChanged));
            this.Player_LeveledUp = ManageEventOf<EventArgsLevelUp>(nameof(PlayerEvents), nameof(PlayerEvents.LeveledUp));

            this.Save_BeforeCreate = ManageEvent(nameof(SaveEvents), nameof(SaveEvents.BeforeCreate));
            this.Save_AfterCreate = ManageEvent(nameof(SaveEvents), nameof(SaveEvents.AfterCreate));
            this.Save_BeforeSave = ManageEvent(nameof(SaveEvents), nameof(SaveEvents.BeforeSave));
            this.Save_AfterSave = ManageEvent(nameof(SaveEvents), nameof(SaveEvents.AfterSave));
            this.Save_AfterLoad = ManageEvent(nameof(SaveEvents), nameof(SaveEvents.AfterLoad));
            this.Save_AfterReturnToTitle = ManageEvent(nameof(SaveEvents), nameof(SaveEvents.AfterReturnToTitle));

            this.Specialised_UnvalidatedUpdateTick = ManageEvent(nameof(SpecialisedEvents), nameof(SpecialisedEvents.UnvalidatedUpdateTick));

            this.Time_AfterDayStarted = ManageEvent(nameof(TimeEvents), nameof(TimeEvents.AfterDayStarted));
            this.Time_TimeOfDayChanged = ManageEventOf<EventArgsIntChanged>(nameof(TimeEvents), nameof(TimeEvents.TimeOfDayChanged));
        }
    }
}
