using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.ReadAllNotifications;

/// <summary>
///     Marks all unread notifications as read for the given user.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class ReadAllNotificationsUseCase(INotificationRepository repository) : IReadAllNotificationsUseCase
{
    /// <inheritdoc />
    public async Task ExecuteAsync(UserId userId, CancellationToken ct = default)
    {
        await repository.MarkAllAsReadAsync(userId.Value, ct);
    }
}
