using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Domain.Entities;

/// <summary>
///     사용자 알림 기록.
/// </summary>
public sealed class Notification
{
    private Notification()
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
    ///     Gets the notification type (e.g. "quiz_result").
    /// </summary>
    public string Type { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the notification title.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the notification body.
    /// </summary>
    public string Body { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets a value indicating whether the notification has been read.
    /// </summary>
    public bool IsRead { get; private set; }

    /// <summary>
    ///     Gets the timestamp when the notification was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    ///     Gets the timestamp when the notification was read.
    /// </summary>
    public DateTimeOffset? ReadAt { get; private set; }

    /// <summary>
    ///     Creates a new notification record.
    /// </summary>
    public static Notification Create(UserId userId, string type, string title, string body)
    {
        return new Notification
        {
            UserId = userId.Value,
            Type = type,
            Title = title,
            Body = body,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    ///     Marks this notification as read.
    /// </summary>
    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
    }
}
