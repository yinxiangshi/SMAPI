using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised for multiplayer messages and connections.</summary>
    public interface IMultiplayerEvents
    {
        /// <summary>Raised after the mod context for a player is received. This happens before the game approves the connection, so the player does not yet exist in the game. This is the earliest point where messages can be sent to the player via SMAPI.</summary>
        event EventHandler<ContextReceivedEventArgs> ContextReceived;

        /// <summary>Raised after a mod message is received over the network.</summary>
        event EventHandler<ModMessageReceivedEventArgs> ModMessageReceived;
    }
}
