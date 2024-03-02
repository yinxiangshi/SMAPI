using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member: This is internal code to support rewriters and shouldn't be called directly.

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6
{
    /// <summary>Maps Stardew Valley 1.5.6's <see cref="CraftingRecipe"/> methods to their newer form to avoid breaking older mods.</summary>
    /// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = SuppressReasons.MatchesOriginal)]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaRewriting)]
    public class CraftingRecipeFacade : CraftingRecipe, IRewriteFacade
    {
        /*********
        ** Public methods
        *********/
        public string getNameFromIndex(int index)
        {
            return base.getNameFromIndex(index.ToString());
        }

        public int getSpriteIndexFromRawIndex(int index)
        {
            string itemId = base.getSpriteIndexFromRawIndex(index.ToString());
            ParsedItemData? data = ItemRegistry.GetData(itemId);

            return data.HasTypeObject()
                ? data.SpriteIndex
                : index;
        }

        public static bool isThereSpecialIngredientRule(Object potentialIngredient, int requiredIngredient)
        {
            return CraftingRecipe.isThereSpecialIngredientRule(potentialIngredient, requiredIngredient.ToString());
        }


        /*********
        ** Private methods
        *********/
        private CraftingRecipeFacade()
            : base(null)
        {
            RewriteHelper.ThrowFakeConstructorCalled();
        }
    }
}
