using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.ReadNotification;

/// <summary>
///     Marks a single notification as read.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class ReadNotificationUseCase(INotificationRepository repository) : IReadNotificationUseCase
{
    /// <inheritdoc />
    public async Task ExecuteAsync(UserId userId, long notificationId, CancellationToken ct = default)
    {
        Notification notification = await repository.GetByIdAsync(notificationId, ct)
            ?? throw new NotFoundException($"Notification {notificationId} not found.");

        if (notification.UserId != userId.Value)
            throw new ForbiddenException("Cannot read another user's notification.");

        notification.MarkAsRead();

        await repository.SaveChangesAsync(ct);
    }
}
