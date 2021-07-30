using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Framework.Patching;
using StardewValley.Menus;

namespace StardewModdingAPI.Patches
{
    /// <summary>Harmony patches which notify SMAPI for save creation load stages.</summary>
    /// <remarks>Patch methods must be static for Harmony to work correctly. See the Harmony documentation before renaming patch arguments.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    internal class TitleMenuPatcher : IHarmonyPatch
    {
        /*********
        ** Fields
        *********/
        /// <summary>A callback to invoke when the load stage changes.</summary>
        private static Action<LoadStage> OnStageChanged;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="onStageChanged">A callback to invoke when the load stage changes.</param>
        public TitleMenuPatcher(Action<LoadStage> onStageChanged)
        {
            TitleMenuPatcher.OnStageChanged = onStageChanged;
        }

        /// <inheritdoc />
        public void Apply(Harmony harmony)
        {
            // detect CreatedBasicInfo
            harmony.Patch(
                original: AccessTools.Method(typeof(TitleMenu), nameof(TitleMenu.createdNewCharacter)),
                prefix: new HarmonyMethod(this.GetType(), nameof(TitleMenuPatcher.Before_TitleMenu_CreatedNewCharacter))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Called before <see cref="TitleMenu.createdNewCharacter"/>.</summary>
        /// <returns>Returns whether to execute the original method.</returns>
        /// <remarks>This method must be static for Harmony to work correctly. See the Harmony documentation before renaming arguments.</remarks>
        private static bool Before_TitleMenu_CreatedNewCharacter()
        {
            TitleMenuPatcher.OnStageChanged(LoadStage.CreatedBasicInfo);
            return true;
        }
    }
}
