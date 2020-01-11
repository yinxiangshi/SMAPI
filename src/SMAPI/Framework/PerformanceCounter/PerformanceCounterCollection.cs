using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using StardewModdingAPI.Framework.Utilities;

namespace StardewModdingAPI.Framework.PerformanceCounter
{
    internal class PerformanceCounterCollection
    {
        public IDictionary<string, PerformanceCounter> PerformanceCounters { get; } = new Dictionary<string, PerformanceCounter>();
        private DateTime StartDateTime = DateTime.Now;
        private long CallCount;
        public string Name { get; private set; }
        public bool IsImportant { get; set; }
        private readonly Stopwatch Stopwatch = new Stopwatch();
        private readonly PerformanceCounterManager PerformanceCounterManager;
        public double MonitorThresholdMilliseconds { get; set; }
        public bool Monitor { get; set; }
        private readonly List<AlertContext> TriggeredPerformanceCounters = new List<AlertContext>();

        public PerformanceCounterCollection(PerformanceCounterManager performanceCounterManager, string name, bool isImportant)
        {
            this.Name = name;
            this.PerformanceCounterManager = performanceCounterManager;
            this.IsImportant = isImportant;
        }

        public PerformanceCounterCollection(PerformanceCounterManager performanceCounterManager, string name)
        {
            this.PerformanceCounterManager = performanceCounterManager;
            this.Name = name;
        }

        public void Track(string source, PerformanceCounterEntry entry)
        {
            if (!this.PerformanceCounters.ContainsKey(source))
            {
                this.PerformanceCounters.Add(source, new PerformanceCounter(this, source));
            }
            this.PerformanceCounters[source].Add(entry);

            if (this.Monitor)
            {
                this.TriggeredPerformanceCounters.Add(new AlertContext(source, entry.Elapsed.TotalMilliseconds));
            }
        }

        public double GetModsAverageExecutionTime()
        {
            return this.PerformanceCounters.Where(p => p.Key != Constants.GamePerformanceCounterName).Sum(p => p.Value.GetAverage());
        }

        public double GetAverageExecutionTime()
        {
            return this.PerformanceCounters.Sum(p => p.Value.GetAverage());
        }

        public double GetGameAverageExecutionTime()
        {
            if (this.PerformanceCounters.TryGetValue(Constants.GamePerformanceCounterName, out PerformanceCounter gameExecTime))
            {
                return gameExecTime.GetAverage();
            }

            return 0;
        }

        public void BeginTrackInvocation()
        {
            if (this.Monitor)
            {
                this.TriggeredPerformanceCounters.Clear();
                this.Stopwatch.Reset();
                this.Stopwatch.Start();
            }

            this.CallCount++;

        }

        public void EndTrackInvocation()
        {
            if (!this.Monitor) return;

            this.Stopwatch.Stop();
            if (this.Stopwatch.Elapsed.TotalMilliseconds >= this.MonitorThresholdMilliseconds)
            {
                this.AddAlert(this.Stopwatch.Elapsed.TotalMilliseconds,
                    this.MonitorThresholdMilliseconds, this.TriggeredPerformanceCounters);
            }
        }

        public void AddAlert(double executionTimeMilliseconds, double threshold, List<AlertContext> alerts)
        {
            this.PerformanceCounterManager.AddAlert(new AlertEntry(this, executionTimeMilliseconds,
                threshold, alerts));
        }

        public void AddAlert(double executionTimeMilliseconds, double threshold, AlertContext alert)
        {
            this.AddAlert(executionTimeMilliseconds, threshold, new List<AlertContext>() {alert});
        }

        public void ResetCallsPerSecond()
        {
            this.CallCount = 0;
            this.StartDateTime = DateTime.Now;
        }

        public void Reset()
        {
            foreach (var i in this.PerformanceCounters)
            {
                i.Value.Reset();
                i.Value.ResetPeak();
            }
        }

        public void ResetSource(string source)
        {
            foreach (var i in this.PerformanceCounters)
            {
                if (i.Value.Source.Equals(source, StringComparison.InvariantCultureIgnoreCase))
                {
                    i.Value.Reset();
                    i.Value.ResetPeak();
                }
            }
        }

        public long GetAverageCallsPerSecond()
        {
            long runtimeInSeconds = (long) DateTime.Now.Subtract(this.StartDateTime).TotalSeconds;

            if (runtimeInSeconds == 0)
            {
                return 0;
            }

            return this.CallCount / runtimeInSeconds;
        }
    }
}
