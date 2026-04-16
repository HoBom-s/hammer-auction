using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Api.ModelBinding;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.DeleteSearchHistory;
using Hammer.Auction.Application.UseCases.GetPopularSearchTerms;
using Hammer.Auction.Application.UseCases.GetRecentSearchTerms;
using Hammer.Auction.Application.UseCases.SearchAuctions;
using Hammer.Auction.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
///     통합 검색 API.
///     KAMCO + 기관 공매 통합 검색, 인기 검색어, 최근 검색어 관리를 제공합니다.
/// </summary>
[ApiController]
[Route("search")]
[Tags("Search")]
[SuppressMessage("SonarAnalyzer.CSharp", "S6960", Justification = "Search resource controller")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
public sealed class SearchController(
    ISearchAuctionsUseCase searchAuctions,
    IGetPopularSearchTermsUseCase getPopularSearchTerms,
    IGetRecentSearchTermsUseCase getRecentSearchTerms,
    IDeleteSearchHistoryUseCase deleteSearchHistory) : ControllerBase
{
    /// <summary>
    ///     통합 경매 물건 검색.
    ///     KAMCO + 기관 공매를 키워드로 검색하고 검색 기록을 저장합니다.
    /// </summary>
    /// <param name="keyword">검색 키워드.</param>
    /// <param name="page">페이지 번호 (1부터 시작, 기본값: 1).</param>
    /// <param name="size">페이지당 항목 수 (기본값: 20, 최대: 100).</param>
    /// <param name="userId">Gateway가 X-User-Id 헤더로 전달한 사용자 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("auctions")]
    public async Task<ActionResult<PagedResponse<UnifiedAuctionItemResponse>>> SearchAuctionsAsync(
        [FromQuery] string? keyword,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [CurrentUserId] UserId userId = default,
        CancellationToken ct = default)
    {
        SearchAuctionsRequest request = new(keyword, page, size);
        PagedResponse<UnifiedAuctionItemResponse> result =
            await searchAuctions.ExecuteAsync(request, userId.Value, ct);

        return Ok(result);
    }

    /// <summary>
    ///     글로벌 인기 검색어를 조회합니다.
    /// </summary>
    /// <param name="days">집계 기간 (일, 기본값: 7).</param>
    /// <param name="limit">반환 개수 (기본값: 10).</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("popular")]
    public async Task<ActionResult<IReadOnlyList<PopularSearchTermResponse>>> GetPopularAsync(
        [FromQuery] int days = 7,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        GetPopularSearchTermsRequest request = new(days, limit);
        IReadOnlyList<PopularSearchTermResponse> result =
            await getPopularSearchTerms.ExecuteAsync(request, ct);

        return Ok(result);
    }

    /// <summary>
    ///     내 최근 검색어를 조회합니다.
    /// </summary>
    /// <param name="limit">반환 개수 (기본값: 10).</param>
    /// <param name="userId">Gateway가 X-User-Id 헤더로 전달한 사용자 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("recent")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetRecentAsync(
        [FromQuery] int limit = 10,
        [CurrentUserId] UserId userId = default,
        CancellationToken ct = default)
    {
        GetRecentSearchTermsRequest request = new(userId.Value, limit);
        IReadOnlyList<string> result = await getRecentSearchTerms.ExecuteAsync(request, ct);

        return Ok(result);
    }

    /// <summary>
    ///     내 검색 기록을 전체 삭제합니다.
    /// </summary>
    /// <param name="userId">Gateway가 X-User-Id 헤더로 전달한 사용자 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpDelete("recent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteRecentAsync(
        [CurrentUserId] UserId userId = default,
        CancellationToken ct = default)
    {
        await deleteSearchHistory.ExecuteAsync(userId.Value, ct);

        return NoContent();
    }
}
