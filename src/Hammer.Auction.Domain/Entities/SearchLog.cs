namespace Hammer.Auction.Domain.Entities;

/// <summary>
///     User search keyword log for popular/recent search tracking.
/// </summary>
public sealed class SearchLog
{
    private SearchLog()
    {
    }

    /// <summary>
    ///     Gets the surrogate primary key.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    ///     Gets the normalized search keyword.
    /// </summary>
    public string Keyword { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the user identifier who performed the search.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the timestamp when the search was performed.
    /// </summary>
    public DateTimeOffset SearchedAt { get; private set; }

    /// <summary>
    ///     Creates a new search log entry with normalized keyword.
    /// </summary>
    public static SearchLog Create(string keyword, string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyword);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return new SearchLog
        {
            Keyword = keyword.Trim().ToUpperInvariant(),
            UserId = userId,
            SearchedAt = DateTimeOffset.UtcNow,
        };
    }
}
