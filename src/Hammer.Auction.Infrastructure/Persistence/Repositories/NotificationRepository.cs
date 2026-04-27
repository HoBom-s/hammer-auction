using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Auction.Infrastructure.Persistence.Repositories;

/// <summary>
///     Repository implementation for notification records.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class NotificationRepository(AuctionDbContext db) : INotificationRepository
{
    /// <inheritdoc />
    public void Add(Notification notification) =>
        db.Notifications.Add(notification);

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await db.SaveChangesAsync(ct);

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetByUserIdAsync(
        string userId,
        int page,
        int size,
        CancellationToken ct = default)
    {
        IQueryable<Notification> query = db.Notifications.Where(n => n.UserId == userId);

        var totalCount = await query.CountAsync(ct);

        List<Notification> items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default)
    {
        return await db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    /// <inheritdoc />
    public async Task<Notification?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await db.Notifications.FindAsync([id], ct);
    }

    /// <inheritdoc />
    public async Task MarkAllAsReadAsync(string userId, CancellationToken ct = default)
    {
        await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, DateTimeOffset.UtcNow),
                ct);
    }
}
