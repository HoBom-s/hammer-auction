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
    public async Task<IReadOnlyList<RealEstateTrade>> FindByLocationAsync(
        string umdNm,
        string jibun,
        int limit,
        CancellationToken ct = default)
    {
        return await db.RealEstateTrades
            .AsNoTracking()
            .Where(e => e.UmdNm == umdNm && e.Jibun == jibun)
            .OrderByDescending(e => e.DealYear)
            .ThenByDescending(e => e.DealMonth)
            .ThenByDescending(e => e.DealDay)
            .Take(limit)
            .ToListAsync(ct);
    }
}
