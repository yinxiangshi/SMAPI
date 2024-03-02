using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley.Tools;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member: This is internal code to support rewriters and shouldn't be called directly.

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6
{
    /// <summary>Maps Stardew Valley 1.5.6's <see cref="FishingRod"/> methods to their newer form to avoid breaking older mods.</summary>
    /// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = SuppressReasons.MatchesOriginal)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = SuppressReasons.MatchesOriginal)]
    [SuppressMessage("ReSharper", "ParameterHidesMember", Justification = SuppressReasons.MatchesOriginal)]
    [SuppressMessage("ReSharper", "RedundantBaseQualifier", Justification = SuppressReasons.BaseForClarity)]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaRewriting)]
    public class FishingRodFacade : FishingRod, IRewriteFacade
    {
        /*********
        ** Public methods
        *********/
        public int getBaitAttachmentIndex()
        {
            return int.TryParse(base.GetBait()?.ItemId, out int index)
                ? index
                : -1;
        }

        public int getBobberAttachmentIndex()
        {
            return int.TryParse(base.GetTackle()?.ItemId, out int index)
                ? index
                : -1;
        }

        public void pullFishFromWater(int whichFish, int fishSize, int fishQuality, int fishDifficulty, bool treasureCaught, bool wasPerfect, bool fromFishPond, bool caughtDouble = false, string itemCategory = "Object")
        {
            base.pullFishFromWater(whichFish.ToString(), fishSize, fishQuality, fishDifficulty, treasureCaught, wasPerfect, fromFishPond, null, false, caughtDouble);
        }


        /*********
        ** Private methods
        *********/
        private FishingRodFacade()
        {
            RewriteHelper.ThrowFakeConstructorCalled();
        }
    }
}
