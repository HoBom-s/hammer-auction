using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Auction.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for real estate trade records.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class RealEstateTradeRepository(AuctionDbContext db) : IRealEstateTradeRepository
{
    /// <inheritdoc />
    public async Task<(IReadOnlyList<RealEstateTrade> Items, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? lawdCd,
        int? propertyType,
        CancellationToken ct = default)
    {
        IQueryable<RealEstateTrade> query = db.RealEstateTrades.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(lawdCd))
            query = query.Where(e => e.LawdCd == lawdCd);

        if (propertyType.HasValue)
            query = query.Where(e => e.PropertyType == propertyType.Value);

        var totalCount = await query.CountAsync(ct);

        List<RealEstateTrade> items = await query
            .OrderByDescending(e => e.DealYear)
            .ThenByDescending(e => e.DealMonth)
            .ThenByDescending(e => e.DealDay)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
