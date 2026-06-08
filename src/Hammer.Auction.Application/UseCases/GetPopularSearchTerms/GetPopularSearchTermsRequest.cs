namespace Hammer.Auction.Application.UseCases.GetPopularSearchTerms;

/// <summary>
/// Request parameters for popular search terms.
/// </summary>
public sealed record GetPopularSearchTermsRequest(
    int Days = 7,
    int Limit = 10);
