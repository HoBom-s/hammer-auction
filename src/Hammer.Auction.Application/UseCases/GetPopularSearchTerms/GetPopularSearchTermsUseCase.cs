using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetPopularSearchTerms;

/// <summary>
/// Retrieves globally popular search terms within a given period.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetPopularSearchTermsUseCase(ISearchLogRepository repository) : IGetPopularSearchTermsUseCase
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<PopularSearchTermResponse>> ExecuteAsync(
        GetPopularSearchTermsRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<(string Keyword, int Count)> rows =
            await repository.GetPopularAsync(request.Days, request.Limit, ct);

        return rows.Select(r => new PopularSearchTermResponse(r.Keyword, r.Count)).ToList();
    }
}
