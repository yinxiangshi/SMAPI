using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework.PerformanceMonitoring;

namespace StardewModdingAPI.Framework.Events
{
    /// <summary>An event wrapper which intercepts and logs errors in handler code.</summary>
    /// <typeparam name="TEventArgs">The event arguments type.</typeparam>
    internal class ManagedEvent<TEventArgs> : IManagedEvent
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying event handlers.</summary>
        private readonly List<ManagedEventHandler<TEventArgs>> EventHandlers = new List<ManagedEventHandler<TEventArgs>>();

        /// <summary>The mod registry with which to identify mods.</summary>
        protected readonly ModRegistry ModRegistry;

        /// <summary>Tracks performance metrics.</summary>
        private readonly PerformanceMonitor PerformanceMonitor;

        /// <summary>The total number of event handlers registered for this events, regardless of whether they're still registered.</summary>
        private int RegistrationIndex;

        /// <summary>Whether any registered event handlers have a custom priority value.</summary>
        private bool HasCustomPriorities;

        /// <summary>Whether event handlers should be sorted before the next invocation.</summary>
        private bool NeedsSort;


        /*********
        ** Accessors
        *********/
        /// <summary>A human-readable name for the event.</summary>
        public string EventName { get; }

        /// <summary>Whether the event is typically called at least once per second.</summary>
        public bool IsPerformanceCritical { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="eventName">A human-readable name for the event.</param>
        /// <param name="modRegistry">The mod registry with which to identify mods.</param>
        /// <param name="performanceMonitor">Tracks performance metrics.</param>
        /// <param name="isPerformanceCritical">Whether the event is typically called at least once per second.</param>
        public ManagedEvent(string eventName, ModRegistry modRegistry, PerformanceMonitor performanceMonitor, bool isPerformanceCritical = false)
        {
            this.EventName = eventName;
            this.ModRegistry = modRegistry;
            this.PerformanceMonitor = performanceMonitor;
            this.IsPerformanceCritical = isPerformanceCritical;
        }

        /// <summary>Get whether anything is listening to the event.</summary>
        public bool HasListeners()
        {
            return this.EventHandlers.Count > 0;
        }

        /// <summary>Add an event handler.</summary>
        /// <param name="handler">The event handler.</param>
        /// <param name="mod">The mod which added the event handler.</param>
        public void Add(EventHandler<TEventArgs> handler, IModMetadata mod)
        {
            EventPriority priority = handler.Method.GetCustomAttribute<EventPriorityAttribute>()?.Priority ?? EventPriority.Normal;
            var managedHandler = new ManagedEventHandler<TEventArgs>(handler, this.RegistrationIndex++, priority, mod);

            this.EventHandlers.Add(managedHandler);
            this.HasCustomPriorities = this.HasCustomPriorities || managedHandler.HasCustomPriority();

            if (this.HasCustomPriorities)
                this.NeedsSort = true;
        }

        /// <summary>Remove an event handler.</summary>
        /// <param name="handler">The event handler.</param>
        public void Remove(EventHandler<TEventArgs> handler)
        {
            this.EventHandlers.RemoveAll(p => p.Handler == handler);
            this.HasCustomPriorities = this.HasCustomPriorities && this.EventHandlers.Any(p => p.HasCustomPriority());
        }

        /// <summary>Raise the event and notify all handlers.</summary>
        /// <param name="args">The event arguments to pass.</param>
        public void Raise(TEventArgs args)
        {
            // sort event handlers by priority
            // (This is done here to avoid repeatedly sorting when handlers are added/removed.)
            if (this.NeedsSort)
            {
                this.NeedsSort = false;
                this.EventHandlers.Sort();
            }

            // raise
            if (this.EventHandlers.Count == 0)
                return;
            this.PerformanceMonitor.Track(this.EventName, () =>
            {
                foreach (ManagedEventHandler<TEventArgs> handler in this.EventHandlers)
                {
                    try
                    {
                        this.PerformanceMonitor.Track(this.EventName, this.GetModNameForPerformanceCounters(handler), () => handler.Handler.Invoke(null, args));
                    }
                    catch (Exception ex)
                    {
                        this.LogError(handler, ex);
                    }
                }
            });
        }

        /// <summary>Raise the event and notify all handlers.</summary>
        /// <param name="args">The event arguments to pass.</param>
        /// <param name="match">A lambda which returns true if the event should be raised for the given mod.</param>
        public void RaiseForMods(TEventArgs args, Func<IModMetadata, bool> match)
        {
            if (this.EventHandlers.Count == 0)
                return;

            foreach (ManagedEventHandler<TEventArgs> handler in this.EventHandlers)
            {
                if (match(handler.SourceMod))
                {
                    try
                    {
                        handler.Handler.Invoke(null, args);
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
        /// <summary>Get the mod name for a given event handler to display in performance monitoring reports.</summary>
        /// <param name="handler">The event handler.</param>
        private string GetModNameForPerformanceCounters(ManagedEventHandler<TEventArgs> handler)
        {
            IModMetadata mod = handler.SourceMod;

            return mod.HasManifest()
                ? mod.Manifest.UniqueID
                : mod.DisplayName;
        }

        /// <summary>Log an exception from an event handler.</summary>
        /// <param name="handler">The event handler instance.</param>
        /// <param name="ex">The exception that was raised.</param>
        protected void LogError(ManagedEventHandler<TEventArgs> handler, Exception ex)
        {
            handler.SourceMod.LogAsMod($"This mod failed in the {this.EventName} event. Technical details: \n{ex.GetLogSummary()}", LogLevel.Error);
        }
    }
}
