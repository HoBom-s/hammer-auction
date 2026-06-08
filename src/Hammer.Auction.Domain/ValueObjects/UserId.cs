namespace Hammer.Auction.Domain.ValueObjects;

/// <summary>
///     Gateway가 전달한 사용자 식별자.
/// </summary>
public readonly record struct UserId
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserId" /> struct.
    /// </summary>
    public UserId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    /// <summary>
    ///     Gets the underlying string value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Gets the anonymous user identifier.
    /// </summary>
    public static UserId Anonymous => new("anonymous");

    /// <inheritdoc />
    public override string ToString() => Value;
}
