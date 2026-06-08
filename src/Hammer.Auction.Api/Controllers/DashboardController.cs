using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetDashboardSummary;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
///     대시보드 요약 API.
///     부동산/동산/기타 카테고리별 전체 건수와 일일 변동을 제공합니다.
/// </summary>
[ApiController]
[Route("dashboard")]
[Tags("Dashboard")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
public sealed class DashboardController(
    IGetDashboardSummaryUseCase getDashboardSummary) : ControllerBase
{
    /// <summary>
    ///     카테고리별 전체 건수 및 일일 변동(오늘 신규 − 어제 신규)을 조회합니다.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummaryAsync(CancellationToken ct = default)
    {
        DashboardSummaryResponse result = await getDashboardSummary.ExecuteAsync(ct);

        return Ok(result);
    }
}
