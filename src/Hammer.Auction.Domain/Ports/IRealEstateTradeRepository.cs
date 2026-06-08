using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Domain.Ports;

/// <summary>
/// Repository port for real estate trade records.
/// </summary>
public interface IRealEstateTradeRepository
{
    /// <summary>
    /// Finds recent trades matching a specific location (district name + lot number).
    /// </summary>
    public Task<IReadOnlyList<RealEstateTrade>> FindByLocationAsync(
        string umdNm,
        string jibun,
        int limit,
        CancellationToken ct = default);
}
