using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Application.Common;

/// <summary>
///     알림 응답 DTO.
/// </summary>
public sealed record NotificationResponse(
    long Id,
    string Type,
    string Title,
    string Body,
    bool IsRead,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt)
{
    /// <summary>
    ///     Creates a response from a domain entity.
    /// </summary>
    public static NotificationResponse FromEntity(Notification entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new NotificationResponse(
            entity.Id,
            entity.Type,
            entity.Title,
            entity.Body,
            entity.IsRead,
            entity.CreatedAt,
            entity.ReadAt);
    }
}
