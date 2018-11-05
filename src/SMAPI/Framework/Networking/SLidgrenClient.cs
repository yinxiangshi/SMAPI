using System;
using StardewValley.Network;

namespace StardewModdingAPI.Framework.Networking
{
    /// <summary>A multiplayer client used to connect to a hosted server. This is an implementation of <see cref="LidgrenClient"/> with callbacks for SMAPI functionality.</summary>
    internal class SLidgrenClient : LidgrenClient
    {
        /*********
        ** Properties
        *********/
        /// <summary>A callback to raise when receiving a message. This receives the client instance, incoming message, and a callback to run the default logic.</summary>
        private readonly Action<SLidgrenClient, IncomingMessage, Action> OnProcessingMessage;

        /// <summary>A callback to raise when sending a message. This receives the client instance, outgoing message, and a callback to run the default logic.</summary>
        private readonly Action<SLidgrenClient, OutgoingMessage, Action> OnSendingMessage;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="address">The remote address being connected.</param>
        /// <param name="onProcessingMessage">A callback to raise when receiving a message. This receives the client instance, incoming message, and a callback to run the default logic.</param>
        /// <param name="onSendingMessage">A callback to raise when sending a message. This receives the client instance, outgoing message, and a callback to run the default logic.</param>
        public SLidgrenClient(string address, Action<SLidgrenClient, IncomingMessage, Action> onProcessingMessage, Action<SLidgrenClient, OutgoingMessage, Action> onSendingMessage)
            : base(address)
        {
            this.OnProcessingMessage = onProcessingMessage;
            this.OnSendingMessage = onSendingMessage;
        }

        /// <summary>Send a message to the connected peer.</summary>
        public override void sendMessage(OutgoingMessage message)
        {
            this.OnSendingMessage(this, message, () => base.sendMessage(message));
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Process an incoming network message.</summary>
        /// <param name="message">The message to process.</param>
        protected override void processIncomingMessage(IncomingMessage message)
        {
            this.OnProcessingMessage(this, message, () => base.processIncomingMessage(message));
        }
    }
}
