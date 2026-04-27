using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Domain.Ports;

/// <summary>
///     Repository interface for notification records.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    ///     Adds a new notification.
    /// </summary>
    public void Add(Notification notification);

    /// <summary>
    ///     Persists pending changes.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    ///     Returns a paginated list of notifications for the given user.
    /// </summary>
    public Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetByUserIdAsync(
        string userId,
        int page,
        int size,
        CancellationToken ct = default);

    /// <summary>
    ///     Returns the count of unread notifications for the given user.
    /// </summary>
    public Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);

    /// <summary>
    ///     Returns a notification by its ID.
    /// </summary>
    public Task<Notification?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    ///     Marks all unread notifications as read for the given user.
    /// </summary>
    public Task MarkAllAsReadAsync(string userId, CancellationToken ct = default);
}
