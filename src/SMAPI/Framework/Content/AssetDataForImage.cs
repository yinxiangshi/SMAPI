using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewModdingAPI.Framework.Content
{
    /// <summary>Encapsulates access and changes to image content being read from a data file.</summary>
    internal class AssetDataForImage : AssetData<Texture2D>, IAssetDataForImage
    {
        /*********
        ** Fields
        *********/
        /// <summary>The minimum value to consider non-transparent.</summary>
        /// <remarks>On Linux/macOS, fully transparent pixels may have an alpha up to 4 for some reason.</remarks>
        private const byte MinOpacity = 5;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="locale">The content's locale code, if the content is localized.</param>
        /// <param name="assetName">The asset name being read.</param>
        /// <param name="data">The content data being read.</param>
        /// <param name="getNormalizedPath">Normalizes an asset key to match the cache key.</param>
        /// <param name="onDataReplaced">A callback to invoke when the data is replaced (if any).</param>
        public AssetDataForImage(string locale, IAssetName assetName, Texture2D data, Func<string, string> getNormalizedPath, Action<Texture2D> onDataReplaced)
            : base(locale, assetName, data, getNormalizedPath, onDataReplaced) { }

        /// <inheritdoc />
        public void PatchImage(Texture2D source, Rectangle? sourceArea = null, Rectangle? targetArea = null, PatchMode patchMode = PatchMode.Replace)
        {
            // get texture
            if (source == null)
                throw new ArgumentNullException(nameof(source), "Can't patch from a null source texture.");
            Texture2D target = this.Data;

            // get areas
            sourceArea ??= new Rectangle(0, 0, source.Width, source.Height);
            targetArea ??= new Rectangle(0, 0, Math.Min(sourceArea.Value.Width, target.Width), Math.Min(sourceArea.Value.Height, target.Height));

            // validate
            if (!source.Bounds.Contains(sourceArea.Value))
                throw new ArgumentOutOfRangeException(nameof(sourceArea), "The source area is outside the bounds of the source texture.");
            if (!target.Bounds.Contains(targetArea.Value))
                throw new ArgumentOutOfRangeException(nameof(targetArea), "The target area is outside the bounds of the target texture.");
            if (sourceArea.Value.Size != targetArea.Value.Size)
                throw new InvalidOperationException("The source and target areas must be the same size.");

            // get source data
            int pixelCount = sourceArea.Value.Width * sourceArea.Value.Height;
            Color[] sourceData = GC.AllocateUninitializedArray<Color>(pixelCount);
            source.GetData(0, sourceArea, sourceData, 0, pixelCount);

            // merge data in overlay mode
            if (patchMode == PatchMode.Overlay)
            {
                // get target data
                Color[] targetData = GC.AllocateUninitializedArray<Color>(pixelCount);
                target.GetData(0, targetArea, targetData, 0, pixelCount);

                // merge pixels
                for (int i = 0; i < sourceData.Length; i++)
                {
                    Color above = sourceData[i];
                    Color below = targetData[i];

                    // shortcut transparency
                    if (above.A < MinOpacity)
                    {
                        sourceData[i] = below;
                        continue;
                    }
                    if (below.A < MinOpacity)
                    {
                        sourceData[i] = above;
                        continue;
                    }

                    // merge pixels
                    // This performs a conventional alpha blend for the pixels, which are already
                    // premultiplied by the content pipeline. The formula is derived from
                    // https://blogs.msdn.microsoft.com/shawnhar/2009/11/06/premultiplied-alpha/.
                    // Note: don't use named arguments here since they're different between
                    // Linux/macOS and Windows.
                    float alphaBelow = 1 - (above.A / 255f);
                    sourceData[i] = new Color(
                        (int)(above.R + (below.R * alphaBelow)), // r
                        (int)(above.G + (below.G * alphaBelow)), // g
                        (int)(above.B + (below.B * alphaBelow)), // b
                        Math.Max(above.A, below.A) // a
                    );
                }
            }

            // patch target texture
            target.SetData(0, targetArea, sourceData, 0, pixelCount);
        }

        /// <inheritdoc />
        public bool ExtendImage(int minWidth, int minHeight)
        {
            if (this.Data.Width >= minWidth && this.Data.Height >= minHeight)
                return false;

            Texture2D original = this.Data;
            Texture2D texture = new(Game1.graphics.GraphicsDevice, Math.Max(original.Width, minWidth), Math.Max(original.Height, minHeight));
            this.ReplaceWith(texture);
            this.PatchImage(original);
            return true;
        }
    }
}
