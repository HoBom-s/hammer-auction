using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetDashboardSummary;

/// <summary>
/// Computes dashboard summary: per-category totals and daily change (today's new − yesterday's new).
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetDashboardSummaryUseCase(
    IKamcoAuctionItemRepository repository) : IGetDashboardSummaryUseCase
{
    private static readonly TimeSpan _kst = TimeSpan.FromHours(9);

    /// <inheritdoc />
    public async Task<DashboardSummaryResponse> ExecuteAsync(CancellationToken ct = default)
    {
        DateTimeOffset nowKst = DateTimeOffset.UtcNow.ToOffset(_kst);
        DateTimeOffset todayStart = new DateTimeOffset(nowKst.Date, _kst).ToUniversalTime();
        DateTimeOffset yesterdayStart = todayStart.AddDays(-1);

        IReadOnlyList<(string CtgrFullNm, int Count)> totalCounts =
            await repository.CountByCtgrFullNmAsync(null, null, ct);

        IReadOnlyList<(string CtgrFullNm, int Count)> todayCounts =
            await repository.CountByCtgrFullNmAsync(todayStart, todayStart.AddDays(1), ct);

        IReadOnlyList<(string CtgrFullNm, int Count)> yesterdayCounts =
            await repository.CountByCtgrFullNmAsync(yesterdayStart, todayStart, ct);

        Dictionary<PropertyCategory, int> totals = Aggregate(totalCounts);
        Dictionary<PropertyCategory, int> todayNew = Aggregate(todayCounts);
        Dictionary<PropertyCategory, int> yesterdayNew = Aggregate(yesterdayCounts);

        var categories = Enum.GetValues<PropertyCategory>()
            .Select(cat =>
            {
                var total = totals.GetValueOrDefault(cat);
                var daily = todayNew.GetValueOrDefault(cat) - yesterdayNew.GetValueOrDefault(cat);
                return new DashboardCategorySummary(cat.ToString(), total, daily);
            })
            .ToList();

        return new DashboardSummaryResponse(categories);
    }

    private static Dictionary<PropertyCategory, int> Aggregate(
        IReadOnlyList<(string CtgrFullNm, int Count)> rows)
    {
        var result = new Dictionary<PropertyCategory, int>();

        foreach ((var ctgrFullNm, var count) in rows)
        {
            PropertyCategory cat = PropertyCategoryClassifier.Classify(ctgrFullNm);
            result[cat] = result.GetValueOrDefault(cat) + count;
        }

        return result;
    }
}
