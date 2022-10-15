using System;

using ToolkitPathUtilities = StardewModdingAPI.Toolkit.Utilities.PathUtilities;

namespace StardewModdingAPI.Utilities.AssetPathUtilities;

/// <summary>
/// A helper class that yields out each bit of an asset path
/// </summary>
internal ref struct AssetPartYielder
{
    private ReadOnlySpan<char> remainder;

    /// <summary>
    /// Construct an instance.
    /// </summary>
    /// <param name="assetName">The asset name.</param>
    internal AssetPartYielder(ReadOnlySpan<char> assetName)
    {
        this.remainder = AssetPartYielder.TrimLeadingPathSeperators(assetName);
    }

    /// <summary>
    /// The remainder of the assetName (that hasn't been yielded out yet.)
    /// </summary>
    internal ReadOnlySpan<char> Remainder => this.remainder;

    /// <summary>
    /// The current segment.
    /// </summary>
    public ReadOnlySpan<char> Current { get; private set; } = default;

    // this is just so it can be used in a foreach loop.
    public AssetPartYielder GetEnumerator() => this;

    /// <summary>
    /// Moves the enumerator to the next element.
    /// </summary>
    /// <returns>True if there is a new</returns>
    public bool MoveNext()
    {
        if (this.remainder.Length == 0)
        {
            return false;
        }

        int index = this.remainder.IndexOfAny(ToolkitPathUtilities.PossiblePathSeparators);

        // no more seperator characters found, I'm done.
        if (index < 0)
        {
            this.Current = this.remainder;
            this.remainder = ReadOnlySpan<char>.Empty;
            return true;
        }

        // Yield the next seperate character bit
        this.Current = this.remainder[..index];
        this.remainder = AssetPartYielder.TrimLeadingPathSeperators(this.remainder[(index + 1)..]);
        return true;
    }

    private static ReadOnlySpan<char> TrimLeadingPathSeperators(ReadOnlySpan<char> span)
    {
        return span.TrimStart(new ReadOnlySpan<char>(ToolkitPathUtilities.PossiblePathSeparators));
    }
}
