using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StardewModdingAPI.Framework.PerformanceCounter
{
    internal class PerformanceCounterCollection
    {
        /// <summary>The list of triggered performance counters.</summary>
        private readonly List<AlertContext> TriggeredPerformanceCounters = new List<AlertContext>();

        /// <summary>The stopwatch used to track the invocation time.</summary>
        private readonly Stopwatch InvocationStopwatch = new Stopwatch();

        /// <summary>The performance counter manager.</summary>
        private readonly PerformanceCounterManager PerformanceCounterManager;

        /// <summary>Holds the time to calculate the average calls per second.</summary>
        private DateTime CallsPerSecondStart = DateTime.UtcNow;

        /// <summary>The number of invocations of this collection.</summary>
        private long CallCount;

        public IDictionary<string, PerformanceCounter> PerformanceCounters { get; } = new Dictionary<string, PerformanceCounter>();

        /// <summary>The name of this collection.</summary>
        public string Name { get; }

        /// <summary>Flag if this collection is important (used for the console summary command).</summary>
        public bool IsImportant { get; }

        /// <summary>The alert threshold in milliseconds.</summary>
        public double AlertThresholdMilliseconds { get; set; }

        /// <summary>If alerting is enabled or not</summary>
        public bool EnableAlerts { get; set; }


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

        /// <summary>Tracks a single invocation for a named source.</summary>
        /// <param name="source">The name of the source.</param>
        /// <param name="entry">The entry.</param>
        public void Track(string source, PerformanceCounterEntry entry)
        {
            if (!this.PerformanceCounters.ContainsKey(source))
                this.PerformanceCounters.Add(source, new PerformanceCounter(this, source));

            this.PerformanceCounters[source].Add(entry);

            if (this.EnableAlerts)
                this.TriggeredPerformanceCounters.Add(new AlertContext(source, entry.ElapsedMilliseconds));
        }

        /// <summary>Returns the average execution time for all non-game internal sources.</summary>
        /// <returns>The average execution time in milliseconds</returns>
        public double GetModsAverageExecutionTime()
        {
            return this.PerformanceCounters.Where(p =>
                p.Key != Constants.GamePerformanceCounterName).Sum(p => p.Value.GetAverage());
        }

        /// <summary>Returns the overall average execution time.</summary>
        /// <returns>The average execution time in milliseconds</returns>
        public double GetAverageExecutionTime()
        {
            return this.PerformanceCounters.Sum(p => p.Value.GetAverage());
        }

        /// <summary>Returns the average execution time for game-internal sources.</summary>
        /// <returns>The average execution time in milliseconds</returns>
        public double GetGameAverageExecutionTime()
        {
            if (this.PerformanceCounters.TryGetValue(Constants.GamePerformanceCounterName, out PerformanceCounter gameExecTime))
                return gameExecTime.GetAverage();

            return 0;
        }

        /// <summary>Begins tracking the invocation of this collection.</summary>
        public void BeginTrackInvocation()
        {
            if (this.EnableAlerts)
            {
                this.TriggeredPerformanceCounters.Clear();
                this.InvocationStopwatch.Reset();
                this.InvocationStopwatch.Start();
            }

            this.CallCount++;
        }

        /// <summary>Ends tracking the invocation of this collection. Also records an alert if alerting is enabled
        /// and the invocation time exceeds the threshold.</summary>
        public void EndTrackInvocation()
        {
            if (!this.EnableAlerts) return;

            this.InvocationStopwatch.Stop();

            if (this.InvocationStopwatch.Elapsed.TotalMilliseconds >= this.AlertThresholdMilliseconds)
                this.AddAlert(this.InvocationStopwatch.Elapsed.TotalMilliseconds,
                    this.AlertThresholdMilliseconds, this.TriggeredPerformanceCounters);
        }

        /// <summary>Adds an alert.</summary>
        /// <param name="executionTimeMilliseconds">The execution time in milliseconds.</param>
        /// <param name="thresholdMilliseconds">The configured threshold.</param>
        /// <param name="alerts">The list of alert contexts.</param>
        public void AddAlert(double executionTimeMilliseconds, double thresholdMilliseconds, List<AlertContext> alerts)
        {
            this.PerformanceCounterManager.AddAlert(new AlertEntry(this, executionTimeMilliseconds,
                thresholdMilliseconds, alerts));
        }

        /// <summary>Adds an alert for a single AlertContext</summary>
        /// <param name="executionTimeMilliseconds">The execution time in milliseconds.</param>
        /// <param name="thresholdMilliseconds">The configured threshold.</param>
        /// <param name="alert">The context</param>
        public void AddAlert(double executionTimeMilliseconds, double thresholdMilliseconds, AlertContext alert)
        {
            this.AddAlert(executionTimeMilliseconds, thresholdMilliseconds, new List<AlertContext>() {alert});
        }

        /// <summary>Resets the calls per second counter.</summary>
        public void ResetCallsPerSecond()
        {
            this.CallCount = 0;
            this.CallsPerSecondStart = DateTime.UtcNow;
        }

        /// <summary>Resets all performance counters in this collection.</summary>
        public void Reset()
        {
            foreach (var i in this.PerformanceCounters)
                i.Value.Reset();
        }

        /// <summary>Resets the performance counter for a specific source.</summary>
        /// <param name="source">The source name</param>
        public void ResetSource(string source)
        {
            foreach (var i in this.PerformanceCounters)
                if (i.Value.Source.Equals(source, StringComparison.InvariantCultureIgnoreCase))
                    i.Value.Reset();
        }

        /// <summary>Returns the average calls per second.</summary>
        /// <returns>The average calls per second.</returns>
        public long GetAverageCallsPerSecond()
        {
            long runtimeInSeconds = (long) DateTime.UtcNow.Subtract(this.CallsPerSecondStart).TotalSeconds;

            if (runtimeInSeconds == 0) return 0;

            return this.CallCount / runtimeInSeconds;
        }
    }
}
