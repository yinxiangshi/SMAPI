using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using StardewModdingAPI.Framework.Utilities;
using PerformanceCounter = StardewModdingAPI.Framework.PerformanceCounter.PerformanceCounter;

namespace StardewModdingAPI.Framework.Events
{
    /// <summary>An event wrapper which intercepts and logs errors in handler code.</summary>
    /// <typeparam name="TEventArgs">The event arguments type.</typeparam>
    internal class ManagedEvent<TEventArgs>: IPerformanceCounterEvent
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying event.</summary>
        private event EventHandler<TEventArgs> Event;

        /// <summary>A human-readable name for the event.</summary>
        private readonly string EventName;

        /// <summary>Writes messages to the log.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The mod registry with which to identify mods.</summary>
        protected readonly ModRegistry ModRegistry;

        /// <summary>The display names for the mods which added each delegate.</summary>
        private readonly IDictionary<EventHandler<TEventArgs>, IModMetadata> SourceMods = new Dictionary<EventHandler<TEventArgs>, IModMetadata>();

        /// <summary>The cached invocation list.</summary>
        private EventHandler<TEventArgs>[] CachedInvocationList;

        public IDictionary<string, PerformanceCounter.PerformanceCounter> PerformanceCounters { get; } = new Dictionary<string, PerformanceCounter.PerformanceCounter>();

        private readonly Stopwatch Stopwatch = new Stopwatch();

        private long EventCallCount = 0;

        private readonly DateTime StartDateTime = DateTime.Now;

        public string GetEventName()
        {
            return this.EventName;
        }

        public double GetGameAverageExecutionTime()
        {
            if (this.PerformanceCounters.TryGetValue(Constants.GamePerformanceCounterName, out PerformanceCounter.PerformanceCounter gameExecTime))
            {
                return gameExecTime.GetAverage();
            }

            return 0;
        }

        public double GetModsAverageExecutionTime()
        {
            return this.PerformanceCounters.Where(p => p.Key != Constants.GamePerformanceCounterName).Sum(p => p.Value.GetAverage());
        }

        public double GetAverageExecutionTime()
        {
            return this.PerformanceCounters.Sum(p => p.Value.GetAverage());
        }

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="eventName">A human-readable name for the event.</param>
        /// <param name="monitor">Writes messages to the log.</param>
        /// <param name="modRegistry">The mod registry with which to identify mods.</param>
        public ManagedEvent(string eventName, IMonitor monitor, ModRegistry modRegistry)
        {
            this.EventName = eventName;
            this.Monitor = monitor;
            this.ModRegistry = modRegistry;
        }

        /// <summary>Get whether anything is listening to the event.</summary>
        public bool HasListeners()
        {
            return this.CachedInvocationList?.Length > 0;
        }

        /// <summary>Add an event handler.</summary>
        /// <param name="handler">The event handler.</param>
        public void Add(EventHandler<TEventArgs> handler)
        {
            this.Add(handler, this.ModRegistry.GetFromStack());
        }

        /// <summary>Add an event handler.</summary>
        /// <param name="handler">The event handler.</param>
        /// <param name="mod">The mod which added the event handler.</param>
        public void Add(EventHandler<TEventArgs> handler, IModMetadata mod)
        {
            this.Event += handler;
            this.AddTracking(mod, handler, this.Event?.GetInvocationList().Cast<EventHandler<TEventArgs>>());


        }

        /// <summary>Remove an event handler.</summary>
        /// <param name="handler">The event handler.</param>
        public void Remove(EventHandler<TEventArgs> handler)
        {
            this.Event -= handler;
            this.RemoveTracking(handler, this.Event?.GetInvocationList().Cast<EventHandler<TEventArgs>>());
        }

        public long GetAverageCallsPerSecond()
        {
            long runtimeInSeconds = (long)DateTime.Now.Subtract(this.StartDateTime).TotalSeconds;

            if (runtimeInSeconds == 0)
            {
                return 0;
            }

            return this.EventCallCount / runtimeInSeconds;
        }

