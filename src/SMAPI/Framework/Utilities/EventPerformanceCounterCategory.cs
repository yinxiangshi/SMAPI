namespace StardewModdingAPI.Framework.Utilities
{
    public class EventPerformanceCounterCategory
    {
        public IPerformanceCounterEvent Event { get; }
        public double MonitorThreshold { get; }
        public bool IsImportant { get; }
        public bool Monitor { get; }

        public EventPerformanceCounterCategory(IPerformanceCounterEvent @event, bool isImportant)
        {
            this.Event = @event;
            this.IsImportant = isImportant;
        }
    }
}
