using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.GetDashboardSummary;

/// <summary>
/// Use case contract for retrieving the dashboard summary.
/// </summary>
public interface IGetDashboardSummaryUseCase
{
    /// <summary>
    /// Retrieves category-level totals and daily changes.
    /// </summary>
    public Task<DashboardSummaryResponse> ExecuteAsync(CancellationToken ct = default);
}