        /// <summary>Raise the event and notify all handlers.</summary>
        /// <param name="args">The event arguments to pass.</param>
        public void Raise(TEventArgs args)
        {
            if (this.Event == null)
                return;

            this.EventCallCount++;

            foreach (EventHandler<TEventArgs> handler in this.CachedInvocationList)
            {
                try
                {
                    var performanceCounterEntry = new PerformanceCounterEntry()
                    {
                        EventTime = DateTime.Now
                    };

                    this.Stopwatch.Reset();
                    this.Stopwatch.Start();
                    handler.Invoke(null, args);
                    this.Stopwatch.Stop();
                    performanceCounterEntry.Elapsed = this.Stopwatch.Elapsed;

                    string modName = this.GetSourceMod(handler)?.DisplayName ?? Constants.GamePerformanceCounterName;

                    if (!this.PerformanceCounters.ContainsKey(modName))
                    {
                        this.PerformanceCounters.Add(modName, new PerformanceCounter.PerformanceCounter($"{modName}.{this.EventName}"));
                    }
                    this.PerformanceCounters[modName].Add(performanceCounterEntry);

                }
                catch (Exception ex)
                {
                    this.LogError(handler, ex);
                }
            }
        }

        /// <summary>Raise the event and notify all handlers.</summary>
        /// <param name="args">The event arguments to pass.</param>
        /// <param name="match">A lambda which returns true if the event should be raised for the given mod.</param>
        public void RaiseForMods(TEventArgs args, Func<IModMetadata, bool> match)
        {
            if (this.Event == null)
                return;

            foreach (EventHandler<TEventArgs> handler in this.CachedInvocationList)
            {
                if (match(this.GetSourceMod(handler)))
                {
                    try
                    {
                        handler.Invoke(null, args);
                    }
                    catch (Exception ex)
                    {
                        this.LogError(handler, ex);
                    }
                }
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Track an event handler.</summary>
        /// <param name="mod">The mod which added the handler.</param>
        /// <param name="handler">The event handler.</param>
        /// <param name="invocationList">The updated event invocation list.</param>
        protected void AddTracking(IModMetadata mod, EventHandler<TEventArgs> handler, IEnumerable<EventHandler<TEventArgs>> invocationList)
        {
            this.SourceMods[handler] = mod;
            this.CachedInvocationList = invocationList?.ToArray() ?? new EventHandler<TEventArgs>[0];
        }

        /// <summary>Remove tracking for an event handler.</summary>
        /// <param name="handler">The event handler.</param>
        /// <param name="invocationList">The updated event invocation list.</param>
        protected void RemoveTracking(EventHandler<TEventArgs> handler, IEnumerable<EventHandler<TEventArgs>> invocationList)
        {
            this.CachedInvocationList = invocationList?.ToArray() ?? new EventHandler<TEventArgs>[0];
            if (!this.CachedInvocationList.Contains(handler)) // don't remove if there's still a reference to the removed handler (e.g. it was added twice and removed once)
                this.SourceMods.Remove(handler);
        }

        /// <summary>Get the mod which registered the given event handler, if available.</summary>
        /// <param name="handler">The event handler.</param>
        protected IModMetadata GetSourceMod(EventHandler<TEventArgs> handler)
        {
            return this.SourceMods.TryGetValue(handler, out IModMetadata mod)
                ? mod
                : null;
        }

        /// <summary>Log an exception from an event handler.</summary>
        /// <param name="handler">The event handler instance.</param>
        /// <param name="ex">The exception that was raised.</param>
        protected void LogError(EventHandler<TEventArgs> handler, Exception ex)
        {
            IModMetadata mod = this.GetSourceMod(handler);
            if (mod != null)
                mod.LogAsMod($"This mod failed in the {this.EventName} event. Technical details: \n{ex.GetLogSummary()}", LogLevel.Error);
            else
                this.Monitor.Log($"A mod failed in the {this.EventName} event. Technical details: \n{ex.GetLogSummary()}", LogLevel.Error);
        }
    }
}
