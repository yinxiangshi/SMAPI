using System;
using System.IO;
using Galaxy.Api;
using StardewModdingAPI.Framework.Reflection;
using StardewValley;
using StardewValley.Network;
using StardewValley.SDKs;

namespace StardewModdingAPI.Framework.Networking
{
    /// <summary>A multiplayer server used to connect to an incoming player. This is an implementation of <see cref="LidgrenServer"/> that adds support for SMAPI's metadata context exchange.</summary>
    internal class SGalaxyNetServer : GalaxyNetServer
    {
        /*********
        ** Properties
        *********/
        /// <summary>A callback to raise when receiving a message. This receives the incoming message, a method to send a message, and a callback to run the default logic.</summary>
        private readonly Action<IncomingMessage, Action<OutgoingMessage>, Action> OnProcessingMessage;

        /// <summary>The peer connections.</summary>
        private readonly Bimap<long, ulong> Peers;

        /// <summary>The underlying net server.</summary>
        private readonly IReflectedField<GalaxySocket> Server;

        /// <summary>The underlying method which handles incoming connections.</summary>
        private readonly Action<GalaxyID> BaseReceiveConnection;

        /// <summary>The underlying method which handles incoming disconnections.</summary>
        private readonly Action<GalaxyID> BaseReceiveDisconnect;

        /// <summary>The underlying method which handles incoming errors.</summary>
        private readonly Action<string> BaseReceiveError;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameServer">The underlying game server.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="onProcessingMessage">A callback to raise when receiving a message. This receives the incoming message, a method to send a message, and a callback to run the default logic.</param>
        public SGalaxyNetServer(IGameServer gameServer, Reflector reflection, Action<IncomingMessage, Action<OutgoingMessage>, Action> onProcessingMessage)
            : base(gameServer)
        {
            this.OnProcessingMessage = onProcessingMessage;
            this.Peers = reflection.GetField<Bimap<long, ulong>>(this, "peers").GetValue();
            this.Server = reflection.GetField<GalaxySocket>(this, "server");

            this.BaseReceiveConnection = (Action<GalaxyID>)Delegate.CreateDelegate(typeof(Action<GalaxyID>), this, reflection.GetMethod(this, "onReceiveConnection").MethodInfo);
            this.BaseReceiveDisconnect = (Action<GalaxyID>)Delegate.CreateDelegate(typeof(Action<GalaxyID>), this, reflection.GetMethod(this, "onReceiveDisconnect").MethodInfo);
            this.BaseReceiveError = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), this, reflection.GetMethod(this, "onReceiveError").MethodInfo);
        }

        /// <summary>Receive and process messages from the client.</summary>
        public override void receiveMessages()
        {
            GalaxySocket server = this.Server.GetValue();
            if (server == null)
                return;

            server.Receive(this.BaseReceiveConnection, this.OnReceiveMessage, this.BaseReceiveDisconnect, this.BaseReceiveError);
            server.Heartbeat(server.LobbyMembers());
            foreach (GalaxyID connection in server.Connections)
            {
                if (server.GetPingWith(connection) > 30000L)
                    server.Kick(connection);
            }
        }

        /// <summary>Read and process a message from the client.</summary>
        /// <param name="peerID">The Galaxy peer ID.</param>
        /// <param name="data">The data to process.</param>
        private void OnReceiveMessage(GalaxyID peerID, Stream data)
        {
            using (IncomingMessage message = new IncomingMessage())
            using (BinaryReader reader = new BinaryReader(data))
            {
                message.Read(reader);
                this.OnProcessingMessage(message, outgoing => this.sendMessage(peerID, outgoing), () =>
                {
                    if (this.Peers.ContainsLeft(message.FarmerID) && (long)this.Peers[message.FarmerID] == (long)peerID.ToUint64())
                    {
                        this.gameServer.processIncomingMessage(message);
                    }
                    else if (message.MessageType == Multiplayer.playerIntroduction)
                    {
                        NetFarmerRoot farmer = Game1.multiplayer.readFarmer(message.Reader);
                        GalaxyID capturedPeer = new GalaxyID(peerID.ToUint64());
                        this.gameServer.checkFarmhandRequest(Convert.ToString(peerID.ToUint64()), farmer, msg => this.sendMessage(capturedPeer, msg), () => this.Peers[farmer.Value.UniqueMultiplayerID] = capturedPeer.ToUint64());
                    }
                });
            }
        }
    }
}
