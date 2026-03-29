namespace Hammer.Auction.Application.Common;

/// <summary>
/// 카테고리별 전체 건수 및 일일 변동.
/// </summary>
public sealed record DashboardCategorySummary(string Category, int TotalCount, int DailyChange);
