using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.ReadAllNotifications;

/// <summary>
///     Use case contract for marking all notifications as read.
/// </summary>
public interface IReadAllNotificationsUseCase
{
    /// <summary>
    ///     Marks all unread notifications as read for the given user.
    /// </summary>
    public Task ExecuteAsync(UserId userId, CancellationToken ct = default);
}
