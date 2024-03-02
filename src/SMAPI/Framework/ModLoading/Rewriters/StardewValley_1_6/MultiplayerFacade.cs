using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member: This is internal code to support rewriters and shouldn't be called directly.

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6
{
    /// <summary>Maps Stardew Valley 1.5.6's <see cref="Multiplayer"/> methods to their newer form to avoid breaking older mods.</summary>
    /// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = SuppressReasons.MatchesOriginal)]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaRewriting)]
    public class MultiplayerFacade : Multiplayer, IRewriteFacade
    {
        /*********
        ** Public methods
        *********/
        public void broadcastSprites(GameLocation location, List<TemporaryAnimatedSprite> sprites)
        {
            var list = new TemporaryAnimatedSpriteList();
            list.AddRange(sprites);

            base.broadcastSprites(location, list);
        }

        public void broadcastGlobalMessage(string localization_string_key, bool only_show_if_empty = false, params string[] substitutions)
        {
            base.broadcastGlobalMessage(localization_string_key, only_show_if_empty, null, substitutions);
        }


        /*********
        ** Private methods
        *********/
        private MultiplayerFacade()
        {
            RewriteHelper.ThrowFakeConstructorCalled();
        }
    }
}
