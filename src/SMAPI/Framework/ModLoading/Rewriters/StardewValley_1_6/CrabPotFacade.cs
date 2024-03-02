using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley.Objects;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member: This is internal code to support rewriters and shouldn't be called directly.

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6
{
    /// <summary>Maps Stardew Valley 1.5.6's <see cref="CrabPot"/> methods to their newer form to avoid breaking older mods.</summary>
    /// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaRewriting)]
    [SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = SuppressReasons.MatchesOriginal)]
    public class CrabPotFacade : CrabPot, IRewriteFacade
    {
        /*********
        ** Public methods
        *********/
        public static CrabPot Constructor(Vector2 tileLocation, int stack = 1)
        {
            return new CrabPot();
        }

        /*********
        ** Private methods
        *********/
        private CrabPotFacade()
        {
            RewriteHelper.ThrowFakeConstructorCalled();
        }
    }
}
