namespace Hammer.Auction.Application.UseCases.GetRecentSearchTerms;

/// <summary>
/// Request parameters for recent search terms.
/// </summary>
public sealed record GetRecentSearchTermsRequest(
    string UserId,
    int Limit = 10);
