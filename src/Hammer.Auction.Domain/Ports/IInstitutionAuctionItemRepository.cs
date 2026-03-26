using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Domain.Ports;

/// <summary>
/// Repository port for institution auction items.
/// </summary>
public interface IInstitutionAuctionItemRepository
{
    /// <summary>
    /// Finds an item by its surrogate primary key.
    /// </summary>
    public Task<InstitutionAuctionItem?> GetByIdAsync(long id, CancellationToken ct = default);

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
}
