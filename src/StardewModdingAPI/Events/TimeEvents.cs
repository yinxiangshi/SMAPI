using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when the in-game date or time changes.</summary>
    public static class TimeEvents
    {
        /*********
        ** Events
        *********/
        /// <summary>Raised after the in-game clock changes.</summary>
        public static event EventHandler<EventArgsIntChanged> TimeOfDayChanged = delegate { };

        /// <summary>Raised after the day-of-month value changes, including when loading a save (unlike <see cref="OnNewDay"/>).</summary>
        public static event EventHandler<EventArgsIntChanged> DayOfMonthChanged = delegate { };

        /// <summary>Raised after the year value changes.</summary>
        public static event EventHandler<EventArgsIntChanged> YearOfGameChanged = delegate { };

        /// <summary>Raised after the season value changes.</summary>
        public static event EventHandler<EventArgsStringChanged> SeasonOfYearChanged = delegate { };

        /// <summary>Raised when the player is transitioning to a new day and the game is performing its day update logic. This event is triggered twice: once after the game starts transitioning, and again after it finishes.</summary>
        public static event EventHandler<EventArgsNewDay> OnNewDay = delegate { };


        /*********
        ** Internal methods
        *********/
        /// <summary>Raise a <see cref="InvokeDayOfMonthChanged"/> event.</summary>
        /// <param name="priorTime">The previous time in military time format (e.g. 6:00pm is 1800).</param>
        /// <param name="newTime">The current time in military time format (e.g. 6:10pm is 1810).</param>
        internal static void InvokeTimeOfDayChanged(int priorTime, int newTime)
        {
            TimeEvents.TimeOfDayChanged.Invoke(null, new EventArgsIntChanged(priorTime, newTime));
        }

        /// <summary>Raise a <see cref="DayOfMonthChanged"/> event.</summary>
        /// <param name="priorDay">The previous day value.</param>
        /// <param name="newDay">The current day value.</param>
        internal static void InvokeDayOfMonthChanged(int priorDay, int newDay)
        {
            TimeEvents.DayOfMonthChanged.Invoke(null, new EventArgsIntChanged(priorDay, newDay));
        }

        /// <summary>Raise a <see cref="YearOfGameChanged"/> event.</summary>
        /// <param name="priorYear">The previous year value.</param>
        /// <param name="newYear">The current year value.</param>
        internal static void InvokeYearOfGameChanged(int priorYear, int newYear)
        {
            TimeEvents.YearOfGameChanged.Invoke(null, new EventArgsIntChanged(priorYear, newYear));
        }

        /// <summary>Raise a <see cref="SeasonOfYearChanged"/> event.</summary>
        /// <param name="priorSeason">The previous season name.</param>
        /// <param name="newSeason">The current season name.</param>
        internal static void InvokeSeasonOfYearChanged(string priorSeason, string newSeason)
        {
            TimeEvents.SeasonOfYearChanged.Invoke(null, new EventArgsStringChanged(priorSeason, newSeason));
        }

        /// <summary>Raise a <see cref="OnNewDay"/> event.</summary>
        /// <param name="priorDay">The previous day value.</param>
        /// <param name="newDay">The current day value.</param>
        /// <param name="isTransitioning">Whether the game just started the transition (<c>true</c>) or finished it (<c>false</c>).</param>
        internal static void InvokeOnNewDay(int priorDay, int newDay, bool isTransitioning)
        {
            TimeEvents.OnNewDay.Invoke(null, new EventArgsNewDay(priorDay, newDay, isTransitioning));
        }
    }
}
