using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using StardewModdingAPI.Framework.Events;
using StardewModdingAPI.Framework.Utilities;

namespace StardewModdingAPI.Framework.PerformanceCounter
{
    internal class PerformanceCounterManager
    {
        public HashSet<PerformanceCounterCollection> PerformanceCounterCollections = new HashSet<PerformanceCounterCollection>();
        public List<AlertEntry> Alerts = new List<AlertEntry>();
        private readonly IMonitor Monitor;
        private readonly Stopwatch Stopwatch = new Stopwatch();

        public PerformanceCounterManager(IMonitor monitor)
        {
            this.Monitor = monitor;
        }

        public void Reset()
        {
            foreach (var performanceCounter in this.PerformanceCounterCollections)
            {
                foreach (var eventPerformanceCounter in performanceCounter.PerformanceCounters)
                {
                    eventPerformanceCounter.Value.Reset();
                }
            }
        }

        /// <summary>Print any queued messages.</summary>
        public void PrintQueued()
        {
            if (this.Alerts.Count == 0)
            {
                return;
            }
            StringBuilder sb = new StringBuilder();

            foreach (var alert in this.Alerts)
            {
                sb.AppendLine($"{alert.Collection.Name} took {alert.ExecutionTimeMilliseconds:F2}ms (exceeded threshold of {alert.Threshold:F2}ms)");

                foreach (var context in alert.Context)
                {
                    sb.AppendLine($"{context.Source}: {context.Elapsed:F2}ms");
                }
            }

            this.Alerts.Clear();

            this.Monitor.Log(sb.ToString(), LogLevel.Error);
        }

        public void BeginTrackInvocation(string collectionName)
        {
            this.GetOrCreateCollectionByName(collectionName).BeginTrackInvocation();
        }

        public void EndTrackInvocation(string collectionName)
        {
            this.GetOrCreateCollectionByName(collectionName).EndTrackInvocation();
        }

        public void Track(string collectionName, string modName, Action action)
        {
            DateTime eventTime = DateTime.UtcNow;
            this.Stopwatch.Reset();
            this.Stopwatch.Start();

            try
            {
                action();
            }
            finally
            {
                this.Stopwatch.Stop();

                this.GetOrCreateCollectionByName(collectionName).Track(modName, new PerformanceCounterEntry
                {
                    EventTime = eventTime,
                    Elapsed = this.Stopwatch.Elapsed
                });
            }
        }

        public PerformanceCounterCollection GetCollectionByName(string name)
        {
            return this.PerformanceCounterCollections.FirstOrDefault(collection => collection.Name == name);
        }

        public PerformanceCounterCollection GetOrCreateCollectionByName(string name)
        {
            PerformanceCounterCollection collection = this.GetCollectionByName(name);

            if (collection == null)
            {
                collection = new PerformanceCounterCollection(this, name);
                this.PerformanceCounterCollections.Add(collection);
            }

            return collection;
        }

        public void ResetCategory(string name)
        {
            foreach (var performanceCounterCollection in this.PerformanceCounterCollections)
            {
                if (performanceCounterCollection.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    performanceCounterCollection.ResetCallsPerSecond();
                    performanceCounterCollection.Reset();
                }
            }
        }

        public void ResetSource(string name)
        {
            foreach (var performanceCounterCollection in this.PerformanceCounterCollections)
            {
                performanceCounterCollection.ResetSource(name);
            }
        }


        public void AddAlert(AlertEntry entry)
        {
            this.Alerts.Add(entry);
        }

        public void InitializePerformanceCounterEvents(EventManager eventManager)
        {
            this.PerformanceCounterCollections = new HashSet<PerformanceCounterCollection>()
            {
                new EventPerformanceCounterCollection(this, eventManager.MenuChanged, false),


                // Rendering Events
                new EventPerformanceCounterCollection(this, eventManager.Rendering, true),
                new EventPerformanceCounterCollection(this, eventManager.Rendered, true),
                new EventPerformanceCounterCollection(this, eventManager.RenderingWorld, true),
                new EventPerformanceCounterCollection(this, eventManager.RenderedWorld, true),
                new EventPerformanceCounterCollection(this, eventManager.RenderingActiveMenu, true),
                new EventPerformanceCounterCollection(this, eventManager.RenderedActiveMenu, true),
                new EventPerformanceCounterCollection(this, eventManager.RenderingHud, true),
                new EventPerformanceCounterCollection(this, eventManager.RenderedHud, true),

                new EventPerformanceCounterCollection(this, eventManager.WindowResized, false),
                new EventPerformanceCounterCollection(this, eventManager.GameLaunched, false),
                new EventPerformanceCounterCollection(this, eventManager.UpdateTicking, true),
                new EventPerformanceCounterCollection(this, eventManager.UpdateTicked, true),
                new EventPerformanceCounterCollection(this, eventManager.OneSecondUpdateTicking, true),
                new EventPerformanceCounterCollection(this, eventManager.OneSecondUpdateTicked, true),

                new EventPerformanceCounterCollection(this, eventManager.SaveCreating, false),
                new EventPerformanceCounterCollection(this, eventManager.SaveCreated, false),
                new EventPerformanceCounterCollection(this, eventManager.Saving, false),
                new EventPerformanceCounterCollection(this, eventManager.Saved, false),

                new EventPerformanceCounterCollection(this, eventManager.DayStarted, false),
                new EventPerformanceCounterCollection(this, eventManager.DayEnding, false),

                new EventPerformanceCounterCollection(this, eventManager.TimeChanged, true),

                new EventPerformanceCounterCollection(this, eventManager.ReturnedToTitle, false),

                new EventPerformanceCounterCollection(this, eventManager.ButtonPressed, true),
                new EventPerformanceCounterCollection(this, eventManager.ButtonReleased, true),
                new EventPerformanceCounterCollection(this, eventManager.CursorMoved, true),
                new EventPerformanceCounterCollection(this, eventManager.MouseWheelScrolled, true),

                new EventPerformanceCounterCollection(this, eventManager.PeerContextReceived, true),
                new EventPerformanceCounterCollection(this, eventManager.ModMessageReceived, true),
                new EventPerformanceCounterCollection(this, eventManager.PeerDisconnected, true),
                new EventPerformanceCounterCollection(this, eventManager.InventoryChanged, true),
                new EventPerformanceCounterCollection(this, eventManager.LevelChanged, true),
                new EventPerformanceCounterCollection(this, eventManager.Warped, true),

                new EventPerformanceCounterCollection(this, eventManager.LocationListChanged, true),
                new EventPerformanceCounterCollection(this, eventManager.BuildingListChanged, true),
                new EventPerformanceCounterCollection(this, eventManager.LocationListChanged, true),
                new EventPerformanceCounterCollection(this, eventManager.DebrisListChanged, true),
                new EventPerformanceCounterCollection(this, eventManager.LargeTerrainFeatureListChanged, true),
                new EventPerformanceCounterCollection(this, eventManager.NpcListChanged, true),
                new EventPerformanceCounterCollection(this, eventManager.ObjectListChanged, true),
                new EventPerformanceCounterCollection(this, eventManager.ChestInventoryChanged, true),
                new EventPerformanceCounterCollection(this, eventManager.TerrainFeatureListChanged, true),
                new EventPerformanceCounterCollection(this, eventManager.LoadStageChanged, false),
                new EventPerformanceCounterCollection(this, eventManager.UnvalidatedUpdateTicking, false),
                new EventPerformanceCounterCollection(this, eventManager.UnvalidatedUpdateTicked, false),
            };
        }
    }
}
