namespace Hammer.Auction.Application.Common;

/// <summary>
///     Generic paged response wrapper.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
/// <param name="Items">조회된 항목 목록.</param>
/// <param name="Page">현재 페이지 번호 (1부터 시작).</param>
/// <param name="Size">페이지당 항목 수.</param>
/// <param name="TotalCount">전체 항목 수.</param>
/// <param name="TotalPages">전체 페이지 수.</param>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int Size,
    int TotalCount,
    int TotalPages);
