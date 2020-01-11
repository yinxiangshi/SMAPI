using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Framework.PerformanceCounter
{
    internal class EventPerformanceCounterCollection: PerformanceCounterCollection
    {
        public EventPerformanceCounterCollection(PerformanceCounterManager manager, IManagedEvent @event, bool isImportant) : base(manager, @event.GetName(), isImportant)
        {
        }
    }
}
