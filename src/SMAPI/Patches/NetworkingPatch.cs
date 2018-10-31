using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Harmony;
using Lidgren.Network;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.Networking;
using StardewModdingAPI.Framework.Patching;
using StardewValley;
using StardewValley.Network;

namespace StardewModdingAPI.Patches
{
    /// <summary>A Harmony patch to enable the SMAPI multiplayer metadata handshake.</summary>
    internal class NetworkingPatch : IHarmonyPatch
    {
        /*********
        ** Properties
        *********/
        /// <summary>The constructor for the internal <c>NetBufferReadStream</c> type.</summary>
        private static readonly ConstructorInfo NetBufferReadStreamConstructor = NetworkingPatch.GetNetBufferReadStreamConstructor();


        /*********
        ** Accessors
        *********/
        /// <summary>A unique name for this patch.</summary>
        public string Name => $"{nameof(NetworkingPatch)}";


        /*********
        ** Public methods
        *********/
        /// <summary>Apply the Harmony patch.</summary>
        /// <param name="harmony">The Harmony instance.</param>
        public void Apply(HarmonyInstance harmony)
        {
            MethodInfo method = AccessTools.Method(typeof(LidgrenServer), "parseDataMessageFromClient");
            MethodInfo prefix = AccessTools.Method(this.GetType(), nameof(NetworkingPatch.Prefix_LidgrenServer_ParseDataMessageFromClient));
            harmony.Patch(method, new HarmonyMethod(prefix), null);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call instead of the <see cref="LidgrenServer.parseDataMessageFromClient"/> method.</summary>
        /// <param name="__instance">The instance being patched.</param>
        /// <param name="dataMsg">The raw network message to parse.</param>
        /// <param name="___peers">The private <c>peers</c> field on the <paramref name="__instance"/> instance.</param>
        /// <param name="___gameServer">The private <c>gameServer</c> field on the <paramref name="__instance"/> instance.</param>
        /// <returns>Returns whether to execute the original method.</returns>
        /// <remarks>This method must be static for Harmony to work correctly. See the Harmony documentation before renaming arguments.</remarks>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        private static bool Prefix_LidgrenServer_ParseDataMessageFromClient(LidgrenServer __instance, NetIncomingMessage dataMsg, Bimap<long, NetConnection> ___peers, IGameServer ___gameServer)
        {
            // get SMAPI overrides
            SMultiplayer multiplayer = ((SGame)Game1.game1).Multiplayer;
            SLidgrenServer server = (SLidgrenServer)__instance;

            // add hook to call multiplayer core
            NetConnection peer = dataMsg.SenderConnection;
            using (IncomingMessage message = new IncomingMessage())
            using (Stream readStream = (Stream)NetworkingPatch.NetBufferReadStreamConstructor.Invoke(new object[] { dataMsg }))
            using (BinaryReader reader = new BinaryReader(readStream))
            {
                while (dataMsg.LengthBits - dataMsg.Position >= 8)
                {
                    message.Read(reader);
                    if (___peers.ContainsLeft(message.FarmerID) && ___peers[message.FarmerID] == peer)
                        ___gameServer.processIncomingMessage(message);
                    else if (message.MessageType == Multiplayer.playerIntroduction)
                    {
                        NetFarmerRoot farmer = multiplayer.readFarmer(message.Reader);
                        ___gameServer.checkFarmhandRequest("", farmer, msg => server.SendMessage(peer, msg), () => ___peers[farmer.Value.UniqueMultiplayerID] = peer);
                    }
                    else
                        multiplayer.ProcessMessageFromUnknownFarmhand(__instance, dataMsg, message); // added hook
                }
            }

            return false;
        }

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
    }
}
