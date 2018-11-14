using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Galaxy.Api;
using Harmony;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.Patching;
using StardewValley.Network;

namespace StardewModdingAPI.Patches
{
    /// <summary>A Harmony patch to let SMAPI override <see cref="GalaxyNetServerPatch"/> methods.</summary>
    internal class GalaxyNetServerPatch : IHarmonyPatch
    {
        /*********
        ** Properties
        *********/
        /// <summary>SMAPI's implementation of the game's core multiplayer logic.</summary>
        private static Lazy<SMultiplayer> Multiplayer;

        /// <summary>The name of the internal GalaxyNetServer class.</summary>
        private static readonly string ServerTypeName = $"StardewValley.SDKs.GalaxyNetServer, {Constants.GameAssemblyName}";

        /// <summary>The method which sends an arbitrary message.</summary>
        private static MethodInfo SendMessageMethod;


        /*********
        ** Accessors
        *********/
        /// <summary>A unique name for this patch.</summary>
        public string Name => $"{nameof(GalaxyNetServerPatch)}";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplayer">SMAPI's implementation of the game's core multiplayer logic.</param>
        public GalaxyNetServerPatch(Func<SMultiplayer> multiplayer)
        {
            // init
            GalaxyNetServerPatch.Multiplayer = new Lazy<SMultiplayer>(multiplayer);

            // get server.sendMessage method
            Type type = Type.GetType(GalaxyNetServerPatch.ServerTypeName);
            if (type == null)
                throw new InvalidOperationException($"Can't find type '{GalaxyNetServerPatch.ServerTypeName}'.");
            GalaxyNetServerPatch.SendMessageMethod = type.GetMethod("sendMessage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(GalaxyID), typeof(OutgoingMessage) }, null);
            if (GalaxyNetServerPatch.SendMessageMethod == null)
                throw new InvalidOperationException($"Can't find method 'sendMessage' on '{GalaxyNetServerPatch.ServerTypeName}'.");
        }

        /// <summary>Apply the Harmony patch.</summary>
        /// <param name="harmony">The Harmony instance.</param>
        public void Apply(HarmonyInstance harmony)
        {
            // override parseDataMessageFromClient
            {
                MethodInfo method = AccessTools.Method(Type.GetType($"StardewValley.SDKs.GalaxyNetServer, {Constants.GameAssemblyName}"), "onReceiveMessage");
                MethodInfo prefix = AccessTools.Method(this.GetType(), nameof(GalaxyNetServerPatch.Prefix_GalaxyNetServer_OnReceiveMessage));
                harmony.Patch(method, new HarmonyMethod(prefix), null);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call instead of the <see cref="GalaxyNetServer.onReceiveMessage"/> method.</summary>
        /// <param name="__instance">The instance being patched.</param>
        /// <param name="peer">The Galaxy peer ID.</param>
        /// <param name="messageStream">The data to process.</param>
        /// <param name="___peers">The private <c>peers</c> field on the <paramref name="__instance"/> instance.</param>
        /// <param name="___gameServer">The private <c>gameServer</c> field on the <paramref name="__instance"/> instance.</param>
        /// <returns>Returns whether to execute the original method.</returns>
        /// <remarks>This method must be static for Harmony to work correctly. See the Harmony documentation before renaming arguments.</remarks>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        private static bool Prefix_GalaxyNetServer_OnReceiveMessage(Server __instance, GalaxyID peer, Stream messageStream, Bimap<long, ulong> ___peers, IGameServer ___gameServer)
        {
            SMultiplayer multiplayer = GalaxyNetServerPatch.Multiplayer.Value;

            using (IncomingMessage message = new IncomingMessage())
            using (BinaryReader reader = new BinaryReader(messageStream))
            {
                message.Read(reader);
                multiplayer.OnServerProcessingMessage(message, outgoing => GalaxyNetServerPatch.SendMessageMethod.Invoke(__instance, new object[] { peer, outgoing }), () =>
                {
                    if (___peers.ContainsLeft(message.FarmerID) && (long)___peers[message.FarmerID] == (long)peer.ToUint64())
                    {
                        ___gameServer.processIncomingMessage(message);
                    }
                    else if (message.MessageType == StardewValley.Multiplayer.playerIntroduction)
                    {
                        NetFarmerRoot farmer = multiplayer.readFarmer(message.Reader);
                        GalaxyID capturedPeer = new GalaxyID(peer.ToUint64());
                        ___gameServer.checkFarmhandRequest(Convert.ToString(peer.ToUint64()), farmer, msg => GalaxyNetServerPatch.SendMessageMethod.Invoke(__instance, new object[] { capturedPeer, msg }), () => ___peers[farmer.Value.UniqueMultiplayerID] = capturedPeer.ToUint64());
                    }
                });
            }

            return false;
        }
    }
}
