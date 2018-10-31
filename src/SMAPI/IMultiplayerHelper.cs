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

        /* disable until ready for release:

        /// <summary>Get a connected player.</summary>
        /// <param name="id">The player's unique ID.</param>
        /// <returns>Returns the connected player, or <c>null</c> if no such player is connected.</returns>
        IMultiplayerPeer GetConnectedPlayer(long id);

        /// <summary>Get all connected players.</summary>
        IEnumerable<IMultiplayerPeer> GetConnectedPlayers();
        */
    }
}
