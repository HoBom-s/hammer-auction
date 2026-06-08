namespace Hammer.Auction.Application.UseCases.UpdateNotificationSettings;

/// <summary>
///     Request to update notification settings.
/// </summary>
public sealed record UpdateNotificationSettingsRequest(bool IsEnabled);
