using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.GetUnreadNotificationCount;

/// <summary>
///     Returns the number of unread notifications for the given user.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetUnreadNotificationCountUseCase(INotificationRepository repository)
    : IGetUnreadNotificationCountUseCase
{
    /// <inheritdoc />
    public async Task<int> ExecuteAsync(UserId userId, CancellationToken ct = default)
    {
        return await repository.GetUnreadCountAsync(userId.Value, ct);
    }
}
