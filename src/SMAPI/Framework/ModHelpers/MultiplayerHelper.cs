using System.Collections.Generic;
using StardewValley;

namespace StardewModdingAPI.Framework.ModHelpers
{
    /// <summary>Provides multiplayer utilities.</summary>
    internal class MultiplayerHelper : BaseHelper, IMultiplayerHelper
    {
        /*********
        ** Properties
        *********/
        /// <summary>SMAPI's core multiplayer utility.</summary>
        private readonly SMultiplayer Multiplayer;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modID">The unique ID of the relevant mod.</param>
        /// <param name="multiplayer">SMAPI's core multiplayer utility.</param>
        public MultiplayerHelper(string modID, SMultiplayer multiplayer)
            : base(modID)
        {
            this.Multiplayer = multiplayer;
        }

        /// <summary>Get the locations which are being actively synced from the host.</summary>
        public IEnumerable<GameLocation> GetActiveLocations()
        {
            return this.Multiplayer.activeLocations();
        }

        /// <summary>Get a new multiplayer ID.</summary>
        public long GetNewID()
        {
            return this.Multiplayer.getNewID();
        }
    }
}
