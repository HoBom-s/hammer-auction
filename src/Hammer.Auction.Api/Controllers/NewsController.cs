using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetNewsById;
using Hammer.Auction.Application.UseCases.GetNewsList;
using Hammer.Auction.Application.UseCases.GetRecentNews;
using Hammer.Auction.Application.UseCases.SearchNews;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
///     뉴스 API.
///     hammer-support에서 수집한 경매·부동산 뉴스를 조회합니다.
/// </summary>
[ApiController]
[Route("news")]
[Tags("News")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
[SuppressMessage("SonarAnalyzer.CSharp", "S6960", Justification = "Single resource controller")]
public sealed class NewsController(
    IGetRecentNewsUseCase getRecentNews,
    IGetNewsByIdUseCase getNewsById,
    IGetNewsListUseCase getNewsList,
    ISearchNewsUseCase searchNews) : ControllerBase
{
    /// <summary>
    ///     뉴스 목록을 페이지네이션 조회합니다 (최신순).
    /// </summary>
    /// <param name="page">페이지 번호 (기본값: 1).</param>
    /// <param name="size">페이지 크기 (기본값: 20, 최대: 100).</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType<PagedResponse<NewsResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<NewsResponse>>> GetListAsync(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        PagedResponse<NewsResponse> result = await getNewsList.ExecuteAsync(page, size, ct);

        return Ok(result);
    }

    /// <summary>
    ///     최신 뉴스 목록을 조회합니다.
    /// </summary>
    /// <param name="count">조회할 기사 수 (기본값: 5, 최대: 20).</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("recent")]
    [ProducesResponseType<IReadOnlyList<NewsResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NewsResponse>>> GetRecentAsync(
        [FromQuery] int count = 5,
        CancellationToken ct = default)
    {
        IReadOnlyList<NewsResponse> result = await getRecentNews.ExecuteAsync(count, ct);

        return Ok(result);
    }

    /// <summary>
    ///     뉴스 상세를 조회합니다.
    /// </summary>
    /// <param name="id">기사 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<NewsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NewsResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        NewsResponse result = await getNewsById.ExecuteAsync(id, ct);

        return Ok(result);
    }

    /// <summary>
    ///     제목에 키워드가 포함된 뉴스를 검색합니다 (최신순).
    /// </summary>
    /// <param name="keyword">제목 검색 키워드.</param>
    /// <param name="page">페이지 번호 (기본값: 1).</param>
    /// <param name="size">페이지 크기 (기본값: 20, 최대: 100).</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("search")]
    [ProducesResponseType<PagedResponse<NewsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<NewsResponse>>> SearchAsync(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        PagedResponse<NewsResponse> result = await searchNews.ExecuteAsync(keyword, page, size, ct);

        return Ok(result);
    }
}
