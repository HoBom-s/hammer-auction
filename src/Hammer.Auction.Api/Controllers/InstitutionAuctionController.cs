using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetInstitutionAuctionItemById;
using Hammer.Auction.Application.UseCases.GetInstitutionAuctionItems;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
///     기관 공매 물건 조회 API.
///     온비드에 등록된 각 기관(캠코 외)의 공매 물건 목록 및 상세 정보를 제공합니다.
/// </summary>
[ApiController]
[Route("institution-auctions/items")]
[Tags("InstitutionAuction")]
[SuppressMessage("SonarAnalyzer.CSharp", "S6960", Justification = "Single resource controller")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
public sealed class InstitutionAuctionController(
    IGetInstitutionAuctionItemsUseCase getItems,
    IGetInstitutionAuctionItemByIdUseCase getItemById) : ControllerBase
{
    /// <summary>
    ///     기관 공매 물건 목록을 페이징 조회합니다.
    /// </summary>
    /// <param name="page">페이지 번호 (1부터 시작, 기본값: 1).</param>
    /// <param name="size">페이지당 항목 수 (기본값: 20, 최대: 100).</param>
    /// <param name="org">공고기관명 필터 (예: 한국자산관리공사). 완전 일치.</param>
    /// <param name="category">용도 필터 (예: 토지, 건물). 부분 일치.</param>
    /// <param name="keyword">키워드 검색 (공고명). 부분 일치.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<InstitutionAuctionItemResponse>>> GetItemsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? org = null,
        [FromQuery] string? category = null,
        [FromQuery] string? keyword = null,
        CancellationToken ct = default)
    {
        GetInstitutionAuctionItemsRequest request = new(page, size, org, category, keyword);
        PagedResponse<InstitutionAuctionItemResponse> result = await getItems.ExecuteAsync(request, ct);

        return Ok(result);
    }

    /// <summary>
    ///     기관 공매 물건 상세 정보를 조회합니다.
    /// </summary>
    /// <param name="id">물건 고유 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InstitutionAuctionItemResponse>> GetItemByIdAsync(long id, CancellationToken ct = default)
    {
        InstitutionAuctionItemResponse result = await getItemById.ExecuteAsync(id, ct);

        return Ok(result);
    }
}
