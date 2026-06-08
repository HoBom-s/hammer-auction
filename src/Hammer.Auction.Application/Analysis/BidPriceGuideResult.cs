namespace Hammer.Auction.Application.Analysis;

/// <summary>
///     적정 입찰가 가이드 결과.
/// </summary>
/// <param name="ConservativeBid">보수적 입찰가 (수익률 20%).</param>
/// <param name="ModerateBid">적정 입찰가 (수익률 10%).</param>
/// <param name="AggressiveBid">공격적 입찰가 (수익률 5%).</param>
/// <param name="AcquisitionTaxRate">취득세율 (%).</param>
/// <param name="EvictionCostRate">명도비율 (%).</param>
/// <param name="MiscCostRate">기타비용률 (%).</param>
/// <param name="AppraisalDiscountRate">감정가 대비 할인율 (%).</param>
/// <param name="Guidance">유찰횟수 기반 가이드 텍스트.</param>
public sealed record BidPriceGuideResult(
    long ConservativeBid,
    long ModerateBid,
    long AggressiveBid,
    double AcquisitionTaxRate,
    double EvictionCostRate,
    double MiscCostRate,
    double AppraisalDiscountRate,
    string Guidance);
