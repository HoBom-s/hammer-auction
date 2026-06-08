using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.GetAuctionItemById;

/// <summary>
/// Use case contract for retrieving a single auction item by ID.
/// </summary>
public interface IGetAuctionItemByIdUseCase
{
    /// <summary>
    /// Retrieves an auction item by its surrogate ID.
    /// </summary>
    public Task<KamcoAuctionItemResponse> ExecuteAsync(KamcoAuctionItemId id, CancellationToken ct = default);
}
