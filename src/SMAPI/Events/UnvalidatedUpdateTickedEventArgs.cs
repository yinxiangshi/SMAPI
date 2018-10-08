using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for an <see cref="ISpecialisedEvents.UnvalidatedUpdateTicked"/> event.</summary>
    public class UnvalidatedUpdateTickedEventArgs : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The number of ticks elapsed since the game started, including the current tick.</summary>
        public uint Ticks { get; }

        /// <summary>Whether <see cref="Ticks"/> is a multiple of 60, which happens approximately once per second.</summary>
        public bool IsOneSecond { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="ticks">The number of ticks elapsed since the game started, including the current tick.</param>
        public UnvalidatedUpdateTickedEventArgs(uint ticks)
        {
            this.Ticks = ticks;
            this.IsOneSecond = this.IsMultipleOf(60);
        }

        /// <summary>Get whether <see cref="Ticks"/> is a multiple of the given <paramref name="number"/>. This is mainly useful if you want to run logic intermittently (e.g. <code>e.IsMultipleOf(30)</code> for every half-second).</summary>
        /// <param name="number">The factor to check.</param>
        public bool IsMultipleOf(uint number)
        {
            return this.Ticks % number == 0;
        }
    }
}
