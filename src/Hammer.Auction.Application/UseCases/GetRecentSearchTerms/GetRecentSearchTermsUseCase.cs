using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetRecentSearchTerms;

/// <summary>
/// Retrieves a user's most recent distinct search terms.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetRecentSearchTermsUseCase(ISearchLogRepository repository) : IGetRecentSearchTermsUseCase
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ExecuteAsync(
        GetRecentSearchTermsRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await repository.GetRecentByUserAsync(request.UserId, request.Limit, ct);
    }
}
