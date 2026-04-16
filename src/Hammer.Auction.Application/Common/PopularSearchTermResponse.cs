namespace Hammer.Auction.Application.Common;

/// <summary>
///     Response DTO for a popular search term with its count.
/// </summary>
/// <param name="Keyword">검색어.</param>
/// <param name="Count">검색 횟수.</param>
public sealed record PopularSearchTermResponse(
    string Keyword,
    int Count);
