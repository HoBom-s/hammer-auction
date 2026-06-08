namespace Hammer.Auction.Application.UseCases.GetRecentSearchTerms;

/// <summary>
/// Use case contract for retrieving a user's recent search terms.
/// </summary>
public interface IGetRecentSearchTermsUseCase
{
    /// <summary>
    /// Retrieves the user's most recent distinct search terms.
    /// </summary>
    public Task<IReadOnlyList<string>> ExecuteAsync(
        GetRecentSearchTermsRequest request,
        CancellationToken ct = default);
}
