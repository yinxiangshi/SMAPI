using System;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the in-game date or time changes.</summary>
    public static class TimeEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>Manages deprecation warnings.</summary>
        private static DeprecationManager DeprecationManager;


        /*********
        ** Events
        *********/
        /// <summary>Raised after the game begins a new day, including when loading a save.</summary>
        public static event EventHandler AfterDayStarted;

        /// <summary>Raised after the in-game clock changes.</summary>
        public static event EventHandler<EventArgsIntChanged> TimeOfDayChanged;

        /// <summary>Raised after the day-of-month value changes, including when loading a save. This may happen before save; in most cases you should use <see cref="AfterDayStarted"/> instead.</summary>
        public static event EventHandler<EventArgsIntChanged> DayOfMonthChanged;

        /// <summary>Raised after the year value changes.</summary>
        public static event EventHandler<EventArgsIntChanged> YearOfGameChanged;

        /// <summary>Raised after the season value changes.</summary>
        public static event EventHandler<EventArgsStringChanged> SeasonOfYearChanged;

        /// <summary>Raised when the player is transitioning to a new day and the game is performing its day update logic. This event is triggered twice: once after the game starts transitioning, and again after it finishes.</summary>
        [Obsolete("Use " + nameof(TimeEvents) + "." + nameof(TimeEvents.AfterDayStarted) + " or " + nameof(SaveEvents) + " instead")]
        public static event EventHandler<EventArgsNewDay> OnNewDay;


        /*********
        ** Internal methods
        *********/
        /// <summary>Injects types required for backwards compatibility.</summary>
        /// <param name="deprecationManager">Manages deprecation warnings.</param>
        internal static void Shim(DeprecationManager deprecationManager)
        {
            TimeEvents.DeprecationManager = deprecationManager;
        }

        /// <summary>Raise an <see cref="AfterDayStarted"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        internal static void InvokeAfterDayStarted(IMonitor monitor)
        {
            monitor.SafelyRaisePlainEvent($"{nameof(TimeEvents)}.{nameof(TimeEvents.AfterDayStarted)}", TimeEvents.AfterDayStarted?.GetInvocationList(), null, EventArgs.Empty);
        }

        /// <summary>Raise a <see cref="InvokeDayOfMonthChanged"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="priorTime">The previous time in military time format (e.g. 6:00pm is 1800).</param>
        /// <param name="newTime">The current time in military time format (e.g. 6:10pm is 1810).</param>
        internal static void InvokeTimeOfDayChanged(IMonitor monitor, int priorTime, int newTime)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(TimeEvents)}.{nameof(TimeEvents.TimeOfDayChanged)}", TimeEvents.TimeOfDayChanged?.GetInvocationList(), null, new EventArgsIntChanged(priorTime, newTime));
        }

        /// <summary>Raise a <see cref="DayOfMonthChanged"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="priorDay">The previous day value.</param>
        /// <param name="newDay">The current day value.</param>
        internal static void InvokeDayOfMonthChanged(IMonitor monitor, int priorDay, int newDay)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(TimeEvents)}.{nameof(TimeEvents.DayOfMonthChanged)}", TimeEvents.DayOfMonthChanged?.GetInvocationList(), null, new EventArgsIntChanged(priorDay, newDay));
        }

        /// <summary>Raise a <see cref="YearOfGameChanged"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="priorYear">The previous year value.</param>
        /// <param name="newYear">The current year value.</param>
        internal static void InvokeYearOfGameChanged(IMonitor monitor, int priorYear, int newYear)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(TimeEvents)}.{nameof(TimeEvents.YearOfGameChanged)}", TimeEvents.YearOfGameChanged?.GetInvocationList(), null, new EventArgsIntChanged(priorYear, newYear));
        }

        /// <summary>Raise a <see cref="SeasonOfYearChanged"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="priorSeason">The previous season name.</param>
        /// <param name="newSeason">The current season name.</param>
        internal static void InvokeSeasonOfYearChanged(IMonitor monitor, string priorSeason, string newSeason)
        {
            monitor.SafelyRaiseGenericEvent($"{nameof(TimeEvents)}.{nameof(TimeEvents.SeasonOfYearChanged)}", TimeEvents.SeasonOfYearChanged?.GetInvocationList(), null, new EventArgsStringChanged(priorSeason, newSeason));
        }

        /// <summary>Raise a <see cref="OnNewDay"/> event.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="priorDay">The previous day value.</param>
        /// <param name="newDay">The current day value.</param>
        /// <param name="isTransitioning">Whether the game just started the transition (<c>true</c>) or finished it (<c>false</c>).</param>
        internal static void InvokeOnNewDay(IMonitor monitor, int priorDay, int newDay, bool isTransitioning)
        {
            if (TimeEvents.OnNewDay == null)
                return;

            string name = $"{nameof(TimeEvents)}.{nameof(TimeEvents.OnNewDay)}";
            Delegate[] handlers = TimeEvents.OnNewDay.GetInvocationList();

            TimeEvents.DeprecationManager.WarnForEvent(handlers, name, "1.6", DeprecationLevel.Notice);
            monitor.SafelyRaiseGenericEvent(name, handlers, null, new EventArgsNewDay(priorDay, newDay, isTransitioning));
        }
    }
}
