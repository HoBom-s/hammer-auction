using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Application.Common;

/// <summary>
///     Response DTO for an institution auction item.
/// </summary>
/// <param name="Id">고유 식별자.</param>
/// <param name="PlnmNo">공고번호.</param>
/// <param name="PbctNo">공매번호.</param>
/// <param name="PlnmKindCd">공고종류코드.</param>
/// <param name="PlnmKindNm">공고종류.</param>
/// <param name="BidDvsnCd">입찰형태코드.</param>
/// <param name="BidDvsnNm">입찰형태.</param>
/// <param name="PlnmNm">공고명.</param>
/// <param name="OrgNm">공고기관명.</param>
/// <param name="PlnmDt">공고일자 (YYYYMMDD).</param>
/// <param name="OrgPlnmNo">기관공고번호.</param>
/// <param name="PlnmMnmtNo">공고관리번호.</param>
/// <param name="BidMtdCd">입찰방식코드.</param>
/// <param name="BidMtdNm">입찰방식.</param>
/// <param name="TotAmtUnpcDvsnCd">총액단가구분코드.</param>
/// <param name="TotAmtUnpcDvsnNm">총액단가구분.</param>
/// <param name="DpslMtdCd">처분방식코드.</param>
/// <param name="DpslMtdNm">처분방식.</param>
/// <param name="PrptDvsnCd">재산구분코드.</param>
/// <param name="PrptDvsnNm">재산구분.</param>
/// <param name="PbctBegnDtm">입찰 시작일시 (UTC).</param>
/// <param name="PbctClsDtm">입찰 마감일시 (UTC).</param>
/// <param name="PbctExctDtm">개찰일시 (UTC).</param>
/// <param name="CtgrId">용도코드.</param>
/// <param name="CtgrFullNm">용도.</param>
/// <param name="CreatedAt">최초 수집일시 (UTC).</param>
/// <param name="UpdatedAt">최종 갱신일시 (UTC).</param>
public sealed record InstitutionAuctionItemResponse(
    long Id,
    long PlnmNo,
    long PbctNo,
    string PlnmKindCd,
    string PlnmKindNm,
    string BidDvsnCd,
    string BidDvsnNm,
    string PlnmNm,
    string OrgNm,
    string PlnmDt,
    string OrgPlnmNo,
    string PlnmMnmtNo,
    string BidMtdCd,
    string BidMtdNm,
    string TotAmtUnpcDvsnCd,
    string TotAmtUnpcDvsnNm,
    string DpslMtdCd,
    string DpslMtdNm,
    string PrptDvsnCd,
    string PrptDvsnNm,
    DateTimeOffset PbctBegnDtm,
    DateTimeOffset PbctClsDtm,
    DateTimeOffset PbctExctDtm,
    string CtgrId,
    string CtgrFullNm,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    /// <summary>
    ///     Maps a domain entity to a response DTO.
    /// </summary>
    public static InstitutionAuctionItemResponse FromEntity(InstitutionAuctionItem entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new InstitutionAuctionItemResponse(
            entity.Id,
            entity.PlnmNo,
            entity.PbctNo,
            entity.PlnmKindCd,
            entity.PlnmKindNm,
            entity.BidDvsnCd,
            entity.BidDvsnNm,
            entity.PlnmNm,
            entity.OrgNm,
            entity.PlnmDt,
            entity.OrgPlnmNo,
            entity.PlnmMnmtNo,
            entity.BidMtdCd,
            entity.BidMtdNm,
            entity.TotAmtUnpcDvsnCd,
            entity.TotAmtUnpcDvsnNm,
            entity.DpslMtdCd,
            entity.DpslMtdNm,
            entity.PrptDvsnCd,
            entity.PrptDvsnNm,
            entity.PbctBegnDtm,
            entity.PbctClsDtm,
            entity.PbctExctDtm,
            entity.CtgrId,
            entity.CtgrFullNm,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
