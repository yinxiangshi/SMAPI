using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Events raised for multiplayer messages and connections.</summary>
    public interface IMultiplayerEvents
    {
        /// <summary>Raised after a mod message is received over the network.</summary>
        event EventHandler<ModMessageReceivedEventArgs> ModMessageReceived;
    }
}
