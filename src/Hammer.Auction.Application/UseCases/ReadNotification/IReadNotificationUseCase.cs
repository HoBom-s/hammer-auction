using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.ReadNotification;

/// <summary>
///     Use case contract for marking a single notification as read.
/// </summary>
public interface IReadNotificationUseCase
{
    /// <summary>
    ///     Marks the specified notification as read.
    /// </summary>
    public Task ExecuteAsync(UserId userId, long notificationId, CancellationToken ct = default);
}
