using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.UpdateNotificationSettings;

/// <summary>
///     Updates the notification settings for the given user.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class UpdateNotificationSettingsUseCase(INotificationSettingRepository repository)
    : IUpdateNotificationSettingsUseCase
{
    /// <inheritdoc />
    public async Task<NotificationSettingsResponse> ExecuteAsync(
        UserId userId,
        UpdateNotificationSettingsRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        NotificationSetting? setting = await repository.GetByUserIdAsync(userId.Value, ct);

        if (setting is not null)
        {
            setting.Update(request.IsEnabled);
            await repository.SaveChangesAsync(ct);
            return new NotificationSettingsResponse(setting.IsEnabled);
        }

        setting = NotificationSetting.Create(userId, request.IsEnabled);
        repository.Add(setting);
        await repository.SaveChangesAsync(ct);

        return new NotificationSettingsResponse(setting.IsEnabled);
    }
}
