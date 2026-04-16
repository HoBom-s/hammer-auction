using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.GetPopularSearchTerms;

/// <summary>
/// Use case contract for retrieving popular search terms.
/// </summary>
public interface IGetPopularSearchTermsUseCase
{
    /// <summary>
    /// Retrieves globally popular search terms.
    /// </summary>
    public Task<IReadOnlyList<PopularSearchTermResponse>> ExecuteAsync(
        GetPopularSearchTermsRequest request,
        CancellationToken ct = default);
}
