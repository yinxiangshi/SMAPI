using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for an <see cref="IGameLoopEvents.OneSecondUpdateTicked"/> event.</summary>
    public class OneSecondUpdateTickedEventArgs : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The number of ticks elapsed since the game started, including the current tick.</summary>
        public uint Ticks { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="ticks">The number of ticks elapsed since the game started, including the current tick.</param>
        internal OneSecondUpdateTickedEventArgs(uint ticks)
        {
            this.Ticks = ticks;
        }

        /// <summary>Get whether <see cref="Ticks"/> is a multiple of the given <paramref name="number"/>. This is mainly useful if you want to run logic intermittently (e.g. <code>e.IsMultipleOf(30)</code> for every half-second).</summary>
        /// <param name="number">The factor to check.</param>
        public bool IsMultipleOf(uint number)
        {
            return this.Ticks % number == 0;
        }
    }
}
