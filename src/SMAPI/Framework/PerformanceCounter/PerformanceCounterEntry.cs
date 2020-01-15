using System;

namespace StardewModdingAPI.Framework.PerformanceCounter
{
    /// <summary>A single performance counter entry. Records the DateTime of the event and the elapsed millisecond.</summary>
    internal struct PerformanceCounterEntry
    {
        /// <summary>The DateTime when the entry occured.</summary>
        public DateTime EventTime;

        /// <summary>The elapsed milliseconds</summary>
        public double ElapsedMilliseconds;
    }
}
