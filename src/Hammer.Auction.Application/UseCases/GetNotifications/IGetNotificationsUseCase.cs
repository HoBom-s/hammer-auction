using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.GetNotifications;

/// <summary>
///     Use case contract for retrieving paginated notifications.
/// </summary>
public interface IGetNotificationsUseCase
{
    /// <summary>
    ///     Retrieves a paginated list of notifications for the given user.
    /// </summary>
    public Task<PagedResponse<NotificationResponse>> ExecuteAsync(
        UserId userId,
        GetNotificationsRequest request,
        CancellationToken ct = default);
}
