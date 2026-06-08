namespace Hammer.Auction.Domain.Entities;

/// <summary>
///     Category code from the Onbid code information API.
/// </summary>
public sealed class OnbidCodeInfo
{
    private OnbidCodeInfo()
    {
    }

    /// <summary>
    ///     Gets the surrogate primary key.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    ///     Gets the category ID (코드 ID).
    /// </summary>
    public string CtgrId { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the category name (코드명).
    /// </summary>
    public string CtgrNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the parent category ID (상위 코드 ID).
    /// </summary>
    public string CtgrHirkId { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the parent category name (상위 코드명).
    /// </summary>
    public string CtgrHirkNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    ///     Gets the last update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    ///     Creates a new code info entry.
    /// </summary>
    public static OnbidCodeInfo Create(
        string ctgrId,
        string ctgrNm,
        string ctgrHirkId,
        string ctgrHirkNm)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return new OnbidCodeInfo
        {
            CtgrId = ctgrId,
            CtgrNm = ctgrNm,
            CtgrHirkId = ctgrHirkId,
            CtgrHirkNm = ctgrHirkNm,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    /// <summary>
    ///     Updates mutable fields from a snapshot.
    /// </summary>
    public void UpdateFromSnapshot(
        string ctgrNm,
        string ctgrHirkId,
        string ctgrHirkNm)
    {
        CtgrNm = ctgrNm;
        CtgrHirkId = ctgrHirkId;
        CtgrHirkNm = ctgrHirkNm;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
