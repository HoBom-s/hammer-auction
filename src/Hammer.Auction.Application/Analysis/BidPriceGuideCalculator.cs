namespace Hammer.Auction.Application.Analysis;

/// <summary>
///     Model 3 - 적정 입찰가 가이드.
///     목표 수익률별 3단계 입찰가와 비용 구조, 안내 문구를 생성합니다.
/// </summary>
internal static class BidPriceGuideCalculator
{
    /// <summary>취득세율 (%) - 경매 낙찰 시 통상 적용되는 세율.</summary>
    internal const double AcquisitionTaxRate = 4.6;

    /// <summary>기타 비용률 (%) - 법무사 수수료, 이전비 등 잡비.</summary>
    internal const double MiscCostRate = 1.0;

    /// <summary>
    ///     키워드별 명도비용률(%) 매핑 테이블.
    ///     선언 순서가 곧 매칭 우선순위 (주거 > 상업 > 건물).
    ///     <see cref="PropertyKeyword" /> enum이 키워드의 단일 소스 오브 트루스.
    /// </summary>
    private static readonly (string Keyword, double CostRate)[] _keywordCostMap =
    [
        (nameof(PropertyKeyword.주거), 3.0),
        (nameof(PropertyKeyword.주택), 3.0),
        (nameof(PropertyKeyword.아파트), 3.0),
        (nameof(PropertyKeyword.빌라), 3.0),
        (nameof(PropertyKeyword.연립), 3.0),
        (nameof(PropertyKeyword.다세대), 3.0),
        (nameof(PropertyKeyword.오피스텔), 3.0),
        (nameof(PropertyKeyword.상가), 2.0),
        (nameof(PropertyKeyword.근린), 2.0),
        (nameof(PropertyKeyword.사무), 2.0),
        (nameof(PropertyKeyword.건물), 1.0),
    ];

    /// <summary>
    ///     적정 입찰가 가이드를 계산합니다.
    /// </summary>
    /// <param name="weightedMarketPrice">시간가중 평균 시세 (원).</param>
    /// <param name="evictionCostRate">명도비용률 (%).</param>
    /// <param name="uscbdCnt">유찰 횟수.</param>
    /// <param name="appraisalAmount">감정가 (원 단위). 0 이하이면 할인율 0%.</param>
    /// <param name="minBidPrc">최저 입찰가 (원 단위).</param>
    /// <returns>3단계 입찰가 가이드 결과.</returns>
    public static BidPriceGuideResult Calculate(
        long weightedMarketPrice,
        double evictionCostRate,
        int uscbdCnt,
        long appraisalAmount = 0,
        long minBidPrc = 0)
    {
        // 총 비용률 = (취득세 + 명도비용 + 기타비용) / 100 → 비율(0~1)로 환산
        var totalCostRate = (AcquisitionTaxRate + evictionCostRate + MiscCostRate) / 100.0;

        // 보수적 입찰가: 목표 수익률 20% 확보 (시세 × (1 - 수익률 - 비용률))
        var conservativeBid = CalculateBid(weightedMarketPrice, 0.20, totalCostRate);

        // 적정 입찰가: 목표 수익률 10% 확보
        var moderateBid = CalculateBid(weightedMarketPrice, 0.10, totalCostRate);

        // 공격적 입찰가: 목표 수익률 5% 확보 (낙찰 가능성 극대화)
        var aggressiveBid = CalculateBid(weightedMarketPrice, 0.05, totalCostRate);

        // 감정가 대비 할인율 (%)
        var appraisalDiscountRate = appraisalAmount > 0
            ? Math.Round((1.0 - (double)minBidPrc / appraisalAmount) * 100, 2)
            : 0.0;

        // 유찰 횟수에 따른 전략 안내 문구 생성
        var guidance = GetGuidanceText(uscbdCnt);

        return new BidPriceGuideResult(
            conservativeBid,
            moderateBid,
            aggressiveBid,
            AcquisitionTaxRate,
            evictionCostRate,
            MiscCostRate,
            appraisalDiscountRate,
            guidance);
    }

    /// <summary>
    ///     카테고리명으로 명도비용률(%) 산출.
    ///     점유자를 내보내는 데 필요한 예상 비용을 물건 유형별로 차등 적용합니다.
    /// </summary>
    internal static double GetEvictionCostRate(string ctgrFullNm)
    {
        // 카테고리 없으면 명도비용 산정 불가 → 0%
        if (string.IsNullOrEmpty(ctgrFullNm))
            return 0;

        // _keywordCostMap 순서대로 첫 매칭 키워드의 비용률 반환 (주거 > 상업 > 건물)
        // 매칭 없으면 default tuple의 CostRate = 0.0 반환
        return _keywordCostMap
            .FirstOrDefault(e => ctgrFullNm.Contains(e.Keyword, StringComparison.Ordinal))
            .CostRate;
    }

    /// <summary>
    ///     유찰 횟수에 따른 투자 전략 안내 문구 생성.
    ///     유찰이 거듭될수록 할인이 깊어지므로 적극적인 전략을 권유합니다.
    /// </summary>
    internal static string GetGuidanceText(int uscbdCnt) => uscbdCnt switch
    {
        0 => "첫 입찰 물건입니다. 시장 상황을 관망하며 신중하게 접근하세요.",
        1 => "1회 유찰로 적정 진입 시점입니다. 시세 대비 할인 폭을 확인하세요.",
        2 => "2회 유찰로 할인이 깊어졌습니다. 적정~공격적 입찰이 유리할 수 있어요.",
        _ => $"{uscbdCnt}회 유찰로 대폭 할인된 물건입니다. 권리 분석 후 적극 검토하세요.",
    };

    /// <summary>
    ///     입찰가 계산 공식: 시세 × (1 - 목표수익률 - 총비용률).
    ///     시세에서 수익률과 비용을 차감한 금액이 곧 입찰 상한선.
    /// </summary>
    private static long CalculateBid(long marketPrice, double targetReturn, double totalCostRate) =>
        (long)Math.Round(marketPrice * (1.0 - targetReturn - totalCostRate));
}
