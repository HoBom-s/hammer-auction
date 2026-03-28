namespace Hammer.Auction.Application.Analysis;

/// <summary>
///     시세 갭 분석 결과.
/// </summary>
/// <param name="WeightedMarketPrice">시간가중 평균 시세 (원).</param>
/// <param name="MinBidPrice">최저입찰가 (원).</param>
/// <param name="GapRate">시세갭률 (%).</param>
/// <param name="Grade">등급 (S/A/B/C/D).</param>
/// <param name="TradeCount">원본 거래 건수.</param>
/// <param name="UsedTradeCount">이상치 제거 후 사용된 거래 건수.</param>
/// <param name="Confidence">분석 신뢰도 (높음/보통/낮음).</param>
public sealed record MarketGapResult(
    long WeightedMarketPrice,
    long MinBidPrice,
    double GapRate,
    string Grade,
    int TradeCount,
    int UsedTradeCount,
    string Confidence);
