using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using Newtonsoft.Json;
using StardewModdingAPI.Framework.Events;
using StardewModdingAPI.Framework.Networking;
using StardewModdingAPI.Framework.Reflection;
using StardewModdingAPI.Toolkit.Serialisation;
using StardewValley;
using StardewValley.Network;

namespace StardewModdingAPI.Framework
{
    /// <summary>SMAPI's implementation of the game's core multiplayer logic.</summary>
    internal class SMultiplayer : Multiplayer
    {
        /*********
        ** Properties
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Tracks the installed mods.</summary>
        private readonly ModRegistry ModRegistry;

        /// <summary>Encapsulates SMAPI's JSON file parsing.</summary>
        private readonly JsonHelper JsonHelper;

        /// <summary>Simplifies access to private code.</summary>
        private readonly Reflector Reflection;

        /// <summary>Manages SMAPI events.</summary>
        private readonly EventManager EventManager;

        /// <summary>The players who are currently disconnecting.</summary>
        private readonly IList<long> DisconnectingFarmers;

        /// <summary>Whether SMAPI should log more detailed information.</summary>
        private readonly bool VerboseLogging;


        /*********
        ** Accessors
        *********/
        /// <summary>The message ID for a SMAPI message containing context about a player.</summary>
        public const byte ContextSyncMessageID = 255;

        /// <summary>The metadata for each connected peer.</summary>
        public IDictionary<long, MultiplayerPeer> Peers { get; } = new Dictionary<long, MultiplayerPeer>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="eventManager">Manages SMAPI events.</param>
        /// <param name="jsonHelper">Encapsulates SMAPI's JSON file parsing.</param>
        /// <param name="modRegistry">Tracks the installed mods.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="verboseLogging">Whether SMAPI should log more detailed information.</param>
        public SMultiplayer(IMonitor monitor, EventManager eventManager, JsonHelper jsonHelper, ModRegistry modRegistry, Reflector reflection, bool verboseLogging)
        {
            this.Monitor = monitor;
            this.EventManager = eventManager;
            this.JsonHelper = jsonHelper;
            this.ModRegistry = modRegistry;
            this.Reflection = reflection;
            this.VerboseLogging = verboseLogging;

            this.DisconnectingFarmers = reflection.GetField<List<long>>(this, "disconnectingFarmers").GetValue();
        }

        /// <summary>Handle sync messages from other players and perform other initial sync logic.</summary>
        public override void UpdateEarly()
        {
            this.EventManager.Legacy_BeforeMainSync.Raise();
            base.UpdateEarly();
            this.EventManager.Legacy_AfterMainSync.Raise();
        }

        /// <summary>Broadcast sync messages to other players and perform other final sync logic.</summary>
        public override void UpdateLate(bool forceSync = false)
        {
            this.EventManager.Legacy_BeforeMainBroadcast.Raise();
            base.UpdateLate(forceSync);
            this.EventManager.Legacy_AfterMainBroadcast.Raise();
        }

        /// <summary>Initialise a client before the game connects to a remote server.</summary>
        /// <param name="client">The client to initialise.</param>
        public override Client InitClient(Client client)
        {
            if (client is LidgrenClient)
            {
                string address = this.Reflection.GetField<string>(client, "address").GetValue();
                return new SLidgrenClient(address, this.GetContextSyncMessageFields, this.TryProcessMessageFromServer);
            }

            return client;
        }

        /// <summary>Initialise a server before the game connects to an incoming player.</summary>
        /// <param name="server">The server to initialise.</param>
        public override Server InitServer(Server server)
        {
            if (server is LidgrenServer)
            {
                IGameServer gameServer = this.Reflection.GetField<IGameServer>(server, "gameServer").GetValue();
                return new SLidgrenServer(gameServer);
            }

            return server;
        }

