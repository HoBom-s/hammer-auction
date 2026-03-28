using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.Analysis;

/// <summary>
///     투자 분석 facade. 3개 모델을 순차 호출하여 <see cref="InvestmentAnalysis" />를 반환합니다.
/// </summary>
internal static class InvestmentAnalyzer
{
    /// <summary>
    ///     경매 물건과 주변 실거래 데이터를 기반으로 투자 분석을 수행.
    /// </summary>
    /// <param name="minBidPrc">최저 입찰가 (원 단위).</param>
    /// <param name="uscbdCnt">유찰 횟수 — 0이면 첫 입찰.</param>
    /// <param name="iqryCnt">조회수 — 관심도(경쟁 강도) 지표.</param>
    /// <param name="ctgrFullNm">카테고리 전체 명칭 — 명도비용률 산출에 사용.</param>
    /// <param name="trades">주변 실거래 내역 — 시세 산출의 원천 데이터.</param>
    /// <param name="referenceDate">분석 기준일 — 시간 가중치 계산의 기준점.</param>
    /// <param name="appraisalAmount">감정가 (원 단위). 0 이하이면 감정가 미사용.</param>
    /// <returns>거래 데이터가 비어 있으면 <c>null</c>, 있으면 3개 모델 분석 결과.</returns>
    public static InvestmentAnalysis? Analyze(
        long minBidPrc,
        int uscbdCnt,
        int iqryCnt,
        string ctgrFullNm,
        IReadOnlyList<RealEstateTradeResponse> trades,
        DateOnly referenceDate,
        long appraisalAmount = 0)
    {
        if (trades.Count == 0)
            return null;

        MarketGapResult marketGap = MarketGapCalculator.Calculate(trades, minBidPrc, referenceDate);

        InvestmentScoreResult investmentScore = InvestmentScoreCalculator.Calculate(
            marketGap, trades, uscbdCnt, iqryCnt, referenceDate, appraisalAmount);

        var evictionCostRate = BidPriceGuideCalculator.GetEvictionCostRate(ctgrFullNm);

        BidPriceGuideResult bidPriceGuide = BidPriceGuideCalculator.Calculate(
            marketGap.WeightedMarketPrice, evictionCostRate, uscbdCnt, appraisalAmount, minBidPrc);

        return new InvestmentAnalysis(marketGap, investmentScore, bidPriceGuide);
    }
}
