using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.GetAuctionItems;

/// <summary>
/// Use case contract for retrieving paginated auction items.
/// </summary>
public interface IGetAuctionItemsUseCase
{
    /// <summary>
    /// Retrieves a paginated list of auction items.
    /// </summary>
    public Task<PagedResponse<KamcoAuctionItemResponse>> ExecuteAsync(
        GetAuctionItemsRequest request,
        CancellationToken ct = default);
}
