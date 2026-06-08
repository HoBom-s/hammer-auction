using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.ValueObjects;

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
    public Task<KamcoAuctionItem?> GetByIdAsync(KamcoAuctionItemId id, CancellationToken ct = default);

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

    /// <summary>
    /// Counts items grouped by <see cref="KamcoAuctionItem.CtgrFullNm"/> within a date range.
    /// When <paramref name="createdFrom"/> is <c>null</c>, all items are counted regardless of creation date.
    /// </summary>
    public Task<IReadOnlyList<(string CtgrFullNm, int Count)>> CountByCtgrFullNmAsync(
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdTo,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves items whose bid period overlaps the given date range.
    /// </summary>
    public Task<IReadOnlyList<KamcoAuctionItem>> GetByDateRangeAsync(
        DateTimeOffset from,
        DateTimeOffset toExclusive,
        CancellationToken ct = default);

    /// <summary>
    /// Searches items by keyword across name and address fields.
    /// </summary>
    public Task<(IReadOnlyList<KamcoAuctionItem> Items, int TotalCount)> SearchAsync(
        string keyword,
        int limit,
        CancellationToken ct = default);
}
