using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Auction.Infrastructure.Persistence.Repositories;

/// <summary>
///     Repository implementation for notification settings.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class NotificationSettingRepository(AuctionDbContext db) : INotificationSettingRepository
{
    /// <inheritdoc />
    public async Task<NotificationSetting?> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await db.NotificationSettings.FirstOrDefaultAsync(s => s.UserId == userId, ct);
    }

    /// <inheritdoc />
    public void Add(NotificationSetting setting) =>
        db.NotificationSettings.Add(setting);

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await db.SaveChangesAsync(ct);
}
