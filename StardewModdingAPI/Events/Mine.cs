using System;

namespace StardewModdingAPI.Events
{
    public static class MineEvents
    {
        public static event EventHandler<EventArgsCurrentLocationChanged> MineLevelChanged = delegate { };

        public static void InvokeLocationsChanged(int currentMineLevel)
        {

        }
    }
}
