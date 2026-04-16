using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Auction.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for KAMCO auction items.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class KamcoAuctionItemRepository(AuctionDbContext db) : IKamcoAuctionItemRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<KamcoAuctionItem>> FindByNaturalKeysAsync(
        IReadOnlyList<(long PlnmNo, long PbctNo, long CltrNo)> keys,
        CancellationToken ct = default)
    {
        var plnmNos = keys.Select(k => k.PlnmNo).Distinct().ToList();

        List<KamcoAuctionItem> candidates = await db.KamcoAuctionItems
            .Where(e => plnmNos.Contains(e.PlnmNo))
            .ToListAsync(ct);

        var keySet = new HashSet<(long, long, long)>(keys);

        return candidates
            .Where(e => keySet.Contains((e.PlnmNo, e.PbctNo, e.CltrNo)))
            .ToList();
    }

    /// <inheritdoc />
    public void Add(KamcoAuctionItem item)
    {
        db.KamcoAuctionItems.Add(item);
    }

    /// <inheritdoc />
    public async Task<KamcoAuctionItem?> GetByIdAsync(KamcoAuctionItemId id, CancellationToken ct = default)
    {
        return await db.KamcoAuctionItems.FindAsync([id.Value], ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<(string CtgrFullNm, int Count)>> CountByCtgrFullNmAsync(
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdTo,
        CancellationToken ct = default)
    {
        IQueryable<KamcoAuctionItem> query = db.KamcoAuctionItems.AsNoTracking();

        if (createdFrom.HasValue)
            query = query.Where(e => e.CreatedAt >= createdFrom.Value);

        if (createdTo.HasValue)
            query = query.Where(e => e.CreatedAt < createdTo.Value);

        var rows = await query
            .GroupBy(e => e.CtgrFullNm)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return rows.Select(x => (x.Key, x.Count)).ToList();
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<KamcoAuctionItem> Items, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? status,
        string? category,
        string? keyword,
        CancellationToken ct = default)
    {
        IQueryable<KamcoAuctionItem> query = db.KamcoAuctionItems.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(e => e.PbctCltrStatNm == status);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(e => e.CtgrFullNm.Contains(category));

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(e =>
                e.CltrNm.Contains(keyword) ||
                e.LdnmAdrs.Contains(keyword) ||
                e.NmrdAdrs.Contains(keyword));
        }

        var totalCount = await query.CountAsync(ct);

        List<KamcoAuctionItem> items = await query
            .OrderByDescending(e => e.PbctBegnDtm)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<KamcoAuctionItem>> GetByDateRangeAsync(
        DateTimeOffset from,
        DateTimeOffset toExclusive,
        CancellationToken ct = default)
    {
        return await db.KamcoAuctionItems
            .AsNoTracking()
            .Where(e => (e.PbctBegnDtm >= from && e.PbctBegnDtm < toExclusive)
                || (e.PbctClsDtm >= from && e.PbctClsDtm < toExclusive))
            .OrderBy(e => e.PbctBegnDtm)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<KamcoAuctionItem> Items, int TotalCount)> SearchAsync(
        string keyword,
        int limit,
        CancellationToken ct = default)
    {
        IQueryable<KamcoAuctionItem> query = db.KamcoAuctionItems
            .AsNoTracking()
            .Where(e =>
                e.CltrNm.Contains(keyword) ||
                e.LdnmAdrs.Contains(keyword) ||
                e.NmrdAdrs.Contains(keyword));

        var totalCount = await query.CountAsync(ct);

        List<KamcoAuctionItem> items = await query
            .OrderByDescending(e => e.PbctBegnDtm)
            .Take(limit)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
