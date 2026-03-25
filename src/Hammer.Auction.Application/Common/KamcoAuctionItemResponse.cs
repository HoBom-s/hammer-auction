using System.Text.Json;
using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Application.Common;

/// <summary>
///     Response DTO for a KAMCO auction item.
/// </summary>
/// <param name="Id">고유 식별자.</param>
/// <param name="PlnmNo">공고번호.</param>
/// <param name="PbctNo">공매번호.</param>
/// <param name="CltrNo">물건번호.</param>
/// <param name="CltrNm">물건명.</param>
/// <param name="CtgrFullNm">용도 (예: 주거용건물 / 단독주택).</param>
/// <param name="LdnmAdrs">지번주소.</param>
/// <param name="NmrdAdrs">도로명주소.</param>
/// <param name="MinBidPrc">최저입찰가 (원).</param>
/// <param name="ApslAsesAvgAmt">감정가 (원).</param>
/// <param name="BidMtdNm">입찰방식.</param>
/// <param name="PbctCltrStatNm">물건상태 (예: 입찰진행중, 입찰준비중).</param>
/// <param name="PbctBegnDtm">입찰 시작일시 (UTC).</param>
/// <param name="PbctClsDtm">입찰 마감일시 (UTC).</param>
/// <param name="UscbdCnt">유찰횟수.</param>
/// <param name="IqryCnt">조회수.</param>
/// <param name="CltrImgFiles">물건 이미지 URL 목록.</param>
/// <param name="DiscountRate">감정가 대비 할인율 (%).</param>
/// <param name="CreatedAt">최초 수집일시 (UTC).</param>
/// <param name="UpdatedAt">최종 갱신일시 (UTC).</param>
public sealed record KamcoAuctionItemResponse(
    long Id,
    long PlnmNo,
    long PbctNo,
    long CltrNo,
    string CltrNm,
    string CtgrFullNm,
    string LdnmAdrs,
    string NmrdAdrs,
    long MinBidPrc,
    long ApslAsesAvgAmt,
    string BidMtdNm,
    string PbctCltrStatNm,
    DateTimeOffset PbctBegnDtm,
    DateTimeOffset PbctClsDtm,
    int UscbdCnt,
    int IqryCnt,
    IReadOnlyList<string>? CltrImgFiles,
    double DiscountRate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    ///     Maps a domain entity to a response DTO.
    /// </summary>
    public static KamcoAuctionItemResponse FromEntity(KamcoAuctionItem entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new KamcoAuctionItemResponse(
            entity.Id,
            entity.PlnmNo,
            entity.PbctNo,
            entity.CltrNo,
            entity.CltrNm,
            entity.CtgrFullNm,
            entity.LdnmAdrs,
            entity.NmrdAdrs,
            entity.MinBidPrc,
            entity.ApslAsesAvgAmt,
            entity.BidMtdNm,
            entity.PbctCltrStatNm,
            entity.PbctBegnDtm,
            entity.PbctClsDtm,
            entity.UscbdCnt,
            entity.IqryCnt,
            ParseImgFiles(entity.CltrImgFiles),
            entity.DiscountRate(),
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private static List<string>? ParseImgFiles(string? json) =>
        string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<List<string>>(json, _jsonOptions);
}
