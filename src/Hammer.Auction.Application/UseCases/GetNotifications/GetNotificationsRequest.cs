namespace Hammer.Auction.Application.UseCases.GetNotifications;

/// <summary>
///     Request parameters for paginated notification listing.
/// </summary>
public sealed record GetNotificationsRequest(
    int Page = 1,
    int Size = 20);
