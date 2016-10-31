using System;

namespace StardewModdingAPI.Events
{
    public class EventArgsIntChanged : EventArgs
    {
        public EventArgsIntChanged(int priorInt, int newInt)
        {
            NewInt = NewInt;
            PriorInt = PriorInt;
        }

        public int NewInt { get; }
        public int PriorInt { get; }
    }
}