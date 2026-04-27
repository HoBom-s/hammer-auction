using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.GetNotifications;

/// <summary>
///     Retrieves a paginated list of notifications for the given user.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetNotificationsUseCase(INotificationRepository repository) : IGetNotificationsUseCase
{
    /// <inheritdoc />
    public async Task<PagedResponse<NotificationResponse>> ExecuteAsync(
        UserId userId,
        GetNotificationsRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        PageHelper.Validate(request.Page, request.Size);

        (IReadOnlyList<Notification>, int) result = await repository.GetByUserIdAsync(
            userId.Value,
            request.Page,
            request.Size,
            ct);

        var responses = result.Item1.Select(NotificationResponse.FromEntity).ToList();
        var totalPages = PageHelper.CalculateTotalPages(result.Item2, request.Size);

        return new PagedResponse<NotificationResponse>(
            responses,
            request.Page,
            request.Size,
            result.Item2,
            totalPages);
    }
}
