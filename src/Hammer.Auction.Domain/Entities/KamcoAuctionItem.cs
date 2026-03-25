using System.Globalization;

namespace Hammer.Auction.Domain.Entities;

/// <summary>
///     KAMCO public auction item from the Onbid API.
/// </summary>
public sealed class KamcoAuctionItem
{
    private static readonly TimeSpan _kst = TimeSpan.FromHours(9);

    private KamcoAuctionItem()
    {
    }

    /// <summary>
    ///     Gets the surrogate primary key.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    ///     Gets the announcement number (공고번호).
    /// </summary>
    public long PlnmNo { get; private set; }

    /// <summary>
    ///     Gets the auction number (공매번호).
    /// </summary>
    public long PbctNo { get; private set; }

    /// <summary>
    ///     Gets the item number (물건번호).
    /// </summary>
    public long CltrNo { get; private set; }

    /// <summary>
    ///     Gets the item name (물건명).
    /// </summary>
    public string CltrNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the full category name (용도명).
    /// </summary>
    public string CtgrFullNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the land lot address (지번주소).
    /// </summary>
    public string LdnmAdrs { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the road name address (도로명주소).
    /// </summary>
    public string NmrdAdrs { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the minimum bid price (최저입찰가).
    /// </summary>
    public long MinBidPrc { get; private set; }

    /// <summary>
    ///     Gets the average appraisal amount (감정가).
    /// </summary>
    public long ApslAsesAvgAmt { get; private set; }

    /// <summary>
    ///     Gets the bid method name (입찰방식명).
    /// </summary>
    public string BidMtdNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the item status name (물건상태).
    /// </summary>
    public string PbctCltrStatNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the bid start datetime (입찰시작일시).
    /// </summary>
    public DateTimeOffset PbctBegnDtm { get; private set; }

    /// <summary>
    ///     Gets the bid close datetime (입찰마감일시).
    /// </summary>
    public DateTimeOffset PbctClsDtm { get; private set; }

    /// <summary>
    ///     Gets the failed bid count (유찰횟수).
    /// </summary>
    public int UscbdCnt { get; private set; }

    /// <summary>
    ///     Gets the inquiry count (조회수).
    /// </summary>
    public int IqryCnt { get; private set; }

    /// <summary>
    ///     Gets the item image files URL (물건이미지).
    /// </summary>
    public string? CltrImgFiles { get; private set; }

    /// <summary>
    ///     Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    ///     Gets the last update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    ///     Creates a new KAMCO auction item from raw snapshot data.
    /// </summary>
    /// <returns></returns>
    public static KamcoAuctionItem Create(
        long plnmNo,
        long pbctNo,
        long cltrNo,
        string cltrNm,
        string ctgrFullNm,
        string ldnmAdrs,
        string nmrdAdrs,
        long minBidPrc,
        long apslAsesAvgAmt,
        string bidMtdNm,
        string pbctCltrStatNm,
        string pbctBegnDtm,
        string pbctClsDtm,
        int uscbdCnt,
        int iqryCnt,
        string? cltrImgFiles)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return new KamcoAuctionItem
        {
            PlnmNo = plnmNo,
            PbctNo = pbctNo,
            CltrNo = cltrNo,
            CltrNm = cltrNm,
            CtgrFullNm = ctgrFullNm,
            LdnmAdrs = ldnmAdrs,
            NmrdAdrs = nmrdAdrs,
            MinBidPrc = minBidPrc,
            ApslAsesAvgAmt = apslAsesAvgAmt,
            BidMtdNm = bidMtdNm,
            PbctCltrStatNm = pbctCltrStatNm,
            PbctBegnDtm = ParseKamcoDateTime(pbctBegnDtm),
            PbctClsDtm = ParseKamcoDateTime(pbctClsDtm),
            UscbdCnt = uscbdCnt,
            IqryCnt = iqryCnt,
            CltrImgFiles = cltrImgFiles,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    /// <summary>
    ///     Updates mutable fields from a daily snapshot.
    /// </summary>
    public void UpdateFromSnapshot(
        string cltrNm,
        string ctgrFullNm,
        string ldnmAdrs,
        string nmrdAdrs,
        long minBidPrc,
        long apslAsesAvgAmt,
        string bidMtdNm,
        string pbctCltrStatNm,
        string pbctBegnDtm,
        string pbctClsDtm,
        int uscbdCnt,
        int iqryCnt,
        string? cltrImgFiles)
    {
        CltrNm = cltrNm;
        CtgrFullNm = ctgrFullNm;
        LdnmAdrs = ldnmAdrs;
        NmrdAdrs = nmrdAdrs;
        MinBidPrc = minBidPrc;
        ApslAsesAvgAmt = apslAsesAvgAmt;
        BidMtdNm = bidMtdNm;
        PbctCltrStatNm = pbctCltrStatNm;
        PbctBegnDtm = ParseKamcoDateTime(pbctBegnDtm);
        PbctClsDtm = ParseKamcoDateTime(pbctClsDtm);
        UscbdCnt = uscbdCnt;
        IqryCnt = iqryCnt;
        CltrImgFiles = cltrImgFiles;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    ///     Returns whether the auction is currently in its bidding window.
    /// </summary>
    /// <returns></returns>
    public bool IsActive(DateTimeOffset now) => now >= PbctBegnDtm && now < PbctClsDtm;

    /// <summary>
    ///     Returns whether the bidding period has ended.
    /// </summary>
    /// <returns></returns>
    public bool IsClosed(DateTimeOffset now) => now >= PbctClsDtm;

    /// <summary>
    ///     Computes the discount rate from the appraisal value as a percentage.
    /// </summary>
    /// <returns></returns>
    public double DiscountRate()
    {
        if (ApslAsesAvgAmt == 0)
            return 0;

        return Math.Round((1.0 - ((double)MinBidPrc / ApslAsesAvgAmt)) * 100, 2);
    }

    /// <summary>
    ///     Parses a KAMCO datetime string (YYYYMMDDHHmmss) to a KST DateTimeOffset.
    /// </summary>
    /// <returns></returns>
    internal static DateTimeOffset ParseKamcoDateTime(string raw) =>
        new(
            DateTime.ParseExact(raw, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            _kst);
}
