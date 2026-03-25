using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Application.Common;

/// <summary>
/// Response DTO for a KAMCO auction item.
/// </summary>
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
    string? CltrImgFiles,
    double DiscountRate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    /// <summary>
    /// Maps a domain entity to a response DTO.
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
            entity.CltrImgFiles,
            entity.DiscountRate(),
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
