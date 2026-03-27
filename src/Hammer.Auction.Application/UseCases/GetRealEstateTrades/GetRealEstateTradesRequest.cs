namespace Hammer.Auction.Application.UseCases.GetRealEstateTrades;

/// <summary>
/// Request parameters for paginated real estate trade listing.
/// </summary>
public sealed record GetRealEstateTradesRequest(
    int Page = 1,
    int Size = 20,
    string? LawdCd = null,
    int? PropertyType = null);
