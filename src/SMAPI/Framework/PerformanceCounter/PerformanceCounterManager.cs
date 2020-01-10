using System.Collections.Generic;
using StardewModdingAPI.Framework.Events;
using StardewModdingAPI.Framework.Utilities;

namespace StardewModdingAPI.Framework.PerformanceCounter
{
    internal class PerformanceCounterManager
    {
        public HashSet<EventPerformanceCounterCategory> PerformanceCounterEvents = new HashSet<EventPerformanceCounterCategory>();

        private readonly EventManager EventManager;

        public PerformanceCounterManager(EventManager eventManager)
        {
            this.EventManager = eventManager;
            this.InitializePerformanceCounterEvents();
        }

        private void InitializePerformanceCounterEvents()
        {
            this.PerformanceCounterEvents = new HashSet<EventPerformanceCounterCategory>()
            {
                new EventPerformanceCounterCategory(this.EventManager.MenuChanged, false),

                // Rendering Events
                new EventPerformanceCounterCategory(this.EventManager.Rendering, true),
                new EventPerformanceCounterCategory(this.EventManager.Rendered, true),
                new EventPerformanceCounterCategory(this.EventManager.RenderingWorld, true),
                new EventPerformanceCounterCategory(this.EventManager.RenderedWorld, true),
                new EventPerformanceCounterCategory(this.EventManager.RenderingActiveMenu, true),
                new EventPerformanceCounterCategory(this.EventManager.RenderedActiveMenu, true),
                new EventPerformanceCounterCategory(this.EventManager.RenderingHud, true),
                new EventPerformanceCounterCategory(this.EventManager.RenderedHud, true),

                new EventPerformanceCounterCategory(this.EventManager.WindowResized, false),
                new EventPerformanceCounterCategory(this.EventManager.GameLaunched, false),
                new EventPerformanceCounterCategory(this.EventManager.UpdateTicking, true),
                new EventPerformanceCounterCategory(this.EventManager.UpdateTicked, true),
                new EventPerformanceCounterCategory(this.EventManager.OneSecondUpdateTicking, true),
                new EventPerformanceCounterCategory(this.EventManager.OneSecondUpdateTicked, true),

                new EventPerformanceCounterCategory(this.EventManager.SaveCreating, false),
                new EventPerformanceCounterCategory(this.EventManager.SaveCreated, false),
                new EventPerformanceCounterCategory(this.EventManager.Saving, false),
                new EventPerformanceCounterCategory(this.EventManager.Saved, false),

                new EventPerformanceCounterCategory(this.EventManager.DayStarted, false),
                new EventPerformanceCounterCategory(this.EventManager.DayEnding, false),

                new EventPerformanceCounterCategory(this.EventManager.TimeChanged, true),

                new EventPerformanceCounterCategory(this.EventManager.ReturnedToTitle, false),

                new EventPerformanceCounterCategory(this.EventManager.ButtonPressed, true),
                new EventPerformanceCounterCategory(this.EventManager.ButtonReleased, true),
                new EventPerformanceCounterCategory(this.EventManager.CursorMoved, true),
                new EventPerformanceCounterCategory(this.EventManager.MouseWheelScrolled, true),

                new EventPerformanceCounterCategory(this.EventManager.PeerContextReceived, true),
                new EventPerformanceCounterCategory(this.EventManager.ModMessageReceived, true),
                new EventPerformanceCounterCategory(this.EventManager.PeerDisconnected, true),
                new EventPerformanceCounterCategory(this.EventManager.InventoryChanged, true),
                new EventPerformanceCounterCategory(this.EventManager.LevelChanged, true),
                new EventPerformanceCounterCategory(this.EventManager.Warped, true),

                new EventPerformanceCounterCategory(this.EventManager.LocationListChanged, true),
                new EventPerformanceCounterCategory(this.EventManager.BuildingListChanged, true),
                new EventPerformanceCounterCategory(this.EventManager.LocationListChanged, true),
                new EventPerformanceCounterCategory(this.EventManager.DebrisListChanged, true),
                new EventPerformanceCounterCategory(this.EventManager.LargeTerrainFeatureListChanged, true),
                new EventPerformanceCounterCategory(this.EventManager.NpcListChanged, true),
                new EventPerformanceCounterCategory(this.EventManager.ObjectListChanged, true),
                new EventPerformanceCounterCategory(this.EventManager.ChestInventoryChanged, true),
                new EventPerformanceCounterCategory(this.EventManager.TerrainFeatureListChanged, true),
                new EventPerformanceCounterCategory(this.EventManager.LoadStageChanged, false),
                new EventPerformanceCounterCategory(this.EventManager.UnvalidatedUpdateTicking, true),
                new EventPerformanceCounterCategory(this.EventManager.UnvalidatedUpdateTicked, true),

            };
        }
    }
}
