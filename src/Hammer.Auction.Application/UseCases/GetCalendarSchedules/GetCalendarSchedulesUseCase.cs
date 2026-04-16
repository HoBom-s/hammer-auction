using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetCalendarSchedules;

/// <summary>
/// Retrieves auction schedules for a given month from both KAMCO and institution sources.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetCalendarSchedulesUseCase(
    IKamcoAuctionItemRepository kamcoRepo,
    IInstitutionAuctionItemRepository institutionRepo) : IGetCalendarSchedulesUseCase
{
    private static readonly TimeSpan _kst = TimeSpan.FromHours(9);

    /// <inheritdoc />
    public async Task<CalendarScheduleResponse> ExecuteAsync(
        GetCalendarSchedulesRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Month is < 1 or > 12)
            throw new BadRequestException("Month must be between 1 and 12.");

        if (request.Year is < 2000 or > 2100)
            throw new BadRequestException("Year must be between 2000 and 2100.");

        // Calculate month boundaries in KST, then convert to UTC
        DateTimeOffset monthStartKst = new(request.Year, request.Month, 1, 0, 0, 0, _kst);
        DateTimeOffset monthEndKst = monthStartKst.AddMonths(1);

        DateTimeOffset from = monthStartKst.ToUniversalTime();
        DateTimeOffset to = monthEndKst.ToUniversalTime();

        IReadOnlyList<Domain.Entities.KamcoAuctionItem> kamcoItems =
            await kamcoRepo.GetByDateRangeAsync(from, to, ct);
        IReadOnlyList<Domain.Entities.InstitutionAuctionItem> institutionItems =
            await institutionRepo.GetByDateRangeAsync(from, to, ct);

        var allItems = kamcoItems.Select(CalendarScheduleItem.FromKamco)
            .Concat(institutionItems.Select(CalendarScheduleItem.FromInstitution))
            .ToList();

        // Group by KST date of PbctBegnDtm
        var schedules = allItems
            .GroupBy(item => item.PbctBegnDtm.ToOffset(_kst).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .ToDictionary(g => g.Key, g => (IReadOnlyList<CalendarScheduleItem>)g.ToList());

        return new CalendarScheduleResponse(request.Year, request.Month, schedules);
    }
}
