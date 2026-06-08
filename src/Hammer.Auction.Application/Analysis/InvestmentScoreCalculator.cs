using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.Analysis;

/// <summary>
///     Model 2 — 투자 매력도 점수 (100점 만점).
///     6개 세부 항목(시세갭·추세·유찰깊이·감정가할인·경쟁도·유동성)을 합산하여 종합 점수 및 등급을 부여합니다.
/// </summary>
internal static class InvestmentScoreCalculator
{
    /// <summary>
    ///     투자 매력도 점수를 계산합니다.
    /// </summary>
    /// <param name="marketGap">시세 갭 분석 결과 (Model 1).</param>
    /// <param name="trades">주변 실거래 내역.</param>
    /// <param name="uscbdCnt">유찰 횟수.</param>
    /// <param name="iqryCnt">조회수.</param>
    /// <param name="referenceDate">분석 기준일.</param>
    /// <param name="appraisalAmount">감정가 (원 단위). 0 이하이면 중립 점수 적용.</param>
    /// <returns>투자 매력도 점수 결과.</returns>
    public static InvestmentScoreResult Calculate(
        MarketGapResult marketGap,
        IReadOnlyList<RealEstateTradeResponse> trades,
        int uscbdCnt,
        int iqryCnt,
        DateOnly referenceDate,
        long appraisalAmount = 0)
    {
        // (1) 시세 갭 점수 (0~25점): 갭 등급 → 점수 매핑
        var marketGapScore = MarketGapScoreFromGrade(marketGap.Grade);

        // (2) 가격 추세 점수 (0~20점): 최근 6개월 vs 이전 6개월 평균가 비교
        var priceTrendScore = CalculatePriceTrendScore(trades, referenceDate);

        // (3) 유찰 깊이 점수 (0~20점): 유찰 1회당 5점, 최대 20점 (4회 이상은 동일)
        var discountDepthScore = Math.Min(uscbdCnt * 5.0, 20);

        // (4) 감정가 할인율 점수 (0~15점): 감정가 대비 최저입찰가 할인 폭
        var appraisalDiscountScore = CalculateAppraisalDiscountScore(appraisalAmount, marketGap.MinBidPrice);

        // (5) 경쟁도 점수 (1~10점): 조회수 적을수록 높은 점수 (낮은 관심 = 낮은 경쟁)
        var competitionScore = CompetitionScoreFromInquiry(iqryCnt);

        // (6) 유동성 점수 (0~10점): 최근 12개월 거래 건수 × 2, 최대 10점
        var liquidityScore = CalculateLiquidityScore(trades, referenceDate);

        // 6개 항목 합산 후 소수점 1자리 반올림
        var totalScore = Math.Round(
            marketGapScore + priceTrendScore + discountDepthScore
                + appraisalDiscountScore + competitionScore + liquidityScore,
            1);

        // 총점에 따른 등급 부여 (80이상 최상, 60이상 상, 40이상 중, 20이상 하, 나머지 최하)
        var rating = RatingFromTotalScore(totalScore);

        return new InvestmentScoreResult(
            totalScore,
            marketGapScore,
            priceTrendScore,
            discountDepthScore,
            appraisalDiscountScore,
            competitionScore,
            liquidityScore,
            rating);
    }

    /// <summary>
    ///     시세 갭 등급 → 투자 매력도 점수 변환 (최대 25점).
    /// </summary>
    internal static double MarketGapScoreFromGrade(string grade) => grade switch
    {
        "S" => 25, // 초저가 물건 → 만점
        "A" => 20, // 우수 갭 → 80%
        "B" => 15, // 양호 → 60%
        "C" => 10, // 보통 → 40%
        _ => 5, // D등급(미달) → 최소 점수
    };

