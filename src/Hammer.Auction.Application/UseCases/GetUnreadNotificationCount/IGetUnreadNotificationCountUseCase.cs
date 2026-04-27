using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.GetUnreadNotificationCount;

/// <summary>
///     Use case contract for retrieving the unread notification count.
/// </summary>
public interface IGetUnreadNotificationCountUseCase
{
    /// <summary>
    ///     Returns the number of unread notifications for the given user.
    /// </summary>
    public Task<int> ExecuteAsync(UserId userId, CancellationToken ct = default);
}
