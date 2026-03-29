using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetAuctionItemById;
using Hammer.Auction.Application.UseCases.GetAuctionItems;
using Hammer.Auction.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
///     KAMCO (한국자산관리공사) 공매 물건 조회 API.
///     캠코 온비드에 등록된 공매 물건의 목록 및 상세 정보를 제공합니다.
///     목록 조회 시 인근 국토부 실거래가가 함께 반환됩니다.
/// </summary>
[ApiController]
[Route("hammer-auctions/items")]
[Tags("Auction")]
[SuppressMessage("SonarAnalyzer.CSharp", "S6960", Justification = "Single resource controller")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
public sealed class AuctionController(
    IGetAuctionItemsUseCase getAuctionItems,
    IGetAuctionItemByIdUseCase getAuctionItemById) : ControllerBase
{
    /// <summary>
    ///     공매 물건 목록을 페이징 조회합니다.
    ///     각 물건에 지번주소 기반 최근 국토부 실거래가(LatestTradeAmount, LatestTradeDate)가 포함됩니다.
    /// </summary>
    /// <param name="page">페이지 번호 (1부터 시작, 기본값: 1).</param>
    /// <param name="size">페이지당 항목 수 (기본값: 20, 최대: 100).</param>
    /// <param name="status">물건 상태 필터 (예: 입찰진행중, 낙찰).</param>
    /// <param name="category">용도 필터 (예: 토지, 건물). 부분 일치.</param>
    /// <param name="keyword">키워드 검색 (물건명, 지번주소, 도로명주소). 부분 일치.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<KamcoAuctionItemResponse>>> GetItemsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? category = null,
        [FromQuery] string? keyword = null,
        CancellationToken ct = default)
    {
        GetAuctionItemsRequest request = new(page, size, status, category, keyword);
        PagedResponse<KamcoAuctionItemResponse> result = await getAuctionItems.ExecuteAsync(request, ct);

        return Ok(result);
    }

    /// <summary>
    ///     공매 물건 상세 정보를 조회합니다.
    ///     인근 국토부 실거래 내역(RecentTrades)이 최대 20건까지 포함됩니다.
    /// </summary>
    /// <param name="id">물건 고유 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("{id}")]
    [ProducesResponseType<KamcoAuctionItemResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KamcoAuctionItemResponse>> GetItemByIdAsync(KamcoAuctionItemId id, CancellationToken ct = default)
    {
        KamcoAuctionItemResponse result = await getAuctionItemById.ExecuteAsync(id, ct);

        return Ok(result);
    }
}
