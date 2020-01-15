using System.Collections.Generic;

namespace StardewModdingAPI.Framework.PerformanceCounter
{
    /// <summary>A single alert entry.</summary>
    internal struct AlertEntry
    {
        /// <summary>The collection in which the alert occurred.</summary>
        public readonly PerformanceCounterCollection Collection;

        /// <summary>The actual execution time in milliseconds.</summary>
        public readonly double ExecutionTimeMilliseconds;

        /// <summary>The configured alert threshold. </summary>
        public readonly double ThresholdMilliseconds;

        /// <summary>The context list, which records all sources involved in exceeding the threshold.</summary>
        public readonly List<AlertContext> Context;

        /// <summary>Creates a new alert entry.</summary>
        /// <param name="collection">The source collection in which the alert occurred.</param>
        /// <param name="executionTimeMilliseconds">The actual execution time in milliseconds.</param>
        /// <param name="thresholdMilliseconds">The configured threshold in milliseconds.</param>
        /// <param name="context">A list of AlertContext to record which sources were involved</param>
        public AlertEntry(PerformanceCounterCollection collection, double executionTimeMilliseconds, double thresholdMilliseconds, List<AlertContext> context)
        {
            this.Collection = collection;
            this.ExecutionTimeMilliseconds = executionTimeMilliseconds;
            this.ThresholdMilliseconds = thresholdMilliseconds;
            this.Context = context;
        }
    }
}
