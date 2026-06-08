namespace Hammer.Auction.Application.UseCases.GetInstitutionAuctionItems;

/// <summary>
/// Request parameters for paginated institution auction item listing.
/// </summary>
public sealed record GetInstitutionAuctionItemsRequest(
    int Page = 1,
    int Size = 20,
    string? Org = null,
    string? Category = null,
    string? Keyword = null);
