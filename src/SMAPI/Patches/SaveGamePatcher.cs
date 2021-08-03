using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI.Internal.Patching;
using StardewValley;
using StardewValley.Menus;

namespace StardewModdingAPI.Patches
{
    /// <summary>Harmony patches for <see cref="SaveGame"/> which track the last loaded save ID.</summary>
    /// <remarks>Patch methods must be static for Harmony to work correctly. See the Harmony documentation before renaming patch arguments.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    internal class SaveGamePatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>A callback to invoke when a save file is being loaded.</summary>
        private static Action<string> OnSaveFileReading;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="onSaveFileReading">A callback to invoke when a save file is being loaded.</param>
        public SaveGamePatcher(Action<string> onSaveFileReading)
        {
            SaveGamePatcher.OnSaveFileReading = onSaveFileReading;
        }

        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<SaveGame>(nameof(SaveGame.getLoadEnumerator)),
                prefix: this.GetHarmonyMethod(nameof(SaveGamePatcher.Before_GetLoadEnumerator))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="TitleMenu.createdNewCharacter"/>.</summary>
        /// <returns>Returns whether to execute the original method.</returns>
        /// <remarks>This method must be static for Harmony to work correctly. See the Harmony documentation before renaming arguments.</remarks>
        private static bool Before_GetLoadEnumerator(string file)
        {
            SaveGamePatcher.OnSaveFileReading(file);
            return true;
        }
    }
}
