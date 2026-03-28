namespace Hammer.Auction.Application.Analysis;

/// <summary>
///     투자 매력도 점수 결과 (6항목 100점 만점).
/// </summary>
/// <param name="TotalScore">종합 점수 (0~100).</param>
/// <param name="MarketGapScore">시세 갭 점수 (0~25).</param>
/// <param name="PriceTrendScore">가격 추세 점수 (0~20).</param>
/// <param name="DiscountDepthScore">할인 깊이 점수 (0~20).</param>
/// <param name="AppraisalDiscountScore">감정가 할인율 점수 (0~15).</param>
/// <param name="CompetitionScore">경쟁 강도 점수 (0~10, 역비례).</param>
/// <param name="LiquidityScore">유동성 점수 (0~10).</param>
/// <param name="Rating">등급 (최상/상/중/하/최하).</param>
public sealed record InvestmentScoreResult(
    double TotalScore,
    double MarketGapScore,
    double PriceTrendScore,
    double DiscountDepthScore,
    double AppraisalDiscountScore,
    double CompetitionScore,
    double LiquidityScore,
    string Rating);
