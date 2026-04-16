using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.GetCalendarSchedules;

/// <summary>
/// Use case contract for retrieving monthly calendar auction schedules.
/// </summary>
public interface IGetCalendarSchedulesUseCase
{
    /// <summary>
    /// Retrieves auction schedules for the given month.
    /// </summary>
    public Task<CalendarScheduleResponse> ExecuteAsync(
        GetCalendarSchedulesRequest request,
        CancellationToken ct = default);
}
