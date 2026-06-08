using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.DeleteSearchHistory;

/// <summary>
/// Deletes all search history for a user.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class DeleteSearchHistoryUseCase(ISearchLogRepository repository) : IDeleteSearchHistoryUseCase
{
    /// <inheritdoc />
    public async Task<int> ExecuteAsync(string userId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return await repository.DeleteByUserAsync(userId, ct);
    }
}