        /// <summary>Process an incoming network message from an unknown farmhand.</summary>
        /// <param name="server">The server instance that received the connection.</param>
        /// <param name="rawMessage">The raw network message that was received.</param>
        /// <param name="message">The message to process.</param>
        public void ProcessMessageFromUnknownFarmhand(Server server, NetIncomingMessage rawMessage, IncomingMessage message)
        {
            // ignore invalid message (farmhands should only receive messages from the server)
            if (!Game1.IsMasterGame)
                return;

            // sync SMAPI context with connected instances
            if (message.MessageType == SMultiplayer.ContextSyncMessageID)
            {
                // get server
                if (!(server is SLidgrenServer customServer))
                {
                    this.Monitor.Log($"Received context from farmhand {message.FarmerID} via unknown client {server.GetType().FullName}. Mods will not be able to sync data to that player.", LogLevel.Warn);
                    return;
                }

                // parse message
                string data = message.Reader.ReadString();
                RemoteContextModel model = this.JsonHelper.Deserialise<RemoteContextModel>(data);
                if (model.ApiVersion == null)
                    model = null; // no data available for unmodded players

                // log info
                if (model != null)
                    this.Monitor.Log($"Received context for farmhand {message.FarmerID} running SMAPI {model.ApiVersion} with {model.Mods.Length} mods{(this.VerboseLogging ? $": {data}" : "")}.", LogLevel.Trace);
                else
                    this.Monitor.Log($"Received context for farmhand {message.FarmerID} running vanilla{(this.VerboseLogging ? $": {data}" : "")}.", LogLevel.Trace);

                // store peer
                MultiplayerPeer newPeer = this.Peers[message.FarmerID] = MultiplayerPeer.ForConnectionToFarmhand(message.FarmerID, model, customServer, rawMessage.SenderConnection);

                // reply with known contexts
                if (this.VerboseLogging)
                    this.Monitor.Log("   Replying with context for current player...", LogLevel.Trace);
                newPeer.SendMessage(new OutgoingMessage(SMultiplayer.ContextSyncMessageID, Game1.player.UniqueMultiplayerID, this.GetContextSyncMessageFields()));
                foreach (MultiplayerPeer otherPeer in this.Peers.Values.Where(p => p.PlayerID != newPeer.PlayerID))
                {
                    if (this.VerboseLogging)
                        this.Monitor.Log($"   Replying with context for player {otherPeer.PlayerID}...", LogLevel.Trace);
                    newPeer.SendMessage(new OutgoingMessage(SMultiplayer.ContextSyncMessageID, otherPeer.PlayerID, this.GetContextSyncMessageFields(otherPeer)));
                }

                // forward to other peers
                if (this.Peers.Count > 1)
                {
                    object[] fields = this.GetContextSyncMessageFields(newPeer);
                    foreach (MultiplayerPeer otherPeer in this.Peers.Values.Where(p => p.PlayerID != newPeer.PlayerID))
                    {
                        if (this.VerboseLogging)
                            this.Monitor.Log($"   Forwarding context to player {otherPeer.PlayerID}...", LogLevel.Trace);
                        otherPeer.SendMessage(new OutgoingMessage(SMultiplayer.ContextSyncMessageID, newPeer.PlayerID, fields));
                    }
                }
            }

            // handle intro from unmodded player
            else if (message.MessageType == Multiplayer.playerIntroduction && !this.Peers.ContainsKey(message.FarmerID))
            {
                // get server
                if (!(server is SLidgrenServer customServer))
                {
                    this.Monitor.Log($"Received connection from farmhand {message.FarmerID} with unknown client {server.GetType().FullName}. Mods will not be able to sync data to that player.", LogLevel.Warn);
                    return;
                }

                // store peer
                this.Monitor.Log($"Received connection for vanilla player {message.FarmerID}.", LogLevel.Trace);
                this.Peers[message.FarmerID] = MultiplayerPeer.ForConnectionToFarmhand(message.FarmerID, null, customServer, rawMessage.SenderConnection);
            }
        }

        /// <summary>Process an incoming network message from the server.</summary>
        /// <param name="client">The client instance that received the connection.</param>
        /// <param name="message">The message to process.</param>
        /// <returns>Returns whether the message was handled.</returns>
        public bool TryProcessMessageFromServer(SLidgrenClient client, IncomingMessage message)
        {
            // receive SMAPI context from a connected player
            if (message.MessageType == SMultiplayer.ContextSyncMessageID)
            {
                // parse message
                string data = message.Reader.ReadString();
                RemoteContextModel model = this.JsonHelper.Deserialise<RemoteContextModel>(data);

                // log info
                if (model != null)
                    this.Monitor.Log($"Received context for {(model.IsHost ? "host" : "farmhand")} {message.FarmerID} running SMAPI {model.ApiVersion} with {model.Mods.Length} mods{(this.VerboseLogging ? $": {data}" : "")}.", LogLevel.Trace);
                else
                    this.Monitor.Log($"Received context for player {message.FarmerID} running vanilla{(this.VerboseLogging ? $": {data}" : "")}.", LogLevel.Trace);

                // store peer
                this.Peers[message.FarmerID] = MultiplayerPeer.ForConnectionToHost(message.FarmerID, model, client);
                return true;
            }

            // handle intro from unmodded player
            if (message.MessageType == Multiplayer.playerIntroduction && !this.Peers.ContainsKey(message.FarmerID))
            {
                // store peer
                this.Monitor.Log($"Received connection for vanilla player {message.FarmerID}.", LogLevel.Trace);
                this.Peers[message.FarmerID] = MultiplayerPeer.ForConnectionToHost(message.FarmerID, null, client);
            }

            return false;
        }

        /// <summary>Remove players who are disconnecting.</summary>
        protected override void removeDisconnectedFarmers()
        {
            foreach (long playerID in this.DisconnectingFarmers)
            {
                this.Monitor.Log($"Player quit: {playerID}", LogLevel.Trace);
                this.Peers.Remove(playerID);
            }

            base.removeDisconnectedFarmers();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the fields to include in a context sync message sent to other players.</summary>
        private object[] GetContextSyncMessageFields()
        {
            RemoteContextModel model = new RemoteContextModel
            {
                IsHost = Context.IsWorldReady && Context.IsMainPlayer,
                Platform = Constants.TargetPlatform,
                ApiVersion = Constants.ApiVersion,
                GameVersion = Constants.GameVersion,
                Mods = this.ModRegistry
                    .GetAll()
                    .Select(mod => new RemoteContextModModel
                    {
                        ID = mod.Manifest.UniqueID,
                        Name = mod.Manifest.Name,
                        Version = mod.Manifest.Version
                    })
                    .ToArray()
            };

            return new object[] { this.JsonHelper.Serialise(model, Formatting.None) };
        }

        /// <summary>Get the fields to include in a context sync message sent to other players.</summary>
        /// <param name="peer">The peer whose data to represent.</param>
        private object[] GetContextSyncMessageFields(IMultiplayerPeer peer)
        {
            if (!peer.HasSmapi)
                return new object[] { "{}" };

            RemoteContextModel model = new RemoteContextModel
            {
                IsHost = peer.IsHostPlayer,
                Platform = peer.Platform.Value,
                ApiVersion = peer.ApiVersion,
                GameVersion = peer.GameVersion,
                Mods = peer.Mods
                    .Select(mod => new RemoteContextModModel
                    {
                        ID = mod.ID,
                        Name = mod.Name,
                        Version = mod.Version
                    })
                    .ToArray()
            };

            return new object[] { this.JsonHelper.Serialise(model, Formatting.None) };
        }
    }
}
