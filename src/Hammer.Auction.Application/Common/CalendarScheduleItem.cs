using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Application.Common;

/// <summary>
///     A single auction schedule entry for the calendar view.
/// </summary>
/// <param name="Id">물건 고유 식별자.</param>
/// <param name="Source">데이터 출처 (Kamco / Institution).</param>
/// <param name="Name">물건명 또는 공고명.</param>
/// <param name="Category">용도.</param>
/// <param name="MinBidPrice">최저입찰가 (KAMCO만 해당, Institution은 null).</param>
/// <param name="PbctBegnDtm">입찰 시작일시 (UTC).</param>
/// <param name="PbctClsDtm">입찰 마감일시 (UTC).</param>
public sealed record CalendarScheduleItem(
    long Id,
    string Source,
    string Name,
    string Category,
    long? MinBidPrice,
    DateTimeOffset PbctBegnDtm,
    DateTimeOffset PbctClsDtm)
{
    /// <summary>
    ///     Maps a KAMCO auction item to a calendar schedule item.
    /// </summary>
    public static CalendarScheduleItem FromKamco(KamcoAuctionItem entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new CalendarScheduleItem(
            entity.Id,
            "Kamco",
            entity.CltrNm,
            entity.CtgrFullNm,
            entity.MinBidPrc,
            entity.PbctBegnDtm,
            entity.PbctClsDtm);
    }

    /// <summary>
    ///     Maps an institution auction item to a calendar schedule item.
    /// </summary>
    public static CalendarScheduleItem FromInstitution(InstitutionAuctionItem entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new CalendarScheduleItem(
            entity.Id,
            "Institution",
            entity.PlnmNm,
            entity.CtgrFullNm,
            null,
            entity.PbctBegnDtm,
            entity.PbctClsDtm);
    }
}
