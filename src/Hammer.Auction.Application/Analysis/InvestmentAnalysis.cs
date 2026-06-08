namespace Hammer.Auction.Application.Analysis;

/// <summary>
///     투자 분석 결과를 담는 최상위 DTO.
/// </summary>
/// <param name="MarketGap">시세 갭 분석 결과.</param>
/// <param name="InvestmentScore">투자 매력도 점수.</param>
/// <param name="BidPriceGuide">적정 입찰가 가이드.</param>
public sealed record InvestmentAnalysis(
    MarketGapResult? MarketGap,
    InvestmentScoreResult? InvestmentScore,
    BidPriceGuideResult? BidPriceGuide);
