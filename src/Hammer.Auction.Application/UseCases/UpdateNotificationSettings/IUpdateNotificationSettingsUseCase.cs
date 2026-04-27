using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.UpdateNotificationSettings;

/// <summary>
///     Use case contract for updating notification settings.
/// </summary>
public interface IUpdateNotificationSettingsUseCase
{
    /// <summary>
    ///     Updates the notification settings for the given user.
    /// </summary>
    public Task<NotificationSettingsResponse> ExecuteAsync(
        UserId userId,
        UpdateNotificationSettingsRequest request,
        CancellationToken ct = default);
}
