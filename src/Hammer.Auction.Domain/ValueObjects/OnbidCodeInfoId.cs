using System.Globalization;

namespace Hammer.Auction.Domain.ValueObjects;

/// <summary>
///     Strongly-typed identifier for a <see cref="Entities.OnbidCodeInfo" />.
/// </summary>
public readonly record struct OnbidCodeInfoId : IParsable<OnbidCodeInfoId>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="OnbidCodeInfoId" /> struct.
    /// </summary>
    public OnbidCodeInfoId(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        Value = value;
    }

    /// <summary>
    ///     Gets the underlying identifier value.
    /// </summary>
    public long Value { get; }

    /// <inheritdoc />
    public static OnbidCodeInfoId Parse(string s, IFormatProvider? provider) =>
        new(long.Parse(s, provider));

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, out OnbidCodeInfoId result)
    {
        if (long.TryParse(s, provider, out var value) && value > 0)
        {
            result = new OnbidCodeInfoId(value);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
