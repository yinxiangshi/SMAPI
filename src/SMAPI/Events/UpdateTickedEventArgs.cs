using System;
using StardewValley;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for an <see cref="IGameLoopEvents.UpdateTicked"/> event.</summary>
    public class UpdateTickedEventArgs : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The number of ticks elapsed since the game started, including the current tick.</summary>
        public uint Ticks => (uint)Game1.ticks;

        /// <summary>Whether <see cref="Ticks"/> is a multiple of 60, which happens approximately once per second.</summary>
        public bool IsOneSecond => Game1.ticks % 60 == 0;


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether <see cref="Ticks"/> is a multiple of the given <paramref name="number"/>. This is mainly useful if you want to run logic intermittently (e.g. <code>e.IsMultipleOf(30)</code> for every half-second).</summary>
        /// <param name="number">The factor to check.</param>
        public bool IsMultipleOf(uint number)
        {
            return this.Ticks % number == 0;
        }
    }
}
