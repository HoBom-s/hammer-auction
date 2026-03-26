using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Auction.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for institution auction items.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class InstitutionAuctionItemRepository(AuctionDbContext db) : IInstitutionAuctionItemRepository
{
    /// <inheritdoc />
    public async Task<InstitutionAuctionItem?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await db.InstitutionAuctionItems.FindAsync([id], ct);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<InstitutionAuctionItem> Items, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? org,
        string? category,
        string? keyword,
        CancellationToken ct = default)
    {
        IQueryable<InstitutionAuctionItem> query = db.InstitutionAuctionItems.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(org))
            query = query.Where(e => e.OrgNm == org);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(e => e.CtgrFullNm.Contains(category));

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(e => e.PlnmNm.Contains(keyword));

        var totalCount = await query.CountAsync(ct);

        List<InstitutionAuctionItem> items = await query
            .OrderByDescending(e => e.PbctBegnDtm)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
