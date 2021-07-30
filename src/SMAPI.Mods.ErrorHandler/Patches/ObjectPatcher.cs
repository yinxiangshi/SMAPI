using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI.Framework.Patching;
using StardewValley;
using SObject = StardewValley.Object;

namespace StardewModdingAPI.Mods.ErrorHandler.Patches
{
    /// <summary>A Harmony patch for <see cref="SObject.getDescription"/> which intercepts crashes due to the item no longer existing.</summary>
    /// <remarks>Patch methods must be static for Harmony to work correctly. See the Harmony documentation before renaming patch arguments.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    internal class ObjectPatcher : IHarmonyPatch
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public void Apply(Harmony harmony)
        {
            // object.getDescription
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.getDescription)),
                prefix: new HarmonyMethod(this.GetType(), nameof(ObjectPatcher.Before_Object_GetDescription))
            );

            // object.getDisplayName
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), "loadDisplayName"),
                finalizer: new HarmonyMethod(this.GetType(), nameof(ObjectPatcher.Finalize_Object_loadDisplayName))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call instead of <see cref="StardewValley.Object.getDescription"/>.</summary>
        /// <param name="__instance">The instance being patched.</param>
        /// <param name="__result">The patched method's return value.</param>
        /// <returns>Returns whether to execute the original method.</returns>
        private static bool Before_Object_GetDescription(SObject __instance, ref string __result)
        {
            // invalid bigcraftables crash instead of showing '???' like invalid non-bigcraftables
            if (!__instance.IsRecipe && __instance.bigCraftable.Value && !Game1.bigCraftablesInformation.ContainsKey(__instance.ParentSheetIndex))
            {
                __result = "???";
                return false;
            }

            return true;
        }

        /// <summary>The method to call after <see cref="StardewValley.Object.loadDisplayName"/>.</summary>
        /// <param name="__result">The patched method's return value.</param>
        /// <param name="__exception">The exception thrown by the wrapped method, if any.</param>
        /// <returns>Returns the exception to throw, if any.</returns>
        private static Exception Finalize_Object_loadDisplayName(ref string __result, Exception __exception)
        {
            if (__exception is KeyNotFoundException)
            {
                __result = "???";
                return null;
            }

            return __exception;
        }
    }
}
