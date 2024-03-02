using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Framework.ModLoading.Framework;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member: This is internal code to support rewriters and shouldn't be called directly.

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6
{
    /// <summary>Maps Stardew Valley 1.5.6's <see cref="Layer"/> methods to their newer form to avoid breaking older mods.</summary>
    /// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = SuppressReasons.MatchesOriginal)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = SuppressReasons.MatchesOriginal)]
    [SuppressMessage("ReSharper", "RedundantBaseQualifier", Justification = SuppressReasons.BaseForClarity)]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaRewriting)]
    [SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = SuppressReasons.MatchesOriginal)]
    public class LayerFacade : Layer, IRewriteFacade
    {
        /*********
        ** Public methods
        *********/
        public void Draw(IDisplayDevice displayDevice, Rectangle mapViewport, Location displayOffset, bool wrapAround, int pixelZoom)
        {
            base.Draw(displayDevice, mapViewport, displayOffset, wrapAround, pixelZoom);
        }


        /*********
        ** Private methods
        *********/
        private LayerFacade()
            : base(null, null, Size.Zero, Size.Zero)
        {
            RewriteHelper.ThrowFakeConstructorCalled();
        }
    }
}
