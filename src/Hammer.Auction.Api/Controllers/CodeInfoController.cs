using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetCodeInfoById;
using Hammer.Auction.Application.UseCases.GetCodeInfos;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
///     온비드 코드 정보 조회 API.
///     공매 물건의 용도 분류 체계(카테고리 트리)를 조회합니다.
///     parentId를 지정하면 해당 코드의 하위 코드만 반환됩니다.
/// </summary>
[ApiController]
[Route("code-infos")]
[Tags("CodeInfo")]
[SuppressMessage("SonarAnalyzer.CSharp", "S6960", Justification = "Single resource controller")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
public sealed class CodeInfoController(
    IGetCodeInfosUseCase getCodeInfos,
    IGetCodeInfoByIdUseCase getCodeInfoById) : ControllerBase
{
    /// <summary>
    ///     코드 정보 목록을 페이징 조회합니다.
    ///     parentId를 지정하면 해당 상위 코드의 하위 코드만 반환됩니다.
    /// </summary>
    /// <param name="page">페이지 번호 (1부터 시작, 기본값: 1).</param>
    /// <param name="size">페이지당 항목 수 (기본값: 100, 최대: 100).</param>
    /// <param name="parentId">상위 코드 ID 필터 (예: ROOT). 완전 일치.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<OnbidCodeInfoResponse>>> GetCodeInfosAsync(
        [FromQuery] int page = 1,
        [FromQuery] int size = 100,
        [FromQuery] string? parentId = null,
        CancellationToken ct = default)
    {
        GetCodeInfosRequest request = new(page, size, parentId);
        PagedResponse<OnbidCodeInfoResponse> result = await getCodeInfos.ExecuteAsync(request, ct);

        return Ok(result);
    }

    /// <summary>
    ///     코드 정보 상세를 조회합니다.
    /// </summary>
    /// <param name="id">코드 고유 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OnbidCodeInfoResponse>> GetCodeInfoByIdAsync(long id, CancellationToken ct = default)
    {
        OnbidCodeInfoResponse result = await getCodeInfoById.ExecuteAsync(id, ct);

        return Ok(result);
    }
}
