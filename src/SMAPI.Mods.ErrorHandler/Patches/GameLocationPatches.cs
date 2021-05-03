using System.Diagnostics.CodeAnalysis;
#if HARMONY_2
using System;
using HarmonyLib;
#else
using System.Reflection;
using Harmony;
#endif
using StardewModdingAPI.Framework.Patching;
using StardewValley;
using xTile;

namespace StardewModdingAPI.Mods.ErrorHandler.Patches
{
    /// <summary>Harmony patches for <see cref="GameLocation.checkEventPrecondition"/> and <see cref="GameLocation.updateSeasonalTileSheets"/> which intercept errors instead of crashing.</summary>
    /// <remarks>Patch methods must be static for Harmony to work correctly. See the Harmony documentation before renaming patch arguments.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    internal class GameLocationPatches : IHarmonyPatch
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
        public GameLocationPatches(IMonitor monitorForGame)
        {
            GameLocationPatches.MonitorForGame = monitorForGame;
        }

        /// <inheritdoc />
#if HARMONY_2
        public void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkEventPrecondition)),
                finalizer: new HarmonyMethod(this.GetType(), nameof(EventErrorPatch.Finalize_GameLocation_CheckEventPrecondition))
            );
harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.updateSeasonalTileSheets)),
                finalizer: new HarmonyMethod(this.GetType(), nameof(EventErrorPatch.Before_GameLocation_UpdateSeasonalTileSheets))
            );
        }
#else
        public void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkEventPrecondition)),
                prefix: new HarmonyMethod(this.GetType(), nameof(GameLocationPatches.Before_GameLocation_CheckEventPrecondition))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.updateSeasonalTileSheets)),
                prefix: new HarmonyMethod(this.GetType(), nameof(GameLocationPatches.Before_GameLocation_UpdateSeasonalTileSheets))
            );
        }
#endif


        /*********
        ** Private methods
        *********/
#if HARMONY_2
        /// <summary>The method to call instead of GameLocation.checkEventPrecondition.</summary>
        /// <param name="__result">The return value of the original method.</param>
        /// <param name="precondition">The precondition to be parsed.</param>
        /// <param name="__exception">The exception thrown by the wrapped method, if any.</param>
        /// <returns>Returns the exception to throw, if any.</returns>
        private static Exception Finalize_GameLocation_CheckEventPrecondition(ref int __result, string precondition, Exception __exception)
        {
            if (__exception != null)
            {
                __result = -1;
                EventErrorPatch.MonitorForGame.Log($"Failed parsing event precondition ({precondition}):\n{__exception.InnerException}", LogLevel.Error);
            }

            return null;
        }
#else
        /// <summary>The method to call instead of <see cref="GameLocation.checkEventPrecondition"/>.</summary>
        /// <param name="__instance">The instance being patched.</param>
        /// <param name="__result">The return value of the original method.</param>
        /// <param name="precondition">The precondition to be parsed.</param>
        /// <param name="__originalMethod">The method being wrapped.</param>
        /// <returns>Returns whether to execute the original method.</returns>
        private static bool Before_GameLocation_CheckEventPrecondition(GameLocation __instance, ref int __result, string precondition, MethodInfo __originalMethod)
        {
            const string key = nameof(GameLocationPatches.Before_GameLocation_CheckEventPrecondition);
            if (!PatchHelper.StartIntercept(key))
                return true;

            try
            {
                __result = (int)__originalMethod.Invoke(__instance, new object[] { precondition });
                return false;
            }
            catch (TargetInvocationException ex)
            {
                __result = -1;
                GameLocationPatches.MonitorForGame.Log($"Failed parsing event precondition ({precondition}):\n{ex.InnerException}", LogLevel.Error);
                return false;
            }
            finally
            {
                PatchHelper.StopIntercept(key);
            }
        }
#endif

#if HARMONY_2
        /// <summary>The method to call instead of <see cref="GameLocation.updateSeasonalTileSheets"/>.</summary>
        /// <param name="__instance">The instance being patched.</param>
        /// <param name="map">The map whose tilesheets to update.</param>
        /// <param name="__exception">The exception thrown by the wrapped method, if any.</param>
        /// <returns>Returns the exception to throw, if any.</returns>
        private static Exception Before_GameLocation_UpdateSeasonalTileSheets(GameLocation __instance, Map map, Exception __exception)
        {
            if (__exception != null)
                GameLocationPatches.MonitorForGame.Log($"Failed updating seasonal tilesheets for location '{__instance.NameOrUniqueName}': \n{__exception}", LogLevel.Error);

            return null;
        }
#else
        /// <summary>The method to call instead of <see cref="GameLocation.updateSeasonalTileSheets"/>.</summary>
        /// <param name="__instance">The instance being patched.</param>
        /// <param name="map">The map whose tilesheets to update.</param>
        /// <param name="__originalMethod">The method being wrapped.</param>
        /// <returns>Returns whether to execute the original method.</returns>
        private static bool Before_GameLocation_UpdateSeasonalTileSheets(GameLocation __instance, Map map, MethodInfo __originalMethod)
        {
            const string key = nameof(GameLocationPatches.Before_GameLocation_UpdateSeasonalTileSheets);
            if (!PatchHelper.StartIntercept(key))
                return true;

            try
            {
                __originalMethod.Invoke(__instance, new object[] { map });
                return false;
            }
            catch (TargetInvocationException ex)
            {
                GameLocationPatches.MonitorForGame.Log($"Failed updating seasonal tilesheets for location '{__instance.NameOrUniqueName}'. Technical details:\n{ex.InnerException}", LogLevel.Error);
                return false;
            }
            finally
            {
                PatchHelper.StopIntercept(key);
            }
        }
#endif
    }
}
