using System;
using System.Collections.Generic;

namespace StardewModdingAPI.Framework.PerformanceCounter
{
    internal struct PeakEntry
    {
        /// <summary>The actual execution time in milliseconds.</summary>
        public readonly double ExecutionTimeMilliseconds;

        /// <summary>The DateTime when the entry occured.</summary>
        public DateTime EventTime;

        /// <summary>The context list, which records all sources involved in exceeding the threshold.</summary>
        public readonly List<AlertContext> Context;

        public PeakEntry(double executionTimeMilliseconds, DateTime eventTime, List<AlertContext> context)
        {
            this.ExecutionTimeMilliseconds = executionTimeMilliseconds;
            this.EventTime = eventTime;
            this.Context = context;
        }
    }
}
