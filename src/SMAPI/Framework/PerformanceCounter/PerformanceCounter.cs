using System;
using System.Linq;
using Cyotek.Collections.Generic;

namespace StardewModdingAPI.Framework.PerformanceCounter
{
    internal class PerformanceCounter
    {
        /// <summary>The size of the ring buffer.</summary>
        private const int MAX_ENTRIES = 16384;

        /// <summary>The collection to which this performance counter belongs.</summary>
        private readonly PerformanceCounterCollection ParentCollection;

        /// <summary>The circular buffer which stores all performance counter entries</summary>
        private readonly CircularBuffer<PerformanceCounterEntry> _counter;

        /// <summary>The peak execution time</summary>
        private PerformanceCounterEntry? PeakPerformanceCounterEntry;

        /// <summary>The name of the source.</summary>
        public string Source { get; }

        /// <summary>The alert threshold in milliseconds</summary>
        public double AlertThresholdMilliseconds { get; set; }

        /// <summary>If alerting is enabled or not</summary>
        public bool EnableAlerts { get; set; }

        public PerformanceCounter(PerformanceCounterCollection parentCollection, string source)
        {
            this.ParentCollection = parentCollection;
            this.Source = source;
            this._counter = new CircularBuffer<PerformanceCounterEntry>(PerformanceCounter.MAX_ENTRIES);
        }

        /// <summary>Adds a new performance counter entry to the list. Updates the peak entry and adds an alert if
        /// monitoring is enabled and the execution time exceeds the threshold.</summary>
        /// <param name="entry">The entry to add.</param>
        public void Add(PerformanceCounterEntry entry)
        {
            this._counter.Put(entry);

            if (this.EnableAlerts && entry.ElapsedMilliseconds > this.AlertThresholdMilliseconds)
                this.ParentCollection.AddAlert(entry.ElapsedMilliseconds, this.AlertThresholdMilliseconds,
                    new AlertContext(this.Source, entry.ElapsedMilliseconds));

            if (this.PeakPerformanceCounterEntry == null)
                this.PeakPerformanceCounterEntry = entry;
            else
            {
                if (entry.ElapsedMilliseconds > this.PeakPerformanceCounterEntry.Value.ElapsedMilliseconds)
                    this.PeakPerformanceCounterEntry = entry;
            }
        }

        /// <summary>Clears all performance counter entries and resets the peak entry.</summary>
        public void Reset()
        {
            this._counter.Clear();
            this.PeakPerformanceCounterEntry = null;
        }

        /// <summary>Returns the peak entry.</summary>
        /// <returns>The peak entry.</returns>
        public PerformanceCounterEntry? GetPeak()
        {
            return this.PeakPerformanceCounterEntry;
        }

        /// <summary>Resets the peak entry.</summary>
        public void ResetPeak()
        {
            this.PeakPerformanceCounterEntry = null;
        }

        /// <summary>Returns the last entry added to the list.</summary>
        /// <returns>The last entry</returns>
        public PerformanceCounterEntry? GetLastEntry()
        {
            if (this._counter.IsEmpty)
                return null;

            return this._counter.PeekLast();
        }

        /// <summary>Returns the average execution time of all entries.</summary>
        /// <returns>The average execution time in milliseconds.</returns>
        public double GetAverage()
        {
            if (this._counter.IsEmpty)
                return 0;

            return this._counter.Average(p => p.ElapsedMilliseconds);
        }

        /// <summary>Returns the average over a given time span.</summary>
        /// <param name="range">The time range to retrieve.</param>
        /// <param name="relativeTo">The DateTime from which to start the average. Defaults to DateTime.UtcNow if null</param>
        /// <returns>The average execution time in milliseconds.</returns>
        /// <remarks>
        /// The relativeTo parameter specifies from which point in time the range is subtracted. Example:
        /// If DateTime is set to 60 seconds ago, and the range is set to 60 seconds, the method would return
        /// the average between all entries between 120s ago and 60s ago.
        /// </remarks>
        public double GetAverage(TimeSpan range, DateTime? relativeTo = null)
        {
            if (this._counter.IsEmpty)
                return 0;

            if (relativeTo == null)
                relativeTo = DateTime.UtcNow;

            DateTime start = relativeTo.Value.Subtract(range);

            var entries = this._counter.Where(x => (x.EventTime >= start) && (x.EventTime <= relativeTo));

            if (!entries.Any())
                return 0;

            return entries.Average(x => x.ElapsedMilliseconds);
        }
    }
}
