namespace Hammer.Auction.Application.Common;

/// <summary>
///     Generic paged response wrapper.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int Size,
    int TotalCount,
    int TotalPages);
