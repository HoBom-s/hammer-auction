namespace Hammer.Auction.Application.UseCases.GetCalendarSchedules;

/// <summary>
/// Request parameters for monthly calendar schedules.
/// </summary>
public sealed record GetCalendarSchedulesRequest(
    int Year,
    int Month);
