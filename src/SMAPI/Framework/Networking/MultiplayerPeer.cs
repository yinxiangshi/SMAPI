using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using StardewValley.Network;

namespace StardewModdingAPI.Framework.Networking
{
    /// <summary>Metadata about a connected player.</summary>
    internal class MultiplayerPeer : IMultiplayerPeer
    {
        /*********
        ** Properties
        *********/
        /// <summary>The server through which to send messages, if this is an incoming farmhand.</summary>
        private readonly SLidgrenServer Server;

        /// <summary>The client through which to send messages, if this is the host player.</summary>
        private readonly SLidgrenClient Client;

        /// <summary>The network connection to the player.</summary>
        private readonly NetConnection ServerConnection;


        /*********
        ** Accessors
        *********/
        /// <summary>The player's unique ID.</summary>
        public long PlayerID { get; }

        /// <summary>Whether this is a connection to the host player.</summary>
        public bool IsHost { get; }

        /// <summary>Whether the player has SMAPI installed.</summary>
        public bool HasSmapi => this.ApiVersion != null;

        /// <summary>The player's OS platform, if <see cref="HasSmapi"/> is true.</summary>
        public GamePlatform? Platform { get; }

        /// <summary>The installed version of Stardew Valley, if <see cref="HasSmapi"/> is true.</summary>
        public ISemanticVersion GameVersion { get; }

        /// <summary>The installed version of SMAPI, if <see cref="HasSmapi"/> is true.</summary>
        public ISemanticVersion ApiVersion { get; }

        /// <summary>The installed mods, if <see cref="HasSmapi"/> is true.</summary>
        public IEnumerable<IMultiplayerPeerMod> Mods { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="playerID">The player's unique ID.</param>
        /// <param name="model">The metadata to copy.</param>
        /// <param name="server">The server through which to send messages.</param>
        /// <param name="serverConnection">The server connection through which to send messages.</param>
        /// <param name="client">The client through which to send messages.</param>
        /// <param name="isHost">Whether this is a connection to the host player.</param>
        public MultiplayerPeer(long playerID, RemoteContextModel model, SLidgrenServer server, NetConnection serverConnection, SLidgrenClient client, bool isHost)
        {
            this.PlayerID = playerID;
            this.IsHost = isHost;
            if (model != null)
            {
                this.Platform = model.Platform;
                this.GameVersion = model.GameVersion;
                this.ApiVersion = model.ApiVersion;
                this.Mods = model.Mods.Select(mod => new MultiplayerPeerMod(mod)).ToArray();
            }
            this.Server = server;
            this.ServerConnection = serverConnection;
            this.Client = client;
        }

        /// <summary>Construct an instance for a connection to an incoming farmhand.</summary>
        /// <param name="playerID">The player's unique ID.</param>
        /// <param name="model">The metadata to copy, if available.</param>
        /// <param name="server">The server through which to send messages.</param>
        /// <param name="serverConnection">The server connection through which to send messages.</param>
        public static MultiplayerPeer ForConnectionToFarmhand(long playerID, RemoteContextModel model, SLidgrenServer server, NetConnection serverConnection)
        {
            return new MultiplayerPeer(
                playerID: playerID,
                model: model,
                server: server,
                serverConnection: serverConnection,
                client: null,
                isHost: false
            );
        }

        /// <summary>Construct an instance for a connection to the host player.</summary>
        /// <param name="playerID">The player's unique ID.</param>
        /// <param name="model">The metadata to copy.</param>
        /// <param name="client">The client through which to send messages.</param>
        /// <param name="isHost">Whether this connection is for the host player.</param>
        public static MultiplayerPeer ForConnectionToHost(long playerID, RemoteContextModel model, SLidgrenClient client, bool isHost)
        {
            return new MultiplayerPeer(
                playerID: playerID,
                model: model,
                server: null,
                serverConnection: null,
                client: client,
                isHost: isHost
            );
        }

        /// <summary>Get metadata for a mod installed by the player.</summary>
        /// <param name="id">The unique mod ID.</param>
        /// <returns>Returns the mod info, or <c>null</c> if the player doesn't have that mod.</returns>
        public IMultiplayerPeerMod GetMod(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || this.Mods == null || !this.Mods.Any())
                return null;

            id = id.Trim();
            return this.Mods.FirstOrDefault(mod => mod.ID != null && mod.ID.Equals(id, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>Send a message to the given peer, bypassing the game's normal validation to allow messages before the connection is approved.</summary>
        /// <param name="message">The message to send.</param>
        public void SendMessage(OutgoingMessage message)
        {
            if (this.IsHost)
                this.Client.sendMessage(message);
            else
                this.Server.SendMessage(this.ServerConnection, message);
        }
    }
}
