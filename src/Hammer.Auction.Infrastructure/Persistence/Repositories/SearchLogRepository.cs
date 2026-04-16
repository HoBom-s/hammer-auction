using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Auction.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for search log entries.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class SearchLogRepository(AuctionDbContext db) : ISearchLogRepository
{
    /// <inheritdoc />
    public void Add(SearchLog log)
    {
        db.SearchLogs.Add(log);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<(string Keyword, int Count)>> GetPopularAsync(
        int days,
        int limit,
        CancellationToken ct = default)
    {
        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-days);

        var rows = await db.SearchLogs
            .AsNoTracking()
            .Where(e => e.SearchedAt >= cutoff)
            .GroupBy(e => e.Keyword)
            .Select(g => new { Keyword = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(limit)
            .ToListAsync(ct);

        return rows.Select(x => (x.Keyword, x.Count)).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetRecentByUserAsync(
        string userId,
        int limit,
        CancellationToken ct = default)
    {
        // Get latest search per keyword using GroupBy + Max
        var rows = await db.SearchLogs
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .GroupBy(e => e.Keyword)
            .Select(g => new { Keyword = g.Key, LatestAt = g.Max(e => e.SearchedAt) })
            .OrderByDescending(x => x.LatestAt)
            .Take(limit)
            .ToListAsync(ct);

        return rows.Select(x => x.Keyword).ToList();
    }

    /// <inheritdoc />
    public async Task<int> DeleteByUserAsync(string userId, CancellationToken ct = default)
    {
        return await db.SearchLogs
            .Where(e => e.UserId == userId)
            .ExecuteDeleteAsync(ct);
    }
}
