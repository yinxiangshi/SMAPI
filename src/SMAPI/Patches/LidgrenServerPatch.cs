using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Harmony;
using Lidgren.Network;
using StardewModdingAPI.Framework.Networking;
using StardewModdingAPI.Framework.Patching;
using StardewValley.Network;

namespace StardewModdingAPI.Patches
{
    /// <summary>A Harmony patch to let SMAPI override <see cref="LidgrenServer"/> methods.</summary>
    internal class LidgrenServerPatch : IHarmonyPatch
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A unique name for this patch.</summary>
        public string Name => $"{nameof(LidgrenServerPatch)}";


        /*********
        ** Public methods
        *********/
        /// <summary>Apply the Harmony patch.</summary>
        /// <param name="harmony">The Harmony instance.</param>
        public void Apply(HarmonyInstance harmony)
        {
            // override parseDataMessageFromClient
            {
                MethodInfo method = AccessTools.Method(typeof(LidgrenServer), "parseDataMessageFromClient");
                MethodInfo prefix = AccessTools.Method(this.GetType(), nameof(LidgrenServerPatch.Prefix_LidgrenServer_ParseDataMessageFromClient));
                harmony.Patch(method, new HarmonyMethod(prefix), null);
            }
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
            if (__instance is SLidgrenServer smapiServer)
            {
                smapiServer.ParseDataMessageFromClient(dataMsg);
                return false;
            }

            return true;
        }
    }
}
