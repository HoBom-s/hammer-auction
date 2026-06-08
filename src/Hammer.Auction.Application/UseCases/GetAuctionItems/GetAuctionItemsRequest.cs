namespace Hammer.Auction.Application.UseCases.GetAuctionItems;

/// <summary>
/// Request parameters for paginated auction item listing.
/// </summary>
public sealed record GetAuctionItemsRequest(
    int Page = 1,
    int Size = 20,
    string? Status = null,
    string? Category = null,
    string? Keyword = null);
