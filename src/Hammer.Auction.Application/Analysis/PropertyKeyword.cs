using System.Diagnostics.CodeAnalysis;

namespace Hammer.Auction.Application.Analysis;

/// <summary>
///     카테고리명(ctgrFullNm)에서 물건 유형을 판별하기 위한 키워드 열거형.
///     각 멤버 이름이 곧 매칭 키워드 문자열로 사용됩니다.
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.NamingRules",
    "SA1300:ElementMustBeginWithUpperCaseLetter",
    Justification = "도메인 고유 한국어 키워드 — 멤버 이름이 곧 매칭 문자열")]
internal enum PropertyKeyword
{
    // 주거용 (명도비용 3%)
    주거,
    주택,
    아파트,
    빌라,
    연립,
    다세대,
    오피스텔,

    // 상업용 (명도비용 2%)
    상가,
    근린,
    사무,

    // 기타 건물 (명도비용 1%)
    건물,
}
