using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Lidgren.Network;
using StardewModdingAPI.Framework.Reflection;
using StardewModdingAPI.Patches;
using StardewValley;
using StardewValley.Network;

namespace StardewModdingAPI.Framework.Networking
{
    /// <summary>A multiplayer server used to connect to an incoming player. This is an implementation of <see cref="LidgrenServer"/> that adds support for SMAPI's metadata context exchange.</summary>
    internal class SLidgrenServer : LidgrenServer
    {
        /*********
        ** Properties
        *********/

        /// <summary>The constructor for the internal <c>NetBufferReadStream</c> type.</summary>
        private readonly ConstructorInfo NetBufferReadStreamConstructor = SLidgrenServer.GetNetBufferReadStreamConstructor();

        /// <summary>The constructor for the internal <c>NetBufferWriteStream</c> type.</summary>
        private readonly ConstructorInfo NetBufferWriteStreamConstructor = SLidgrenServer.GetNetBufferWriteStreamConstructor();

        /// <summary>A method which reads farmer data from the given binary reader.</summary>
        private readonly Func<BinaryReader, NetFarmerRoot> ReadFarmer;

        /// <summary>A callback to raise when receiving a message. This receives the server instance, raw/parsed incoming message, and a callback to run the default logic.</summary>
        private readonly Action<SLidgrenServer, NetIncomingMessage, IncomingMessage, Action> OnProcessingMessage;

        /// <summary>A callback to raise when sending a message. This receives the server instance, outgoing connection, outgoing message, target player ID, and a callback to run the default logic.</summary>
        private readonly Action<SLidgrenServer, NetConnection, OutgoingMessage, Action> OnSendingMessage;

        /// <summary>The peer connections.</summary>
        private readonly Bimap<long, NetConnection> Peers;

        /// <summary>The underlying net server.</summary>
        private readonly IReflectedField<NetServer> Server;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameServer">The underlying game server.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="readFarmer">A method which reads farmer data from the given binary reader.</param>
        /// <param name="onProcessingMessage">A callback to raise when receiving a message. This receives the server instance, raw/parsed incoming message, and a callback to run the default logic.</param>
        /// <param name="onSendingMessage">A callback to raise when sending a message. This receives the server instance, outgoing connection, outgoing message, and a callback to run the default logic.</param>
        public SLidgrenServer(IGameServer gameServer, Reflector reflection, Func<BinaryReader, NetFarmerRoot> readFarmer, Action<SLidgrenServer, NetIncomingMessage, IncomingMessage, Action> onProcessingMessage, Action<SLidgrenServer, NetConnection, OutgoingMessage, Action> onSendingMessage)
            : base(gameServer)
        {
            this.ReadFarmer = readFarmer;
            this.OnProcessingMessage = onProcessingMessage;
            this.OnSendingMessage = onSendingMessage;
            this.Peers = reflection.GetField<Bimap<long, NetConnection>>(this, "peers").GetValue();
            this.Server = reflection.GetField<NetServer>(this, "server");
        }

        /// <summary>Send a message to a remote server.</summary>
        /// <param name="connection">The network connection.</param>
        /// <param name="message">The message to send.</param>
        /// <remarks>This is an implementation of <see cref="LidgrenServer.sendMessage(NetConnection, OutgoingMessage)"/> which calls <see cref="OnSendingMessage"/>. This method is invoked via <see cref="LidgrenServerPatch.Prefix_LidgrenServer_SendMessage"/>.</remarks>
        public void SendMessage(NetConnection connection, OutgoingMessage message)
        {
            this.OnSendingMessage(this, connection, message, () =>
            {
                NetServer server = this.Server.GetValue();
                NetOutgoingMessage netMessage = server.CreateMessage();
                using (Stream bufferWriteStream = (Stream)this.NetBufferWriteStreamConstructor.Invoke(new object[] { netMessage }))
                using (BinaryWriter writer = new BinaryWriter(bufferWriteStream))
                    message.Write(writer);

                server.SendMessage(netMessage, connection, NetDeliveryMethod.ReliableOrdered);
            });
        }

        /// <summary>Parse a data message from a client.</summary>
        /// <param name="rawMessage">The raw network message to parse.</param>
        /// <remarks>This is an implementation of <see cref="LidgrenServer.parseDataMessageFromClient"/> which calls <see cref="OnProcessingMessage"/>. This method is invoked via <see cref="LidgrenServerPatch.Prefix_LidgrenServer_ParseDataMessageFromClient"/>.</remarks>
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "The callback is invoked synchronously.")]
        public bool ParseDataMessageFromClient(NetIncomingMessage rawMessage)
        {
            // add hook to call multiplayer core
            NetConnection peer = rawMessage.SenderConnection;
            using (IncomingMessage message = new IncomingMessage())
            using (Stream readStream = (Stream)this.NetBufferReadStreamConstructor.Invoke(new object[] { rawMessage }))
            using (BinaryReader reader = new BinaryReader(readStream))
            {
                while (rawMessage.LengthBits - rawMessage.Position >= 8)
                {
                    message.Read(reader);
                    this.OnProcessingMessage(this, rawMessage, message, () =>
                    {
                        if (this.Peers.ContainsLeft(message.FarmerID) && this.Peers[message.FarmerID] == peer)
                            this.gameServer.processIncomingMessage(message);
                        else if (message.MessageType == Multiplayer.playerIntroduction)
                        {
                            NetFarmerRoot farmer = this.ReadFarmer(message.Reader);
                            this.gameServer.checkFarmhandRequest("", farmer, msg => this.SendMessage(peer, msg), () => this.Peers[farmer.Value.UniqueMultiplayerID] = peer);
                        }
                    });
                }
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the constructor for the internal <c>NetBufferReadStream</c> type.</summary>
        private static ConstructorInfo GetNetBufferReadStreamConstructor()
        {
            // get type
            string typeName = $"StardewValley.Network.NetBufferReadStream, {Constants.GameAssemblyName}";
            Type type = Type.GetType(typeName);
            if (type == null)
                throw new InvalidOperationException($"Can't find type: {typeName}");

            // get constructor
            ConstructorInfo constructor = type.GetConstructor(new[] { typeof(NetBuffer) });
            if (constructor == null)
                throw new InvalidOperationException($"Can't find constructor for type: {typeName}");

            return constructor;
        }

        /// <summary>Get the constructor for the internal <c>NetBufferWriteStream</c> type.</summary>
        private static ConstructorInfo GetNetBufferWriteStreamConstructor()
        {
            // get type
            string typeName = $"StardewValley.Network.NetBufferWriteStream, {Constants.GameAssemblyName}";
            Type type = Type.GetType(typeName);
            if (type == null)
                throw new InvalidOperationException($"Can't find type: {typeName}");

            // get constructor
            ConstructorInfo constructor = type.GetConstructor(new[] { typeof(NetBuffer) });
            if (constructor == null)
                throw new InvalidOperationException($"Can't find constructor for type: {typeName}");

            return constructor;
        }
    }
}
