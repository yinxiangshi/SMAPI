using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI.Framework.Patching;
using StardewValley;

namespace StardewModdingAPI.Mods.ErrorHandler.Patches
{
    /// <summary>Harmony patches for <see cref="Event"/> which intercept errors to log more details.</summary>
    /// <remarks>Patch methods must be static for Harmony to work correctly. See the Harmony documentation before renaming patch arguments.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    internal class EventPatches : IHarmonyPatch
    {
        /*********
        ** Fields
        *********/
        /// <summary>Writes messages to the console and log file on behalf of the game.</summary>
        private static IMonitor MonitorForGame;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitorForGame">Writes messages to the console and log file on behalf of the game.</param>
        public EventPatches(IMonitor monitorForGame)
        {
            EventPatches.MonitorForGame = monitorForGame;
        }

        /// <inheritdoc />
        public void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.LogErrorAndHalt)),
                postfix: new HarmonyMethod(this.GetType(), nameof(EventPatches.After_Event_LogErrorAndHalt))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after <see cref="Event.LogErrorAndHalt"/>.</summary>
        /// <param name="e">The exception being logged.</param>
        private static void After_Event_LogErrorAndHalt(Exception e)
        {
            EventPatches.MonitorForGame.Log(e.ToString(), LogLevel.Error);
        }
    }
}
