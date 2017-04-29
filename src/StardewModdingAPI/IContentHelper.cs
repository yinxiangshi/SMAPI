using Microsoft.Xna.Framework.Graphics;

namespace StardewModdingAPI
{
    /// <summary>Provides an API for loading content assets.</summary>
    public interface IContentHelper
    {
        /// <summary>Fetch and cache content from the game content or mod folder (if not already cached), and return it.</summary>
        /// <typeparam name="T">The expected data type. The main supported types are <see cref="Texture2D"/> and dictionaries; other types may be supported by the game's content pipeline.</typeparam>
        /// <param name="key">The asset key to fetch (if the <paramref name="source"/> is <see cref="ContentSource.GameContent"/>), or the local path to an XNB file relative to the mod folder.</param>
        /// <param name="source">Where to search for a matching content asset.</param>
        T Load<T>(string key, ContentSource source);
    }
}
