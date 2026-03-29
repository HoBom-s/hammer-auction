namespace Hammer.Auction.Application.Common;

/// <summary>
/// <see cref="KamcoAuctionItemResponse.CtgrFullNm"/>의 첫 세그먼트로 대분류 카테고리를 판별합니다.
/// </summary>
public static class PropertyCategoryClassifier
{
    private static readonly HashSet<string> _realEstateSegments = new(StringComparer.Ordinal)
    {
        "토지",
        "주거용건물",
        "상가용및업무용건물",
        "산업용및기타특수용건물",
        "용도복합용건물",
    };

    private static readonly HashSet<string> _movableSegments = new(StringComparer.Ordinal)
    {
        "자동차",
        "건축자재및기계",
        "선박",
        "시계/귀금속",
        "컴퓨터/전기/통신기계",
        "석유/화학/연료",
    };

    /// <summary>
    /// <paramref name="ctgrFullNm"/>의 첫 <c>" / "</c> 세그먼트를 기준으로 대분류를 반환합니다.
    /// </summary>
    public static PropertyCategory Classify(string ctgrFullNm)
    {
        ArgumentNullException.ThrowIfNull(ctgrFullNm);

        var segment = ExtractFirstSegment(ctgrFullNm);

        if (_realEstateSegments.Contains(segment))
            return PropertyCategory.부동산;

        if (_movableSegments.Contains(segment))
            return PropertyCategory.동산;

        return PropertyCategory.기타;
    }

    private static string ExtractFirstSegment(string ctgrFullNm)
    {
        var idx = ctgrFullNm.IndexOf(" / ", StringComparison.Ordinal);
        return idx < 0 ? ctgrFullNm : ctgrFullNm[..idx];
    }
}
