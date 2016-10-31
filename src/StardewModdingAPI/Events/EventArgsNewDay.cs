using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="TimeEvents.OnNewDay"/> event.</summary>
    public class EventArgsNewDay : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The previous day value.</summary>
        public int PreviousDay { get; private set; }

        /// <summary>The current day value.</summary>
        public int CurrentDay { get; private set; }

        /// <summary>Whether the game just started the transition (<c>true</c>) or finished it (<c>false</c>).</summary>
        public bool IsNewDay { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="priorDay">The previous day value.</param>
        /// <param name="newDay">The current day value.</param>
        /// <param name="isTransitioning">Whether the game just started the transition (<c>true</c>) or finished it (<c>false</c>).</param>
        public EventArgsNewDay(int priorDay, int newDay, bool isTransitioning)
        {
            this.PreviousDay = priorDay;
            this.CurrentDay = newDay;
            this.IsNewDay = isTransitioning;
        }
    }
}
