using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Framework.PerformanceCounter
{
    internal class PerformanceCounterManager
    {
        public HashSet<PerformanceCounterCollection> PerformanceCounterCollections = new HashSet<PerformanceCounterCollection>();

        /// <summary>The recorded alerts.</summary>
        private readonly List<AlertEntry> Alerts = new List<AlertEntry>();

        /// <summary>The monitor for output logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The invocation stopwatch.</summary>
        private readonly Stopwatch InvocationStopwatch = new Stopwatch();

        /// <summary>Specifies if alerts should be paused.</summary>
        public bool PauseAlerts { get; set; }

        /// <summary>Specifies if performance counter tracking should be enabled.</summary>
        public bool EnableTracking { get; set; }

        /// <summary>Constructs a performance counter manager.</summary>
        /// <param name="monitor">The monitor for output logging.</param>
        public PerformanceCounterManager(IMonitor monitor)
        {
            this.Monitor = monitor;
        }

        /// <summary>Resets all performance counters in all collections.</summary>
        public void Reset()
        {
            foreach (PerformanceCounterCollection collection in this.PerformanceCounterCollections)
            {
                collection.Reset();
            }

            foreach (var eventPerformanceCounter in
                this.PerformanceCounterCollections.SelectMany(performanceCounter => performanceCounter.PerformanceCounters))
            {
                eventPerformanceCounter.Value.Reset();
            }
        }

        /// <summary>Begins tracking the invocation for a collection.</summary>
        /// <param name="collectionName">The collection name</param>
        public void BeginTrackInvocation(string collectionName)
        {
            if (!this.EnableTracking)
            {
                return;
            }

            this.GetOrCreateCollectionByName(collectionName).BeginTrackInvocation();
        }

        /// <summary>Ends tracking the invocation for a collection.</summary>
        /// <param name="collectionName"></param>
        public void EndTrackInvocation(string collectionName)
        {
            if (!this.EnableTracking)
            {
                return;
            }

            this.GetOrCreateCollectionByName(collectionName).EndTrackInvocation();
        }

        /// <summary>Tracks a single performance counter invocation in a specific collection.</summary>
        /// <param name="collectionName">The name of the collection.</param>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="action">The action to execute and track invocation time for.</param>
        public void Track(string collectionName, string sourceName, Action action)
        {
            if (!this.EnableTracking)
            {
                action();
                return;
            }

            DateTime eventTime = DateTime.UtcNow;
            this.InvocationStopwatch.Reset();
            this.InvocationStopwatch.Start();

            try
            {
                action();
            }
            finally
            {
                this.InvocationStopwatch.Stop();

                this.GetOrCreateCollectionByName(collectionName).Track(sourceName, new PerformanceCounterEntry
                {
                    EventTime = eventTime,
                    ElapsedMilliseconds = this.InvocationStopwatch.Elapsed.TotalMilliseconds
                });
            }
        }

        /// <summary>Gets a collection by name.</summary>
        /// <param name="name">The name of the collection.</param>
        /// <returns>The collection or null if none was found.</returns>
        private PerformanceCounterCollection GetCollectionByName(string name)
        {
            return this.PerformanceCounterCollections.FirstOrDefault(collection => collection.Name == name);
        }

        /// <summary>Gets a collection by name and creates it if it doesn't exist.</summary>
        /// <param name="name">The name of the collection.</param>
        /// <returns>The collection.</returns>
        private PerformanceCounterCollection GetOrCreateCollectionByName(string name)
        {
            PerformanceCounterCollection collection = this.GetCollectionByName(name);

            if (collection != null) return collection;

            collection = new PerformanceCounterCollection(this, name);
            this.PerformanceCounterCollections.Add(collection);

            return collection;
        }

        /// <summary>Resets the performance counters for a specific collection.</summary>
        /// <param name="name">The collection name.</param>
        public void ResetCollection(string name)
        {
            foreach (PerformanceCounterCollection performanceCounterCollection in
                this.PerformanceCounterCollections.Where(performanceCounterCollection =>
                    performanceCounterCollection.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            {
                performanceCounterCollection.ResetCallsPerSecond();
                performanceCounterCollection.Reset();
            }
        }

        /// <summary>Resets performance counters for a specific source.</summary>
        /// <param name="name">The name of the source.</param>
        public void ResetSource(string name)
        {
            foreach (PerformanceCounterCollection performanceCounterCollection in this.PerformanceCounterCollections)
                performanceCounterCollection.ResetSource(name);
        }

        /// <summary>Print any queued alerts.</summary>
        public void PrintQueuedAlerts()
        {
            if (this.Alerts.Count == 0) return;

            StringBuilder sb = new StringBuilder();

            foreach (AlertEntry alert in this.Alerts)
            {
                sb.AppendLine($"{alert.Collection.Name} took {alert.ExecutionTimeMilliseconds:F2}ms (exceeded threshold of {alert.ThresholdMilliseconds:F2}ms)");

                foreach (AlertContext context in alert.Context.OrderByDescending(p => p.Elapsed))
                    sb.AppendLine(context.ToString());
            }

            this.Alerts.Clear();
            this.Monitor.Log(sb.ToString(), LogLevel.Error);
        }

        /// <summary>Adds an alert to the queue.</summary>
        /// <param name="entry">The alert to add.</param>
        public void AddAlert(AlertEntry entry)
        {
            if (!this.PauseAlerts)
                this.Alerts.Add(entry);
        }

        /// <summary>Initialized the default performance counter collections.</summary>
        /// <param name="eventManager">The event manager.</param>
        public void InitializePerformanceCounterCollections(EventManager eventManager)
        {
            this.PerformanceCounterCollections = new HashSet<PerformanceCounterCollection>()
            {
                new EventPerformanceCounterCollection(this, eventManager.MenuChanged, false),

                // Rendering Events
                new EventPerformanceCounterCollection(this, eventManager.Rendering, false),
                new EventPerformanceCounterCollection(this, eventManager.Rendered, true),
                new EventPerformanceCounterCollection(this, eventManager.RenderingWorld, false),
                new EventPerformanceCounterCollection(this, eventManager.RenderedWorld, true),
                new EventPerformanceCounterCollection(this, eventManager.RenderingActiveMenu, false),
                new EventPerformanceCounterCollection(this, eventManager.RenderedActiveMenu, true),
                new EventPerformanceCounterCollection(this, eventManager.RenderingHud, false),
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

                new EventPerformanceCounterCollection(this, eventManager.PeerContextReceived, false),
                new EventPerformanceCounterCollection(this, eventManager.ModMessageReceived, false),
                new EventPerformanceCounterCollection(this, eventManager.PeerDisconnected, false),
                new EventPerformanceCounterCollection(this, eventManager.InventoryChanged, true),
                new EventPerformanceCounterCollection(this, eventManager.LevelChanged, false),
                new EventPerformanceCounterCollection(this, eventManager.Warped, false),

                new EventPerformanceCounterCollection(this, eventManager.LocationListChanged, false),
                new EventPerformanceCounterCollection(this, eventManager.BuildingListChanged, false),
                new EventPerformanceCounterCollection(this, eventManager.LocationListChanged, false),
                new EventPerformanceCounterCollection(this, eventManager.DebrisListChanged, true),
                new EventPerformanceCounterCollection(this, eventManager.LargeTerrainFeatureListChanged, true),
                new EventPerformanceCounterCollection(this, eventManager.NpcListChanged, false),
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
