using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley;
using StardewValley.Minigames;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member: This is internal code to support rewriters and shouldn't be called directly.

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6
{
    /// <summary>Maps Stardew Valley 1.5.6's <see cref="AbigailGame"/> methods to their newer form to avoid breaking older mods.</summary>
    /// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaRewriting)]
    public class AbigailGameFacade : AbigailGame, IRewriteFacade
    {
        /*********
        ** Public methods
        *********/
        public static AbigailGame Constructor(bool playingWithAbby = false)
        {
            return new AbigailGame(
                playingWithAbby
                    ? Game1.getCharacterFromName("Abigail")
                    : null
            );
        }


        /*********
        ** Private methods
        *********/
        private AbigailGameFacade()
        {
            RewriteHelper.ThrowFakeConstructorCalled();
        }
    }
}
