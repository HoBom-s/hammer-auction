using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Domain.Entities;

/// <summary>
///     사용자 알림 설정.
/// </summary>
public sealed class NotificationSetting
{
    private NotificationSetting()
    {
    }

    /// <summary>
    ///     Gets the surrogate primary key.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    ///     Gets the user identifier.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets a value indicating whether notifications are enabled.
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    ///     Gets the timestamp of the last update.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    ///     Creates a new notification setting record.
    /// </summary>
    public static NotificationSetting Create(UserId userId, bool isEnabled)
    {
        return new NotificationSetting
        {
            UserId = userId.Value,
            IsEnabled = isEnabled,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    ///     Updates the enabled state.
    /// </summary>
    public void Update(bool isEnabled)
    {
        IsEnabled = isEnabled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
