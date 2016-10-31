using System;

namespace StardewModdingAPI.Events
{
    public class EventArgsNewDay : EventArgs
    {
        public EventArgsNewDay(int prevDay, int curDay, bool newDay)
        {
            PreviousDay = prevDay;
            CurrentDay = curDay;
            IsNewDay = newDay;
        }

        public int PreviousDay { get; private set; }
        public int CurrentDay { get; private set; }
        public bool IsNewDay { get; private set; }
    }
}