using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Domain.Ports;

/// <summary>
/// Repository port for search log entries.
/// </summary>
public interface ISearchLogRepository
{
    /// <summary>
    /// Adds a new search log to the context.
    /// </summary>
    public void Add(SearchLog log);

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the most popular search keywords globally within the given period.
    /// </summary>
    public Task<IReadOnlyList<(string Keyword, int Count)>> GetPopularAsync(
        int days,
        int limit,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the most recent distinct keywords searched by a specific user.
    /// </summary>
    public Task<IReadOnlyList<string>> GetRecentByUserAsync(
        string userId,
        int limit,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes all search logs for the given user.
    /// </summary>
    public Task<int> DeleteByUserAsync(string userId, CancellationToken ct = default);
}
