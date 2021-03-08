using System;
using System.Diagnostics.CodeAnalysis;
#if HARMONY_2
using HarmonyLib;
#else
using Harmony;
#endif
using StardewModdingAPI.Enums;
using StardewModdingAPI.Framework.Patching;
using StardewModdingAPI.Framework.Reflection;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;

namespace StardewModdingAPI.Patches
{
    /// <summary>Harmony patches which notify SMAPI for save creation load stages.</summary>
    /// <remarks>Patch methods must be static for Harmony to work correctly. See the Harmony documentation before renaming patch arguments.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    internal class LoadContextPatch : IHarmonyPatch
    {
        /*********
        ** Fields
        *********/
        /// <summary>Simplifies access to private code.</summary>
        private static Reflector Reflection;

        /// <summary>A callback to invoke when the load stage changes.</summary>
        private static Action<LoadStage> OnStageChanged;

        /// <summary>Whether the game is running running the code in <see cref="Game1.loadForNewGame"/>.</summary>
        private static bool IsInLoadForNewGame;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="onStageChanged">A callback to invoke when the load stage changes.</param>
        public LoadContextPatch(Reflector reflection, Action<LoadStage> onStageChanged)
        {
            LoadContextPatch.Reflection = reflection;
            LoadContextPatch.OnStageChanged = onStageChanged;
        }

        /// <inheritdoc />
#if HARMONY_2
        public void Apply(Harmony harmony)
#else
        public void Apply(HarmonyInstance harmony)
#endif
        {
            // detect CreatedBasicInfo
            harmony.Patch(
                original: AccessTools.Method(typeof(TitleMenu), nameof(TitleMenu.createdNewCharacter)),
                prefix: new HarmonyMethod(this.GetType(), nameof(LoadContextPatch.Before_TitleMenu_CreatedNewCharacter))
            );

            // detect CreatedInitialLocations and SaveAddedLocations
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.AddModNPCs)),
                prefix: new HarmonyMethod(this.GetType(), nameof(LoadContextPatch.Before_Game1_AddModNPCs))
            );

            // detect CreatedLocations, and track IsInLoadForNewGame
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.loadForNewGame)),
                prefix: new HarmonyMethod(this.GetType(), nameof(LoadContextPatch.Before_Game1_LoadForNewGame)),
                postfix: new HarmonyMethod(this.GetType(), nameof(LoadContextPatch.After_Game1_LoadForNewGame))
            );

            // detect ReturningToTitle
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.CleanupReturningToTitle)),
                prefix: new HarmonyMethod(this.GetType(), nameof(LoadContextPatch.Before_Game1_CleanupReturningToTitle))
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
            LoadContextPatch.OnStageChanged(LoadStage.CreatedBasicInfo);
            return true;
        }

        /// <summary>Called before <see cref="Game1.AddModNPCs"/>.</summary>
        /// <returns>Returns whether to execute the original method.</returns>
        /// <remarks>This method must be static for Harmony to work correctly. See the Harmony documentation before renaming arguments.</remarks>
        private static bool Before_Game1_AddModNPCs()
        {
            // When this method is called from Game1.loadForNewGame, it happens right after adding the vanilla
            // locations but before initializing them.
            if (LoadContextPatch.IsInLoadForNewGame)
            {
                LoadContextPatch.OnStageChanged(LoadContextPatch.IsCreating()
                    ? LoadStage.CreatedInitialLocations
                    : LoadStage.SaveAddedLocations
                );
            }

            return true;
        }

        /// <summary>Called before <see cref="Game1.CleanupReturningToTitle"/>.</summary>
        /// <returns>Returns whether to execute the original method.</returns>
        /// <remarks>This method must be static for Harmony to work correctly. See the Harmony documentation before renaming arguments.</remarks>
        private static bool Before_Game1_CleanupReturningToTitle()
        {
            LoadContextPatch.OnStageChanged(LoadStage.ReturningToTitle);
            return true;
        }

        /// <summary>Called before <see cref="Game1.loadForNewGame"/>.</summary>
        /// <returns>Returns whether to execute the original method.</returns>
        /// <remarks>This method must be static for Harmony to work correctly. See the Harmony documentation before renaming arguments.</remarks>
        private static bool Before_Game1_LoadForNewGame()
        {
            LoadContextPatch.IsInLoadForNewGame = true;
            return true;
        }

        /// <summary>Called after <see cref="Game1.loadForNewGame"/>.</summary>
        /// <remarks>This method must be static for Harmony to work correctly. See the Harmony documentation before renaming arguments.</remarks>
        private static void After_Game1_LoadForNewGame()
        {
            LoadContextPatch.IsInLoadForNewGame = false;

            if (LoadContextPatch.IsCreating())
                LoadContextPatch.OnStageChanged(LoadStage.CreatedLocations);
        }

        /// <summary>Get whether the save file is currently being created.</summary>
        private static bool IsCreating()
        {
            return
                (Game1.currentMinigame is Intro) // creating save with intro
                || (Game1.activeClickableMenu is TitleMenu menu && LoadContextPatch.Reflection.GetField<bool>(menu, "transitioningCharacterCreationMenu").GetValue()); // creating save, skipped intro
        }
    }
}
