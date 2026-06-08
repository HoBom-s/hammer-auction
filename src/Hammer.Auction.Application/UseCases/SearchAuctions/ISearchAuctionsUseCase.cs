using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.SearchAuctions;

/// <summary>
/// Use case contract for unified auction item search.
/// </summary>
public interface ISearchAuctionsUseCase
{
    /// <summary>
    /// Searches auction items across all sources.
    /// </summary>
    public Task<PagedResponse<UnifiedAuctionItemResponse>> ExecuteAsync(
        SearchAuctionsRequest request,
        string userId,
        CancellationToken ct = default);
}
