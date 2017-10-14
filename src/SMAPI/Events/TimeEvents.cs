using System;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the in-game date or time changes.</summary>
    public static class TimeEvents
    {
        /*********
        ** Events
        *********/
        /// <summary>Raised after the game begins a new day, including when loading a save.</summary>
        public static event EventHandler AfterDayStarted;

        /// <summary>Raised after the in-game clock changes.</summary>
        public static event EventHandler<EventArgsIntChanged> TimeOfDayChanged;

        /*********
        ** Internal methods
        *********/
        /// <summary>Raise an <see cref="AfterDayStarted"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeAfterDayStarted(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(TimeEvents)}.{nameof(TimeEvents.AfterDayStarted)}", TimeEvents.AfterDayStarted?.GetInvocationList(), null, EventArgs.Empty);
        }

        /// <summary>Raise a <see cref="TimeOfDayChanged"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="priorTime">The previous time in military time format (e.g. 6:00pm is 1800).</param>
        /// <param name="newTime">The current time in military time format (e.g. 6:10pm is 1810).</param>
        internal static void InvokeTimeOfDayChanged(IMonitor monitor, int priorTime, int newTime)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(TimeEvents)}.{nameof(TimeEvents.TimeOfDayChanged)}", TimeEvents.TimeOfDayChanged?.GetInvocationList(), null, new EventArgsIntChanged(priorTime, newTime));
        }
    }
}
