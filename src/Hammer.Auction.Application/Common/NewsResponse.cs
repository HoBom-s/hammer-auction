namespace Hammer.Auction.Application.Common;

/// <summary>
/// 뉴스 응답 DTO. hammer-support에서 조회한 뉴스 기사를 표현한다.
/// </summary>
/// <param name="Id">기사 고유 식별자.</param>
/// <param name="Query">수집 키워드 (예: 경매, 부동산).</param>
/// <param name="Title">기사 제목 (HTML 제거됨).</param>
/// <param name="OriginalLink">원문 기사 URL.</param>
/// <param name="Link">네이버 캐시 URL.</param>
/// <param name="Description">기사 요약 (HTML 제거됨).</param>
/// <param name="PubDate">기사 발행일시.</param>
public sealed record NewsResponse(
    Guid Id,
    string Query,
    string Title,
    string OriginalLink,
    string Link,
    string Description,
    DateTimeOffset PubDate);
