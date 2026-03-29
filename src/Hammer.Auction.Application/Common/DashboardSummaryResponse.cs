namespace Hammer.Auction.Application.Common;

/// <summary>
/// 대시보드 카테고리별 요약 응답.
/// </summary>
public sealed record DashboardSummaryResponse(IReadOnlyList<DashboardCategorySummary> Categories);
