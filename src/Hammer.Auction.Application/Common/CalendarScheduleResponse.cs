namespace Hammer.Auction.Application.Common;

/// <summary>
///     Response DTO for monthly calendar auction schedules.
/// </summary>
/// <param name="Year">조회 연도.</param>
/// <param name="Month">조회 월.</param>
/// <param name="Schedules">날짜별 일정 목록 (키: yyyy-MM-dd, KST 기준).</param>
public sealed record CalendarScheduleResponse(
    int Year,
    int Month,
    IReadOnlyDictionary<string, IReadOnlyList<CalendarScheduleItem>> Schedules);
