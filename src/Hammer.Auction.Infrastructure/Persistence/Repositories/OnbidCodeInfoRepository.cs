using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Auction.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Onbid code info entries.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class OnbidCodeInfoRepository(AuctionDbContext db) : IOnbidCodeInfoRepository
{
    /// <inheritdoc />
    public async Task<OnbidCodeInfo?> GetByIdAsync(OnbidCodeInfoId id, CancellationToken ct = default)
    {
        return await db.OnbidCodeInfos.FindAsync([id.Value], ct);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<OnbidCodeInfo> Items, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? parentId,
        CancellationToken ct = default)
    {
        IQueryable<OnbidCodeInfo> query = db.OnbidCodeInfos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parentId))
            query = query.Where(e => e.CtgrHirkId == parentId);

        var totalCount = await query.CountAsync(ct);

        List<OnbidCodeInfo> items = await query
            .OrderBy(e => e.CtgrId)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
