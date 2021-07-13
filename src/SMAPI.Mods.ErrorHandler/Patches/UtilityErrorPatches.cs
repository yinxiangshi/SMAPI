using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI.Framework.Patching;
using StardewValley;

namespace StardewModdingAPI.Mods.ErrorHandler.Patches
{
    /// <summary>A Harmony patch for <see cref="Utility"/> methods to log more detailed errors.</summary>
    /// <remarks>Patch methods must be static for Harmony to work correctly. See the Harmony documentation before renaming patch arguments.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    internal class UtilityErrorPatches : IHarmonyPatch
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getItemFromStandardTextDescription)),
                finalizer: new HarmonyMethod(this.GetType(), nameof(UtilityErrorPatches.Finalize_Utility_GetItemFromStandardTextDescription))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call instead of <see cref="Utility.getItemFromStandardTextDescription"/>.</summary>
        /// <param name="description">The item text description to parse.</param>
        /// <param name="delimiter">The delimiter by which to split the text description.</param>
        /// <param name="__exception">The exception thrown by the wrapped method, if any.</param>
        /// <returns>Returns the exception to throw, if any.</returns>
        private static Exception Finalize_Utility_GetItemFromStandardTextDescription(string description, char delimiter, ref Exception __exception)
        {
            return __exception != null
                ? new FormatException($"Failed to parse item text description \"{description}\" with delimiter \"{delimiter}\".", __exception)
                : null;
        }
    }
}
