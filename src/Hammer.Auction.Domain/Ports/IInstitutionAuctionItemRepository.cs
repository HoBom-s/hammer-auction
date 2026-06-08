using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Domain.Ports;

/// <summary>
/// Repository port for institution auction items.
/// </summary>
public interface IInstitutionAuctionItemRepository
{
    /// <summary>
    /// Finds an item by its surrogate primary key.
    /// </summary>
    public Task<InstitutionAuctionItem?> GetByIdAsync(InstitutionAuctionItemId id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a paginated list of items with optional filtering.
    /// </summary>
    public Task<(IReadOnlyList<InstitutionAuctionItem> Items, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? org,
        string? category,
        string? keyword,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves items whose bid period overlaps the given date range.
    /// </summary>
    public Task<IReadOnlyList<InstitutionAuctionItem>> GetByDateRangeAsync(
        DateTimeOffset from,
        DateTimeOffset toExclusive,
        CancellationToken ct = default);

    /// <summary>
    /// Searches items by keyword across announcement name.
    /// </summary>
    public Task<(IReadOnlyList<InstitutionAuctionItem> Items, int TotalCount)> SearchAsync(
        string keyword,
        int limit,
        CancellationToken ct = default);
}
