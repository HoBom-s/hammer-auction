using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.GetNotificationSettings;

/// <summary>
///     Returns the notification settings for the given user.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetNotificationSettingsUseCase(INotificationSettingRepository repository)
    : IGetNotificationSettingsUseCase
{
    /// <inheritdoc />
    public async Task<NotificationSettingsResponse> ExecuteAsync(UserId userId, CancellationToken ct = default)
    {
        NotificationSetting? setting = await repository.GetByUserIdAsync(userId.Value, ct);

        return new NotificationSettingsResponse(setting?.IsEnabled ?? true);
    }
}
