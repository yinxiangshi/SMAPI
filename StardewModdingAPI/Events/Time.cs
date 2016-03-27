using System;

namespace StardewModdingAPI.Events
{
    public static class TimeEvents
    {
        public static event EventHandler<EventArgsIntChanged> TimeOfDayChanged = delegate { };
        public static event EventHandler<EventArgsIntChanged> DayOfMonthChanged = delegate { };
        public static event EventHandler<EventArgsIntChanged> YearOfGameChanged = delegate { };
        public static event EventHandler<EventArgsStringChanged> SeasonOfYearChanged = delegate { };
        public static event EventHandler OnNewDay = delegate { };

        public static void InvokeTimeOfDayChanged(int priorInt, int newInt)
        {
            TimeOfDayChanged.Invoke(null, new EventArgsIntChanged(priorInt, newInt));
        }

        public static void InvokeDayOfMonthChanged(int priorInt, int newInt)
        {
            DayOfMonthChanged.Invoke(null, new EventArgsIntChanged(priorInt, newInt));
        }

        public static void InvokeYearOfGameChanged(int priorInt, int newInt)
        {
            YearOfGameChanged.Invoke(null, new EventArgsIntChanged(priorInt, newInt));
        }

        public static void InvokeSeasonOfYearChanged(string priorString, string newString)
        {
            SeasonOfYearChanged.Invoke(null, new EventArgsStringChanged(priorString, newString));
        }

        public static void InvokeOnNewDay(int priorInt, int newInt)
        {
            OnNewDay.Invoke(null, new EventArgsIntChanged(priorInt, newInt));
        }
    }
}