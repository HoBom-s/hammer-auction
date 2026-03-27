using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Domain.Ports;

/// <summary>
/// Repository port for real estate trade records.
/// </summary>
public interface IRealEstateTradeRepository
{
    /// <summary>
    /// Retrieves a paginated list of trades with optional filtering.
    /// </summary>
    public Task<(IReadOnlyList<RealEstateTrade> Items, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? lawdCd,
        int? propertyType,
        CancellationToken ct = default);
}
