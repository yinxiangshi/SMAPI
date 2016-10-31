using System;

namespace StardewModdingAPI.Events
{
    public class EventArgsMineLevelChanged : EventArgs
    {
        public EventArgsMineLevelChanged(int previousMineLevel, int currentMineLevel)
        {
            PreviousMineLevel = previousMineLevel;
            CurrentMineLevel = currentMineLevel;
        }

        public int PreviousMineLevel { get; private set; }
        public int CurrentMineLevel { get; private set; }
    }
}