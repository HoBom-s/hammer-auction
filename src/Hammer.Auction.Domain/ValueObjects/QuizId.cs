using System.Globalization;

namespace Hammer.Auction.Domain.ValueObjects;

/// <summary>
///     Strongly-typed identifier for a quiz.
/// </summary>
public readonly record struct QuizId : IParsable<QuizId>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="QuizId" /> struct.
    /// </summary>
    public QuizId(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        Value = value;
    }

    /// <summary>
    ///     Gets the underlying identifier value.
    /// </summary>
    public long Value { get; }

    /// <inheritdoc />
    public static QuizId Parse(string s, IFormatProvider? provider) =>
        new(long.Parse(s, provider));

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, out QuizId result)
    {
        if (long.TryParse(s, provider, out var value) && value > 0)
        {
            result = new QuizId(value);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
