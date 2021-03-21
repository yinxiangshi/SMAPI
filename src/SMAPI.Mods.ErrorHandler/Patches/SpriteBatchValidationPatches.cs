#if HARMONY_2
using HarmonyLib;
#else
using Harmony;
#endif
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Framework.Patching;

namespace StardewModdingAPI.Mods.ErrorHandler.Patches
{
    /// <summary>Harmony patch for <see cref="SpriteBatch"/> to validate textures earlier.</summary>
    /// <remarks>Patch methods must be static for Harmony to work correctly. See the Harmony documentation before renaming patch arguments.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Argument names are defined by Harmony and methods are named for clarity.")]
    internal class SpriteBatchValidationPatches : IHarmonyPatch
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
#if HARMONY_2
        public void Apply(Harmony harmony)
#else
        public void Apply(HarmonyInstance harmony)
#endif
        {
            harmony.Patch(
                original: Constants.GameFramework == GameFramework.Xna
                    ? AccessTools.Method(typeof(SpriteBatch), "InternalDraw")
                    : AccessTools.Method(typeof(SpriteBatch), "CheckValid", new[] { typeof(Texture2D) }),
                postfix: new HarmonyMethod(this.GetType(), nameof(SpriteBatchValidationPatches.After_SpriteBatch_CheckValid))
            );
        }


        /*********
        ** Private methods
        *********/
#if SMAPI_FOR_XNA
        /// <summary>The method to call instead of <see cref="SpriteBatch.InternalDraw"/>.</summary>
        /// <param name="texture">The texture to validate.</param>
#else
        /// <summary>The method to call instead of <see cref="SpriteBatch.CheckValid"/>.</summary>
        /// <param name="texture">The texture to validate.</param>
#endif
        private static void After_SpriteBatch_CheckValid(Texture2D texture)
        {
            if (texture?.IsDisposed == true)
                throw new ObjectDisposedException("Cannot draw this texture because it's disposed.");
        }
    }
}
