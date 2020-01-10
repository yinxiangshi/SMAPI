using System;
using System.Diagnostics;
using System.Linq;
using Cyotek.Collections.Generic;
using StardewModdingAPI.Framework.Utilities;

namespace StardewModdingAPI.Framework.PerformanceCounter
{
    public class PerformanceCounter
    {
        private const int MAX_ENTRIES = 16384;

        public string Name { get; }
        public static Stopwatch Stopwatch = new Stopwatch();
        public static long TotalNumEventsLogged;


        private readonly CircularBuffer<PerformanceCounterEntry> _counter;

        private PerformanceCounterEntry? PeakPerformanceCounterEntry;

        public PerformanceCounter(string name)
        {
            this.Name = name;
            this._counter = new CircularBuffer<PerformanceCounterEntry>(PerformanceCounter.MAX_ENTRIES);
        }

        public int GetAverageCallsPerSecond()
        {
            var x = this._counter.GroupBy(
                p =>
                    (int) p.EventTime.Subtract(
                        new DateTime(1970, 1, 1)
                    ).TotalSeconds);

            return x.Last().Count();
        }

        public void Add(PerformanceCounterEntry entry)
        {
            PerformanceCounter.Stopwatch.Start();
            this._counter.Put(entry);

            if (this.PeakPerformanceCounterEntry == null)
            {
                this.PeakPerformanceCounterEntry = entry;
            }
            else
            {
                if (entry.Elapsed.TotalMilliseconds > this.PeakPerformanceCounterEntry.Value.Elapsed.TotalMilliseconds)
                {
                    this.PeakPerformanceCounterEntry = entry;
                }
            }

            PerformanceCounter.Stopwatch.Stop();
            PerformanceCounter.TotalNumEventsLogged++;
        }

        public PerformanceCounterEntry? GetPeak()
        {
            return this.PeakPerformanceCounterEntry;
        }

        public void ResetPeak()
        {
            this.PeakPerformanceCounterEntry = null;
        }

        public PerformanceCounterEntry? GetLastEntry()
        {
            if (this._counter.IsEmpty)
            {
                return null;
            }
            return this._counter.PeekLast();
        }

        public double GetAverage()
        {
            if (this._counter.IsEmpty)
            {
                return 0;
            }

            return this._counter.Average(p => p.Elapsed.TotalMilliseconds);
        }

        public double GetAverage(TimeSpan range)
        {
            if (this._counter.IsEmpty)
            {
                return 0;
            }

            var lastTime = this._counter.Max(x => x.EventTime);
            var start = lastTime.Subtract(range);

            var entries = this._counter.Where(x => (x.EventTime >= start) && (x.EventTime <= lastTime));
            return entries.Average(x => x.Elapsed.TotalMilliseconds);
        }
    }
}
