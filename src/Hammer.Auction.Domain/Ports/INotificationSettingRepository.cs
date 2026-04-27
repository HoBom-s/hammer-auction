using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Domain.Ports;

/// <summary>
///     Repository interface for notification settings.
/// </summary>
public interface INotificationSettingRepository
{
    /// <summary>
    ///     Returns the notification setting for the given user, or null if not found.
    /// </summary>
    public Task<NotificationSetting?> GetByUserIdAsync(string userId, CancellationToken ct = default);

    /// <summary>
    ///     Adds a new notification setting.
    /// </summary>
    public void Add(NotificationSetting setting);

    /// <summary>
    ///     Persists pending changes.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken ct = default);
}