    /// <summary>
    ///     가격 추세 점수 계산 (최대 20점).
    ///     최근 6개월 평균가와 이전 6개월(6~12개월) 평균가를 비교하여
    ///     상승 추세면 높은 점수, 하락 추세면 낮은 점수를 부여합니다.
    /// </summary>
    internal static double CalculatePriceTrendScore(
        IReadOnlyList<RealEstateTradeResponse> trades,
        DateOnly referenceDate)
    {
        // 비교 기준점 산출: 기준일로부터 6개월 전, 12개월 전
        DateOnly sixMonthsAgo = referenceDate.AddMonths(-6);
        DateOnly twelveMonthsAgo = referenceDate.AddMonths(-12);

        // 최근 구간(0~6개월)과 이전 구간(6~12개월) 거래를 분류
        List<long> recent = [];
        List<long> previous = [];

        foreach (RealEstateTradeResponse trade in trades)
        {
            DateOnly tradeDate = new(trade.DealYear, trade.DealMonth, trade.DealDay);

            // 6개월 이내 거래 → 최근 구간
            if (tradeDate >= sixMonthsAgo)
                recent.Add(trade.DealAmount);

            // 6~12개월 거래 → 이전 구간
            else if (tradeDate >= twelveMonthsAgo)
                previous.Add(trade.DealAmount);

            // 12개월 이상은 추세 비교에서 제외 (너무 오래된 데이터)
        }

        // 어느 한쪽 구간에 데이터가 없으면 추세 판단 불가 → 중립 점수(10.0) 반환
        if (recent.Count == 0 || previous.Count == 0)
            return 10.0;

        // 두 구간의 평균 거래가 산출
        var recentAvg = recent.Average();
        var previousAvg = previous.Average();

        // 변화율 = (최근 평균 - 이전 평균) / 이전 평균
        // 양수면 상승 추세, 음수면 하락 추세
        var changeRate = (recentAvg - previousAvg) / previousAvg;

        // 선형 매핑: 변화율 ±20%를 0~20점 범위로 변환
        // changeRate = -0.20 → score = 0, changeRate = 0 → score = 10, changeRate = +0.20 → score = 20
        var trendFactor = changeRate / 0.20;
        var trendScore = trendFactor * 10.0;
        var score = 10.0 + trendScore;

        // 0~20점 범위로 클램핑 후 소수점 1자리 반올림
        return Math.Round(Math.Clamp(score, 0, 20), 1);
    }

    /// <summary>
    ///     감정가 할인율 점수 계산 (최대 15점).
    ///     감정가 대비 최저입찰가의 할인 폭이 클수록 높은 점수.
    /// </summary>
    internal static double CalculateAppraisalDiscountScore(long appraisalAmount, long minBidPrice)
    {
        if (appraisalAmount <= 0)
            return 7.5;

        var discountRate = Math.Round((1.0 - ((double)minBidPrice / appraisalAmount)) * 100, 2);

        return discountRate switch
        {
            >= 50 => 15,
            >= 35 => 12,
            >= 20 => 9,
            >= 10 => 6,
            _ => 3,
        };
    }

    /// <summary>
    ///     조회수 → 경쟁도 점수 변환 (최대 10점).
    ///     조회수가 적을수록 경쟁이 낮아 유리하므로 높은 점수.
    /// </summary>
    internal static double CompetitionScoreFromInquiry(int iqryCnt) => iqryCnt switch
    {
        < 50 => 10, // 50건 미만: 관심 매우 낮음 → 만점
        < 100 => 8, // 50~100건: 관심 낮음
        < 200 => 6, // 100~200건: 보통
        < 500 => 4, // 200~500건: 관심 높음
        < 1000 => 2, // 500~1000건: 경쟁 치열
        _ => 1, // 1000건 이상: 과열 → 최소 점수
    };

    /// <summary>
    ///     유동성 점수 계산 (최대 10점).
    ///     최근 12개월 내 거래 건수가 많을수록 환금성이 높다고 판단합니다.
    /// </summary>
    internal static double CalculateLiquidityScore(
        IReadOnlyList<RealEstateTradeResponse> trades,
        DateOnly referenceDate)
    {
        // 12개월 이내 거래만 유동성 판단에 사용
        DateOnly twelveMonthsAgo = referenceDate.AddMonths(-12);

        var recentTradeCount = 0;

        foreach (RealEstateTradeResponse trade in trades)
        {
            DateOnly tradeDate = new(trade.DealYear, trade.DealMonth, trade.DealDay);

            // 12개월 이내 거래이면 카운트 증가
            if (tradeDate >= twelveMonthsAgo)
                recentTradeCount++;
        }

        // 거래 1건당 2점, 최대 10점 (5건 이상이면 만점)
        return Math.Min(recentTradeCount * 2.0, 10);
    }

    /// <summary>
    ///     총점 → 투자 등급 변환.
    ///     100점 만점 기준 5단계로 분류합니다.
    /// </summary>
    internal static string RatingFromTotalScore(double totalScore) => totalScore switch
    {
        >= 80 => "최상", // 80점 이상: 매우 우수한 투자 기회
        >= 60 => "상", // 60~80점: 좋은 기회
        >= 40 => "중", // 40~60점: 보통
        >= 20 => "하", // 20~40점: 주의 필요
        _ => "최하", // 20점 미만: 투자 매력 낮음
    };
}
