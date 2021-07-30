using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI.Framework.Patching;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

namespace StardewModdingAPI.Mods.ErrorHandler.Patches
{
    /// <summary>A Harmony patch for <see cref="IClickableMenu"/> which intercepts crashes due to invalid items.</summary>
    /// <remarks>Patch methods must be static for Harmony to work correctly. See the Harmony documentation before renaming patch arguments.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    internal class IClickablePatcher : IHarmonyPatch
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawToolTip)),
                prefix: new HarmonyMethod(this.GetType(), nameof(IClickablePatcher.Before_IClickableMenu_DrawTooltip))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call instead of <see cref="IClickableMenu.drawToolTip"/>.</summary>
        /// <param name="hoveredItem">The item for which to draw a tooltip.</param>
        /// <returns>Returns whether to execute the original method.</returns>
        private static bool Before_IClickableMenu_DrawTooltip(Item hoveredItem)
        {
            // invalid edible item cause crash when drawing tooltips
            if (hoveredItem is SObject obj && obj.Edibility != -300 && !Game1.objectInformation.ContainsKey(obj.ParentSheetIndex))
                return false;

            return true;
        }
    }
}
