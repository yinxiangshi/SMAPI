using System;
using System.Collections.Generic;

namespace StardewModdingAPI.Framework.Utilities
{
    public interface IPerformanceCounterEvent
    {
        string GetEventName();
        long GetAverageCallsPerSecond();
        IDictionary<string, PerformanceCounter.PerformanceCounter> PerformanceCounters { get; }

        double GetGameAverageExecutionTime();
        double GetModsAverageExecutionTime();
        double GetAverageExecutionTime();
    }
}
