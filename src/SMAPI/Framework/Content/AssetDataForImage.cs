using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        public AssetDataForImage(string? locale, IAssetName assetName, Texture2D data, Func<string, string> getNormalizedPath, Action<Texture2D> onDataReplaced)
            : base(locale, assetName, data, getNormalizedPath, onDataReplaced) { }

        /// <inheritdoc />
        public void PatchImage(IRawTextureData source, Rectangle? sourceArea = null, Rectangle? targetArea = null, PatchMode patchMode = PatchMode.Replace)
        {
            // nullcheck
            if (source == null)
                throw new ArgumentNullException(nameof(source), "Can't patch from null source data.");

            this.GetPatchBounds(ref sourceArea, ref targetArea, source.Width, source.Height);

            // get the pixels for the source area
            Color[] sourceData;
            {
                int areaX = sourceArea.Value.X;
                int areaY = sourceArea.Value.Y;
                int areaWidth = sourceArea.Value.Width;
                int areaHeight = sourceArea.Value.Height;

                if (areaX == 0 && areaY == 0 && areaWidth == source.Width && areaHeight <= source.Height)
                {
                    // It's actually fine if the source is taller than the sourceArea
                    // the "extra" bits on the end of the array can just be ignored.
                    sourceData = source.Data;
                    this.PatchImageImpl(sourceData, areaWidth, areaHeight, sourceArea.Value, targetArea.Value, patchMode);
                }
                else
                {
                    int pixelCount = areaWidth * areaHeight;
                    sourceData = ArrayPool<Color>.Shared.Rent(pixelCount);

                    if (areaX == 0 && areaWidth == source.Width)
                    {
                        // shortcut copying because the area to copy is contiguous. This is
                        // probably uncommon, but Array.Copy benefits a lot.

                        int sourceIndex = areaY * areaWidth;
                        int targetIndex = 0;
                        Array.Copy(source.Data, sourceIndex, sourceData, targetIndex, pixelCount);

                    }
                    else
                    {
                        // slower copying, line by line
                        for (int y = areaY, maxY = areaY + areaHeight; y < maxY; y++)
                        {
                            int sourceIndex = (y * source.Width) + areaX;
                            int targetIndex = (y - areaY) * areaWidth;
                            Array.Copy(source.Data, sourceIndex, sourceData, targetIndex, areaWidth);
                        }
                    }

                    // apply
                    this.PatchImageImpl(sourceData, source.Width, source.Height, sourceArea.Value, targetArea.Value, patchMode);

                    // return
                    ArrayPool<Color>.Shared.Return(sourceData);
                }
            }
        }

        /// <inheritdoc />
        public void PatchImage(Texture2D source, Rectangle? sourceArea = null, Rectangle? targetArea = null, PatchMode patchMode = PatchMode.Replace)
        {
            // nullcheck
            if (source == null)
                throw new ArgumentNullException(nameof(source), "Can't patch from a null source texture.");

            this.GetPatchBounds(ref sourceArea, ref targetArea, source.Width, source.Height);

            // validate source bounds
            if (!source.Bounds.Contains(sourceArea.Value))
                throw new ArgumentOutOfRangeException(nameof(sourceArea), "The source area is outside the bounds of the source texture.");

            // get source data
            int pixelCount = sourceArea.Value.Width * sourceArea.Value.Height;
            Color[] sourceData = ArrayPool<Color>.Shared.Rent(pixelCount);
            source.GetData(0, sourceArea, sourceData, 0, pixelCount);

            // apply
            this.PatchImageImpl(sourceData, source.Width, source.Height, sourceArea.Value, targetArea.Value, patchMode);

            // return
            ArrayPool<Color>.Shared.Return(sourceData);
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


        /*********
        ** Private methods
        *********/
        /// <summary>Get the bounds for an image patch.</summary>
        /// <param name="sourceArea">The source area to set if needed.</param>
        /// <param name="targetArea">The target area to set if needed.</param>
        /// <param name="sourceWidth">The width of the full source image.</param>
        /// <param name="sourceHeight">The height of the full source image.</param>
        private void GetPatchBounds([NotNull] ref Rectangle? sourceArea, [NotNull] ref Rectangle? targetArea, int sourceWidth, int sourceHeight)
        {
            sourceArea ??= new Rectangle(0, 0, sourceWidth, sourceHeight);
            targetArea ??= new Rectangle(0, 0, Math.Min(sourceArea.Value.Width, this.Data.Width), Math.Min(sourceArea.Value.Height, this.Data.Height));
        }

        /// <summary>Overwrite part of the image.</summary>
        /// <param name="sourceData">The image data to patch into the content.</param>
        /// <param name="sourceWidth">The pixel width of the source image.</param>
        /// <param name="sourceHeight">The pixel height of the source image.</param>
        /// <param name="sourceArea">The part of the <paramref name="sourceData"/> to copy (or <c>null</c> to take the whole texture). This must be within the bounds of the <paramref name="sourceData"/> texture.</param>
        /// <param name="targetArea">The part of the content to patch (or <c>null</c> to patch the whole texture). The original content within this area will be erased. This must be within the bounds of the existing spritesheet.</param>
        /// <param name="patchMode">Indicates how an image should be patched.</param>
        /// <exception cref="ArgumentNullException">One of the arguments is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="targetArea"/> is outside the bounds of the spritesheet.</exception>
        /// <exception cref="InvalidOperationException">The content being read isn't an image.</exception>
        private void PatchImageImpl(Color[] sourceData, int sourceWidth, int sourceHeight, Rectangle sourceArea, Rectangle targetArea, PatchMode patchMode)
        {
            // get texture
            Texture2D target = this.Data;
            int pixelCount = sourceArea.Width * sourceArea.Height;

            // validate
            if (sourceArea.X < 0 || sourceArea.Y < 0 || sourceArea.Right > sourceWidth || sourceArea.Bottom > sourceHeight)
                throw new ArgumentOutOfRangeException(nameof(sourceArea), "The source area is outside the bounds of the source texture.");
            if (!target.Bounds.Contains(targetArea))
                throw new ArgumentOutOfRangeException(nameof(targetArea), "The target area is outside the bounds of the target texture.");
            if (sourceArea.Size != targetArea.Size)
                throw new InvalidOperationException("The source and target areas must be the same size.");

            if (patchMode == PatchMode.Replace)
                target.SetData(0, targetArea, sourceData, 0, pixelCount);
            else
            {
                // merge data

                // Content packs have a habit of using large amounts of blank space.
                // Adjusting bounds to ignore transparent pixels at the start and end.

                int startIndex = -1;
                for (int i = 0; i < pixelCount; i++)
                {
                    if (sourceData[i].A >= MinOpacity)
                    {
                        startIndex = i;
                        break;
                    }
                }

                if (startIndex == -1)
                    return;

                int endIndex = -1;
                for (int i = pixelCount - 1; i >= startIndex; i--)
                {
                    if (sourceData[i].A >= MinOpacity)
                    {
                        endIndex = i;
                        break;
                    }
                }

                if (endIndex == -1)
                    return;

                int topoffset = startIndex / sourceArea.Width;
                int bottomoffset = endIndex / sourceArea.Width;

                // Update target rectangle
                targetArea = new(targetArea.X, targetArea.Y + topoffset, targetArea.Width, bottomoffset - topoffset + 1);

                pixelCount = targetArea.Width * targetArea.Height;

                int sourceoffset = topoffset * sourceArea.Width;

                // get target data
                Color[] mergedData = ArrayPool<Color>.Shared.Rent(pixelCount);
                target.GetData(0, targetArea, mergedData, 0, pixelCount);

                // merge pixels
                for (int i = 0; i < pixelCount; i++)
                {
                    // ref locals here? Not sure.
                    Color above = sourceData[sourceoffset + i];
                    Color below = mergedData[i];

                    // shortcut transparency
                    if (above.A < MinOpacity)
                        continue;
                    if (below.A < MinOpacity || above.A == byte.MaxValue)
                        mergedData[i] = above;

                    // merge pixels
                    else
                    {
                        // This performs a conventional alpha blend for the pixels, which are already
                        // premultiplied by the content pipeline. The formula is derived from
                        // https://blogs.msdn.microsoft.com/shawnhar/2009/11/06/premultiplied-alpha/.
                        float alphaBelow = 1 - (above.A / 255f);
                        mergedData[i] = new Color(
                            r: (int)(above.R + (below.R * alphaBelow)),
                            g: (int)(above.G + (below.G * alphaBelow)),
                            b: (int)(above.B + (below.B * alphaBelow)),
                            alpha: Math.Max(above.A, below.A)
                        );
                    }
                }

                target.SetData(0, targetArea, mergedData, 0, pixelCount);
                ArrayPool<Color>.Shared.Return(mergedData);
            }
        }
    }
}
