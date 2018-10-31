using System;
using StardewValley;
using StardewValley.Network;

namespace StardewModdingAPI.Framework.Networking
{
    /// <summary>A multiplayer client used to connect to a hosted server. This is an implementation of <see cref="LidgrenClient"/> that adds support for SMAPI's metadata context exchange.</summary>
    internal class SLidgrenClient : LidgrenClient
    {
        /*********
        ** Properties
        *********/
        /// <summary>Get the metadata to include in a metadata message sent to other players.</summary>
        private readonly Func<object[]> GetMetadataMessageFields;

        /// <summary>The method to call when receiving a custom SMAPI message from the server, which returns whether the message was processed.</summary>
        private readonly Func<SLidgrenClient, IncomingMessage, bool> TryProcessMessage;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="address">The remote address being connected.</param>
        /// <param name="getMetadataMessageFields">Get the metadata to include in a metadata message sent to other players.</param>
        /// <param name="tryProcessMessage">The method to call when receiving a custom SMAPI message from the server, which returns whether the message was processed..</param>
        public SLidgrenClient(string address, Func<object[]> getMetadataMessageFields, Func<SLidgrenClient, IncomingMessage, bool> tryProcessMessage)
            : base(address)
        {
            this.GetMetadataMessageFields = getMetadataMessageFields;
            this.TryProcessMessage = tryProcessMessage;
        }

        /// <summary>Send the metadata needed to connect with a remote server.</summary>
        public override void sendPlayerIntroduction()
        {
            // send custom intro
            if (this.getUserID() != "")
                Game1.player.userID.Value = this.getUserID();
            this.sendMessage(SMultiplayer.ContextSyncMessageID, this.GetMetadataMessageFields());
            base.sendPlayerIntroduction();
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Process an incoming network message.</summary>
        /// <param name="message">The message to process.</param>
        protected override void processIncomingMessage(IncomingMessage message)
        {
            if (this.TryProcessMessage(this, message))
                return;

            base.processIncomingMessage(message);
        }
    }
}
