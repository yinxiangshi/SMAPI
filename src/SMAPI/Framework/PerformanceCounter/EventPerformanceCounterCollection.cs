using StardewModdingAPI.Framework.Events;

namespace StardewModdingAPI.Framework.PerformanceCounter
{
    /// <summary>Represents a performance counter collection specific to game events.</summary>
    internal class EventPerformanceCounterCollection: PerformanceCounterCollection
    {
        /// <summary>Creates a new event performance counter collection.</summary>
        /// <param name="manager">The performance counter manager.</param>
        /// <param name="event">The ManagedEvent.</param>
        /// <param name="isImportant">If the event is flagged as important.</param>
        public EventPerformanceCounterCollection(PerformanceCounterManager manager, IManagedEvent @event, bool isImportant) : base(manager, @event.GetName(), isImportant)
        {
        }
    }
}
