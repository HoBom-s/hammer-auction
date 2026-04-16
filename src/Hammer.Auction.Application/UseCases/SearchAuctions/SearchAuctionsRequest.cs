namespace Hammer.Auction.Application.UseCases.SearchAuctions;

/// <summary>
/// Request parameters for unified auction search.
/// </summary>
public sealed record SearchAuctionsRequest(
    string? Keyword,
    int Page = 1,
    int Size = 20);
