using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Domain.Ports;

/// <summary>
/// Repository port for KAMCO auction items.
/// </summary>
public interface IKamcoAuctionItemRepository
{
    /// <summary>
    /// Finds existing items matching any of the given composite natural keys.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<IReadOnlyList<KamcoAuctionItem>> FindByNaturalKeysAsync(
        IReadOnlyList<(long PlnmNo, long PbctNo, long CltrNo)> keys,
        CancellationToken ct = default);

    /// <summary>
    /// Adds a new item to the context.
    /// </summary>
    public void Add(KamcoAuctionItem item);

    /// <summary>
    /// Finds an item by its surrogate primary key.
    /// </summary>
    public Task<KamcoAuctionItem?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a paginated list of items with optional filtering.
    /// </summary>
    public Task<(IReadOnlyList<KamcoAuctionItem> Items, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? status,
        string? category,
        string? keyword,
        CancellationToken ct = default);
}
