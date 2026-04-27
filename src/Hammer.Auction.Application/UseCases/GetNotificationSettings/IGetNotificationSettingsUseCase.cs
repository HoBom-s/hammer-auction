using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.GetNotificationSettings;

/// <summary>
///     Use case contract for retrieving notification settings.
/// </summary>
public interface IGetNotificationSettingsUseCase
{
    /// <summary>
    ///     Returns the notification settings for the given user.
    /// </summary>
    public Task<NotificationSettingsResponse> ExecuteAsync(UserId userId, CancellationToken ct = default);
}
