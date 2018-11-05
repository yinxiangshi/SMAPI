using System;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for an <see cref="IMultiplayerEvents.ContextReceived"/> event.</summary>
    public class ContextReceivedEventArgs : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The player whose metadata was received.</summary>
        public IMultiplayerPeer Peer { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="peer">The player to whom a connection is being established.</param>
        internal ContextReceivedEventArgs(IMultiplayerPeer peer)
        {
            this.Peer = peer;
        }
    }
}
