using System;
using StardewValley;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for a <see cref="PlayerEvents.FarmerChanged"/> event.</summary>
    public class EventArgsFarmerChanged : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The previous player character.</summary>
        public Farmer NewFarmer { get; }

        /// <summary>The new player character.</summary>
        public Farmer PriorFarmer { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="priorFarmer">The previous player character.</param>
        /// <param name="newFarmer">The new player character.</param>
        public EventArgsFarmerChanged(Farmer priorFarmer, Farmer newFarmer)
        {
            this.PriorFarmer = priorFarmer;
            this.NewFarmer = newFarmer;
        }
    }
}
