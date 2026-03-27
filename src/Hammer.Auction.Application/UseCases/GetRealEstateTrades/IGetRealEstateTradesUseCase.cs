using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.GetRealEstateTrades;

/// <summary>
/// Use case contract for retrieving paginated real estate trade records.
/// </summary>
public interface IGetRealEstateTradesUseCase
{
    /// <summary>
    /// Retrieves a paginated list of real estate trade records.
    /// </summary>
    public Task<PagedResponse<RealEstateTradeResponse>> ExecuteAsync(
        GetRealEstateTradesRequest request,
        CancellationToken ct = default);
}
