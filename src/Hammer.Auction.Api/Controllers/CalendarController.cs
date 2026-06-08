using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetCalendarSchedules;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
///     달력 기반 경매 일정 API.
///     월별 KAMCO + 기관 공매 일정을 날짜별로 그룹핑하여 제공합니다.
/// </summary>
[ApiController]
[Route("calendar")]
[Tags("Calendar")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
public sealed class CalendarController(
    IGetCalendarSchedulesUseCase getCalendarSchedules) : ControllerBase
{
    /// <summary>
    ///     월별 경매 일정을 조회합니다.
    ///     KST 기준 날짜별로 그룹핑된 일정이 반환됩니다.
    /// </summary>
    /// <param name="year">조회 연도.</param>
    /// <param name="month">조회 월 (1-12).</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("schedules")]
    public async Task<ActionResult<CalendarScheduleResponse>> GetSchedulesAsync(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct = default)
    {
        GetCalendarSchedulesRequest request = new(year, month);
        CalendarScheduleResponse result = await getCalendarSchedules.ExecuteAsync(request, ct);

        return Ok(result);
    }
}
