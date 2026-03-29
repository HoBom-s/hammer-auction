using System.Globalization;

namespace Hammer.Auction.Domain.ValueObjects;

/// <summary>
///     Strongly-typed identifier for a <see cref="Entities.InstitutionAuctionItem" />.
/// </summary>
public readonly record struct InstitutionAuctionItemId : IParsable<InstitutionAuctionItemId>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InstitutionAuctionItemId" /> struct.
    /// </summary>
    public InstitutionAuctionItemId(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        Value = value;
    }

    /// <summary>
    ///     Gets the underlying identifier value.
    /// </summary>
    public long Value { get; }

    /// <inheritdoc />
    public static InstitutionAuctionItemId Parse(string s, IFormatProvider? provider) =>
        new(long.Parse(s, provider));

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, out InstitutionAuctionItemId result)
    {
        if (long.TryParse(s, provider, out var value) && value > 0)
        {
            result = new InstitutionAuctionItemId(value);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
