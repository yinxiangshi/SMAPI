using System.Reflection;
using Lidgren.Network;
using StardewValley.Network;

namespace StardewModdingAPI.Framework.Networking
{
    /// <summary>A multiplayer server used to connect to an incoming player. This is an implementation of <see cref="LidgrenServer"/> that adds support for SMAPI's metadata context exchange.</summary>
    internal class SLidgrenServer : LidgrenServer
    {
        /*********
        ** Properties
        *********/
        /// <summary>A method which sends a message through a specific connection.</summary>
        private readonly MethodInfo SendMessageToConnectionMethod;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameServer">The underlying game server.</param>
        public SLidgrenServer(IGameServer gameServer)
            : base(gameServer)
        {
            this.SendMessageToConnectionMethod = typeof(LidgrenServer).GetMethod(nameof(LidgrenServer.sendMessage), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(NetConnection), typeof(OutgoingMessage) }, null);
        }

        /// <summary>Send a message to a remote server.</summary>
        /// <param name="connection">The network connection.</param>
        /// <param name="message">The message to send.</param>
        public void SendMessage(NetConnection connection, OutgoingMessage message)
        {
            this.SendMessageToConnectionMethod.Invoke(this, new object[] { connection, message });
        }
    }
}
