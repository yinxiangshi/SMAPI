using System;
using StardewValley;

namespace StardewModdingAPI.Events
{
    /// <summary>Events related to assets loaded from the content pipeline (including data, maps, and textures).</summary>
    public interface IContentEvents
    {
        /// <summary>Raised when an asset is being requested from the content pipeline.</summary>
        /// <remarks>
        /// The asset isn't necessarily being loaded yet (e.g. the game may be checking if it exists). Mods can register the changes they want to apply using methods on the <paramref name="e"/> parameter. These will be applied when the asset is actually loaded.
        ///
        /// If the asset is requested multiple times in the same tick (e.g. once to check if it exists and once to load it), SMAPI might only raise the event once and reuse the cached result.
        /// </remarks>
        event EventHandler<AssetRequestedEventArgs> AssetRequested;
    }
}
