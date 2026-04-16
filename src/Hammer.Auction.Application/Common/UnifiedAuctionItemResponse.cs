using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Application.Common;

/// <summary>
///     Unified response DTO for auction items from any source.
/// </summary>
/// <param name="Id">물건 고유 식별자.</param>
/// <param name="Source">데이터 출처 (Kamco / Institution).</param>
/// <param name="Name">물건명 또는 공고명.</param>
/// <param name="Category">용도.</param>
/// <param name="MinBidPrice">최저입찰가 (KAMCO만 해당).</param>
/// <param name="PbctBegnDtm">입찰 시작일시 (UTC).</param>
/// <param name="PbctClsDtm">입찰 마감일시 (UTC).</param>
/// <param name="Address">주소 (KAMCO: 지번주소, Institution: null).</param>
/// <param name="Status">물건 상태 (KAMCO만 해당).</param>
public sealed record UnifiedAuctionItemResponse(
    long Id,
    string Source,
    string Name,
    string Category,
    long? MinBidPrice,
    DateTimeOffset PbctBegnDtm,
    DateTimeOffset PbctClsDtm,
    string? Address,
    string? Status)
{
    /// <summary>
    ///     Maps a KAMCO auction item to a unified response.
    /// </summary>
    public static UnifiedAuctionItemResponse FromKamco(KamcoAuctionItem entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UnifiedAuctionItemResponse(
            entity.Id,
            "Kamco",
            entity.CltrNm,
            entity.CtgrFullNm,
            entity.MinBidPrc,
            entity.PbctBegnDtm,
            entity.PbctClsDtm,
            entity.LdnmAdrs,
            entity.PbctCltrStatNm);
    }

    /// <summary>
    ///     Maps an institution auction item to a unified response.
    /// </summary>
    public static UnifiedAuctionItemResponse FromInstitution(InstitutionAuctionItem entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UnifiedAuctionItemResponse(
            entity.Id,
            "Institution",
            entity.PlnmNm,
            entity.CtgrFullNm,
            null,
            entity.PbctBegnDtm,
            entity.PbctClsDtm,
            null,
            null);
    }
}
