using System;
using ToolkitPathUtilities = StardewModdingAPI.Toolkit.Utilities.PathUtilities;

namespace StardewModdingAPI.Utilities.AssetPathUtilities;

/// <summary>
/// A helper class that yields out each bit of an asset path
/// </summary>
internal ref struct AssetNamePartEnumerator
{
    private ReadOnlySpan<char> RemainderImpl;

    /// <summary>
    /// Construct an instance.
    /// </summary>
    /// <param name="assetName">The asset name.</param>
    internal AssetNamePartEnumerator(ReadOnlySpan<char> assetName)
    {
        this.RemainderImpl = AssetNamePartEnumerator.TrimLeadingPathSeparators(assetName);
    }

    /// <summary>
    /// The remainder of the assetName (that hasn't been yielded out yet.)
    /// </summary>
    internal ReadOnlySpan<char> Remainder => this.RemainderImpl;

    /// <summary>
    /// The current segment.
    /// </summary>
    public ReadOnlySpan<char> Current { get; private set; } = default;

    // this is just so it can be used in a foreach loop.
    public AssetNamePartEnumerator GetEnumerator() => this;

    /// <summary>
    /// Moves the enumerator to the next element.
    /// </summary>
    /// <returns>True if there is a new</returns>
    public bool MoveNext()
    {
        if (this.RemainderImpl.Length == 0)
        {
            return false;
        }

        int index = this.RemainderImpl.IndexOfAny(ToolkitPathUtilities.PossiblePathSeparators);

        // no more separator characters found, I'm done.
        if (index < 0)
        {
            this.Current = this.RemainderImpl;
            this.RemainderImpl = ReadOnlySpan<char>.Empty;
            return true;
        }

        // Yield the next separate character bit
        this.Current = this.RemainderImpl[..index];
        this.RemainderImpl = AssetNamePartEnumerator.TrimLeadingPathSeparators(this.RemainderImpl[(index + 1)..]);
        return true;
    }

    private static ReadOnlySpan<char> TrimLeadingPathSeparators(ReadOnlySpan<char> span)
    {
        return span.TrimStart(new ReadOnlySpan<char>(ToolkitPathUtilities.PossiblePathSeparators));
    }
}
