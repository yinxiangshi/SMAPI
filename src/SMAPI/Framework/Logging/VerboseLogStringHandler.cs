using System.Runtime.CompilerServices;

namespace StardewModdingAPI.Framework.Logging;

/// <summary>
/// An interpolated string handler to handle verbose logging.
/// </summary>
[InterpolatedStringHandler]
public ref struct VerboseLogStringHandler
{
    private readonly DefaultInterpolatedStringHandler _handler;

    /// <summary>
    /// Construct an instance.
    /// </summary>
    /// <param name="literalLength">The total length of literals used in interpolation.</param>
    /// <param name="formattedCount">The number of interpolation holes to fill.</param>
    /// <param name="monitor">The monitor instance.</param>
    /// <param name="isValid">Whether or not </param>
    public VerboseLogStringHandler(int literalLength, int formattedCount, IMonitor monitor, out bool isValid)
    {
        if (monitor.IsVerbose)
        {
            this._handler = new(literalLength, formattedCount);
            isValid = true;
            return;
        }

        isValid = false;
        this._handler = default;
    }

    /// <inheritdoc cref="DefaultInterpolatedStringHandler.AppendLiteral(string)"/>
    public void AppendLiteral(string literal)
    {
        this._handler.AppendLiteral(literal);
    }

    /// <inheritdoc cref="DefaultInterpolatedStringHandler.AppendFormatted{T}(T)"/>
    public void AppendFormatted<T>(T value)
    {
        this._handler.AppendFormatted(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return this._handler.ToStringAndClear();
    }
}
