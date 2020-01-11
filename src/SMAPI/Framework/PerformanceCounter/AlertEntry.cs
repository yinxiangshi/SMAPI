using System.Collections.Generic;

namespace StardewModdingAPI.Framework.PerformanceCounter
{
    internal struct AlertEntry
    {
        public PerformanceCounterCollection Collection;
        public double ExecutionTimeMilliseconds;
        public double Threshold;
        public List<AlertContext> Context;

        public AlertEntry(PerformanceCounterCollection collection, double executionTimeMilliseconds, double threshold, List<AlertContext> context)
        {
            this.Collection = collection;
            this.ExecutionTimeMilliseconds = executionTimeMilliseconds;
            this.Threshold = threshold;
            this.Context = context;
        }
    }
}
