using System.Collections.Generic;
using StardewValley;

namespace StardewModdingAPI
{
    /// <summary>Provides multiplayer utilities.</summary>
    public interface IMultiplayerHelper : IModLinked
    {
        /// <summary>Get a new multiplayer ID.</summary>
        long GetNewID();

        /// <summary>Get the locations which are being actively synced from the host.</summary>
        IEnumerable<GameLocation> GetActiveLocations();
    }
}
