using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised when something happens in the mines.</summary>
    public static class MineEvents
    {
        /*********
        ** Events
        *********/
        /// <summary>Raised after the player warps to a new level of the mine.</summary>
        public static event EventHandler<EventArgsMineLevelChanged> MineLevelChanged;


        /*********
        ** Internal methods
        *********/
        /// <summary>Raise a <see cref="MineLevelChanged"/> event.</summary>
        /// <param name="previousMineLevel">The previous mine level.</param>
        /// <param name="currentMineLevel">The current mine level.</param>
        internal static void InvokeMineLevelChanged(int previousMineLevel, int currentMineLevel)
        {
            MineEvents.MineLevelChanged?.Invoke(null, new EventArgsMineLevelChanged(previousMineLevel, currentMineLevel));
        }
    }
}
