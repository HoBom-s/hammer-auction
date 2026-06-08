using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.Analysis;

/// <summary>
///     Model 1 — 시간가중 시세 갭 분석.
///     실거래가에 시간 가중치를 적용하여 추정 시세를 산출하고, 최저입찰가와의 괴리율·등급을 계산합니다.
/// </summary>
internal static class MarketGapCalculator
{
    /// <summary>
    ///     시세 갭 분석을 수행합니다.
    /// </summary>
    /// <param name="trades">주변 실거래 내역.</param>
    /// <param name="minBidPrc">최저 입찰가 (원 단위).</param>
    /// <param name="referenceDate">분석 기준일.</param>
    /// <returns>시세 갭 분석 결과.</returns>
    public static MarketGapResult Calculate(
        IReadOnlyList<RealEstateTradeResponse> trades,
        long minBidPrc,
        DateOnly referenceDate)
    {
        var originalCount = trades.Count;

        // IQR 기반 이상치 제거 (4건 미만이면 생략)
        IReadOnlyList<RealEstateTradeResponse> filtered = FilterOutliers(trades);

        // 가중 합계와 가중치 총합 — 가중 평균 계산용 누적기
        var weightedSum = 0.0;
        var weightTotal = 0.0;

        foreach (RealEstateTradeResponse trade in filtered)
        {
            // 거래 시점에 따른 가중치 (최근일수록 높음: 6개월내 1.0 → 24개월이상 0.1)
            var weight = GetTimeWeight(trade, referenceDate);

            // DealAmount는 만원 단위이므로 ×10,000하여 원 단위로 환산 후 가중
            weightedSum += trade.DealAmount * 10_000L * weight;

            // 가중치 누적 (나중에 가중 평균 분모로 사용)
            weightTotal += weight;
        }

        // 가중 평균 시세 계산 -> 가중치 합이 0이면 (이론적 불가) 0원 처리
        var weightedMarketPrice = weightTotal > 0
            ? (long)Math.Round(weightedSum / weightTotal)
            : 0L;

        // 시세 갭률(%) = (추정시세 - 최저입찰가) / 추정시세 × 100
        // 양수면 시세보다 싸게 입찰 가능, 음수면 시세 대비 비싼 물건
        var gapRate = weightedMarketPrice > 0
            ? Math.Round((double)(weightedMarketPrice - minBidPrc) / weightedMarketPrice * 100, 2)
            : 0.0;

        // 갭률에 따른 등급 부여 (S/A/B/C/D)
        var grade = GradeFromGapRate(gapRate);

        var confidence = ConfidenceFromCount(filtered.Count);

        return new MarketGapResult(
            weightedMarketPrice, minBidPrc, gapRate, grade,
            originalCount, filtered.Count, confidence);
    }

    /// <summary>
    ///     IQR 기반 이상치 제거. per-㎡ 단가 기준으로 [Q1-1.5*IQR, Q3+1.5*IQR] 범위 밖 거래를 제외합니다.
    ///     4건 미만이면 IQR이 무의미하므로 필터링을 생략합니다.
    /// </summary>
    internal static IReadOnlyList<RealEstateTradeResponse> FilterOutliers(
        IReadOnlyList<RealEstateTradeResponse> trades)
    {
        if (trades.Count < 4)
            return trades;

        // per-㎡ 단가 산출 (Area ≤ 0이면 DealAmount × 10000 그대로 사용)
        var unitPrices = trades
            .Select(t => t.Area > 0 ? (double)(t.DealAmount * 10_000L) / (double)t.Area : t.DealAmount * 10_000.0)
            .ToList();

        unitPrices.Sort();

        var q1 = Percentile(unitPrices, 25);
        var q3 = Percentile(unitPrices, 75);
        var iqr = q3 - q1;
        var lowerBound = q1 - (1.5 * iqr);
        var upperBound = q3 + (1.5 * iqr);

        var filtered = trades
            .Where(t =>
            {
                var unitPrice = t.Area > 0
                    ? (double)(t.DealAmount * 10_000L) / (double)t.Area
                    : t.DealAmount * 10_000.0;
                return unitPrice >= lowerBound && unitPrice <= upperBound;
            })
            .ToList();

        // 필터링 후 0건이면 원본 유지 (안전장치)
        return filtered.Count > 0 ? filtered : trades;
    }

    /// <summary>
    ///     선형 보간 백분위수 계산.
    /// </summary>
    internal static double Percentile(List<double> sorted, double percentile)
    {
        var n = sorted.Count;
        var rank = (percentile / 100.0) * (n - 1);
        var lower = (int)Math.Floor(rank);
        var upper = (int)Math.Ceiling(rank);

        if (lower == upper)
            return sorted[lower];

        var fraction = rank - lower;
        return sorted[lower] + (fraction * (sorted[upper] - sorted[lower]));
    }

    /// <summary>
    ///     사용 거래 건수 기반 신뢰도 반환.
    /// </summary>
    internal static string ConfidenceFromCount(int usedCount) => usedCount switch
    {
        >= 5 => "높음",
        >= 3 => "보통",
        _ => "낮음",
    };

    /// <summary>
    ///     거래 시점 기준 시간 가중치 반환.
    ///     최근 거래일수록 현재 시세를 더 잘 반영하므로 높은 가중치 부여.
    /// </summary>
    /// <returns>6개월 미만=1.0, 12개월 미만=0.6, 24개월 미만=0.3, 이상=0.1.</returns>
    internal static double GetTimeWeight(RealEstateTradeResponse trade, DateOnly referenceDate)
    {
        // 거래 일자를 DateOnly로 변환
        DateOnly tradeDate = new(trade.DealYear, trade.DealMonth, trade.DealDay);

        // 기준일과 거래일 사이의 개월 수 차이 계산 (연 차이 > 월 환산 + 월 차이)
        var yearDiff = referenceDate.Year - tradeDate.Year;
        var yearInMonths = yearDiff * 12;
        var monthsDiff = yearInMonths + referenceDate.Month - tradeDate.Month;

        // 개월 수 구간에 따라 가중치 반환 — 최근 거래에 더 높은 신뢰도 부여
        return monthsDiff switch
        {
            < 6 => 1.0, // 6개월 미만: 가장 최신 시세 반영
            < 12 => 0.6, // 6~12개월: 비교적 최근
            < 24 => 0.3, // 1~2년: 참고 수준
            _ => 0.1, // 2년 이상: 최소 반영 (노이즈 최소화)
        };
    }

    /// <summary>
    ///     시세 갭률(%) → 등급 변환.
    ///     갭이 클수록 시세 대비 저렴하여 투자 매력도가 높음.
    /// </summary>
    internal static string GradeFromGapRate(double gapRate) => gapRate switch
    {
        >= 40 => "S", // 40% 이상: 초저가 (극히 드묾)
        >= 25 => "A", // 25~40%: 우수한 갭
        >= 15 => "B", // 15~25%: 양호
        >= 5 => "C", // 5~15%: 보통
        _ => "D", // 5% 미만 또는 음수: 시세 수준이거나 고가
    };
}
