using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        /// <summary>A callback to invoke when a mod message is received.</summary>
        private readonly Action<ModMessageModel> OnModMessageReceived;


        /*********
        ** Accessors
        *********/
        /// <summary>The message ID for a SMAPI message containing context about a player.</summary>
        public const byte ContextSyncMessageID = 255;

        /// <summary>The message ID for a mod message.</summary>
        public const byte ModMessageID = 254;

        /// <summary>The metadata for each connected peer.</summary>
        public IDictionary<long, MultiplayerPeer> Peers { get; } = new Dictionary<long, MultiplayerPeer>();

        /// <summary>The metadata for the host player, if the current player is a farmhand.</summary>
        public MultiplayerPeer HostPeer;


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
        /// <param name="onModMessageReceived">A callback to invoke when a mod message is received.</param>
        public SMultiplayer(IMonitor monitor, EventManager eventManager, JsonHelper jsonHelper, ModRegistry modRegistry, Reflector reflection, bool verboseLogging, Action<ModMessageModel> onModMessageReceived)
        {
            this.Monitor = monitor;
            this.EventManager = eventManager;
            this.JsonHelper = jsonHelper;
            this.ModRegistry = modRegistry;
            this.Reflection = reflection;
            this.VerboseLogging = verboseLogging;
            this.OnModMessageReceived = onModMessageReceived;

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

        /// <summary>Process an incoming network message from an unknown farmhand, usually a player whose connection hasn't been approved yet.</summary>
        /// <param name="server">The server instance that received the connection.</param>
        /// <param name="rawMessage">The raw network message that was received.</param>
        /// <param name="message">The message to process.</param>
        public void ProcessMessageFromUnknownFarmhand(Server server, NetIncomingMessage rawMessage, IncomingMessage message)
        {
            // ignore invalid message (farmhands should only receive messages from the server)
            if (!Game1.IsMasterGame)
                return;

            switch (message.MessageType)
            {
                // sync SMAPI context with connected instances
                case SMultiplayer.ContextSyncMessageID:
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
                        this.VerboseLog("   Replying with context for current player...");
                        newPeer.SendMessage(new OutgoingMessage(SMultiplayer.ContextSyncMessageID, Game1.player.UniqueMultiplayerID, this.GetContextSyncMessageFields()));
                        foreach (MultiplayerPeer otherPeer in this.Peers.Values.Where(p => p.PlayerID != newPeer.PlayerID))
                        {
                            this.VerboseLog($"   Replying with context for player {otherPeer.PlayerID}...");
                            newPeer.SendMessage(new OutgoingMessage(SMultiplayer.ContextSyncMessageID, otherPeer.PlayerID, this.GetContextSyncMessageFields(otherPeer)));
                        }

                        // forward to other peers
                        if (this.Peers.Count > 1)
                        {
                            object[] fields = this.GetContextSyncMessageFields(newPeer);
                            foreach (MultiplayerPeer otherPeer in this.Peers.Values.Where(p => p.PlayerID != newPeer.PlayerID))
                            {
                                this.VerboseLog($"   Forwarding context to player {otherPeer.PlayerID}...");
                                otherPeer.SendMessage(new OutgoingMessage(SMultiplayer.ContextSyncMessageID, newPeer.PlayerID, fields));
                            }
                        }
                    }
                    break;

                // handle intro from unmodded player
                case Multiplayer.playerIntroduction:
                    if (!this.Peers.ContainsKey(message.FarmerID))
                    {
                        // get server
                        if (!(server is SLidgrenServer customServer))
                        {
                            this.Monitor.Log($"Received connection from farmhand {message.FarmerID} with unknown client {server.GetType().FullName}. Mods will not be able to sync data to that player.", LogLevel.Warn);
                            return;
                        }

                        // store peer
                        this.Monitor.Log($"Received connection for vanilla player {message.FarmerID}.", LogLevel.Trace);
                        var peer = MultiplayerPeer.ForConnectionToFarmhand(message.FarmerID, null, customServer, rawMessage.SenderConnection);
                        this.Peers[message.FarmerID] = peer;
                        if (peer.IsHost)
                            this.HostPeer = peer;
                    }
                    break;

                // handle mod message
                case SMultiplayer.ModMessageID:
                    this.ReceiveModMessage(message);
                    break;
            }
        }

        /// <summary>Process an incoming message from an approved connection.</summary>
        /// <param name="message">The message to process.</param>
        public override void processIncomingMessage(IncomingMessage message)
        {
            switch (message.MessageType)
            {
                // handle mod message
                case SMultiplayer.ModMessageID:
                    this.ReceiveModMessage(message);
                    break;

                // let game process message
                default:
                    base.processIncomingMessage(message);
                    break;
            }
            
        }

        /// <summary>Process an incoming network message from the server.</summary>
        /// <param name="client">The client instance that received the connection.</param>
        /// <param name="message">The message to process.</param>
        /// <returns>Returns whether the message was handled.</returns>
        public bool TryProcessMessageFromServer(SLidgrenClient client, IncomingMessage message)
        {
            switch (message.MessageType)
            {
                // receive SMAPI context from a connected player
                case SMultiplayer.ContextSyncMessageID:
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
                        MultiplayerPeer peer = MultiplayerPeer.ForConnectionToHost(message.FarmerID, model, client);
                        this.Peers[message.FarmerID] = peer;
                        if (peer.IsHost)
                            this.HostPeer = peer;
                    }
                    return true;

                // handle intro from unmodded player
                case Multiplayer.playerIntroduction:
                    if (!this.Peers.ContainsKey(message.FarmerID))
                    {
                        // store peer
                        this.Monitor.Log($"Received connection for vanilla player {message.FarmerID}.", LogLevel.Trace);
                        var peer = MultiplayerPeer.ForConnectionToHost(message.FarmerID, null, client);
                        this.Peers[message.FarmerID] = peer;
                        if (peer.IsHost)
                            this.HostPeer = peer;
                    }
                    return false;

                // handle mod message
                case SMultiplayer.ModMessageID:
                    this.ReceiveModMessage(message);
                    return true;

                default:
                    return false;
            }
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

        /// <summary>Broadcast a mod message to matching players.</summary>
        /// <param name="message">The data to send over the network.</param>
        /// <param name="messageType">A message type which receiving mods can use to decide whether it's the one they want to handle, like <c>SetPlayerLocation</c>. This doesn't need to be globally unique, since mods should check the originating mod ID.</param>
        /// <param name="fromModID">The unique ID of the mod sending the message.</param>
        /// <param name="toModIDs">The mod IDs which should receive the message on the destination computers, or <c>null</c> for all mods. Specifying mod IDs is recommended to improve performance, unless it's a general-purpose broadcast.</param>
        /// <param name="toPlayerIDs">The <see cref="Farmer.UniqueMultiplayerID" /> values for the players who should receive the message, or <c>null</c> for all players. If you don't need to broadcast to all players, specifying player IDs is recommended to reduce latency.</param>
        public void BroadcastModMessage<TMessage>(TMessage message, string messageType, string fromModID, string[] toModIDs, long[] toPlayerIDs)
        {
            // validate
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrWhiteSpace(messageType))
                throw new ArgumentNullException(nameof(messageType));
            if (string.IsNullOrWhiteSpace(fromModID))
                throw new ArgumentNullException(nameof(fromModID));
            if (!this.Peers.Any())
            {
                this.VerboseLog($"Ignored '{messageType}' broadcast from mod {fromModID}: not connected to any players.");
                return;
            }

            // filter player IDs
            HashSet<long> playerIDs = null;
            if (toPlayerIDs != null && toPlayerIDs.Any())
            {
                playerIDs = new HashSet<long>(toPlayerIDs);
                playerIDs.RemoveWhere(id => !this.Peers.ContainsKey(id));
                if (!playerIDs.Any())
                {
                    this.VerboseLog($"Ignored '{messageType}' broadcast from mod {fromModID}: none of the specified player IDs are connected.");
                    return;
                }
            }

            // get data to send
            ModMessageModel model = new ModMessageModel(
                fromPlayerID: Game1.player.UniqueMultiplayerID,
                fromModID: fromModID,
                toModIDs: toModIDs,
                toPlayerIDs: playerIDs?.ToArray(),
                type: messageType,
                data: JToken.FromObject(message)
            );
            string data = JsonConvert.SerializeObject(model, Formatting.None);

            // log message
            if (this.VerboseLogging)
                this.Monitor.Log($"Broadcasting '{messageType}' message: {data}.", LogLevel.Trace);

            // send message
            if (Context.IsMainPlayer)
            {
                foreach (MultiplayerPeer peer in this.Peers.Values)
                {
                    if (playerIDs == null || playerIDs.Contains(peer.PlayerID))
                    {
                        model.ToPlayerIDs = new[] { peer.PlayerID };
                        peer.SendMessage(new OutgoingMessage(SMultiplayer.ModMessageID, peer.PlayerID, data));
                    }
                }
            }
            else if (this.HostPeer != null && this.HostPeer.HasSmapi)
                this.HostPeer.SendMessage(new OutgoingMessage(SMultiplayer.ModMessageID, this.HostPeer.PlayerID, data));
            else
                this.VerboseLog("  Can't send message because no valid connections were found.");

        }


        /*********
        ** Private methods
        *********/
        /// <summary>Receive a mod message sent from another player's mods.</summary>
        /// <param name="message">The raw message to parse.</param>
        private void ReceiveModMessage(IncomingMessage message)
        {
            // parse message
            string json = message.Reader.ReadString();
            ModMessageModel model = this.JsonHelper.Deserialise<ModMessageModel>(json);
            HashSet<long> playerIDs = new HashSet<long>(model.ToPlayerIDs ?? this.GetKnownPlayerIDs());
            if (this.VerboseLogging)
                this.Monitor.Log($"Received message: {json}.");

            // notify local mods
            if (playerIDs.Contains(Game1.player.UniqueMultiplayerID))
                this.OnModMessageReceived(model);

            // forward to other players
            if (Context.IsMainPlayer && playerIDs.Any(p => p != Game1.player.UniqueMultiplayerID))
            {
                ModMessageModel newModel = new ModMessageModel(model);
                foreach (long playerID in playerIDs)
                {
                    if (playerID != Game1.player.UniqueMultiplayerID && playerID != model.FromPlayerID && this.Peers.TryGetValue(playerID, out MultiplayerPeer peer))
                    {
                        newModel.ToPlayerIDs = new[] { peer.PlayerID };
                        this.VerboseLog($"  Forwarding message to player {peer.PlayerID}.");
                        peer.SendMessage(new OutgoingMessage(SMultiplayer.ModMessageID, peer.PlayerID, this.JsonHelper.Serialise(newModel, Formatting.None)));
                    }
                }
            }
        }

        /// <summary>Get all connected player IDs, including the current player.</summary>
        private IEnumerable<long> GetKnownPlayerIDs()
        {
            yield return Game1.player.UniqueMultiplayerID;
            foreach (long peerID in this.Peers.Keys)
                yield return peerID;
        }

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
                IsHost = peer.IsHost,
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

        /// <summary>Log a trace message if <see cref="VerboseLogging"/> is enabled.</summary>
        /// <param name="message">The message to log.</param>
        private void VerboseLog(string message)
        {
            if (this.VerboseLogging)
                this.Monitor.Log(message, LogLevel.Trace);
        }
    }
}
