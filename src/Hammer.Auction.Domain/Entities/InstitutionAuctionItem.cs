using System.Globalization;

namespace Hammer.Auction.Domain.Entities;

/// <summary>
///     Institution public sale announcement from the Onbid API.
/// </summary>
public sealed class InstitutionAuctionItem
{
    private static readonly TimeSpan _kst = TimeSpan.FromHours(9);

    private InstitutionAuctionItem()
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
    ///     Gets the announcement type code (공고종류코드).
    /// </summary>
    public string PlnmKindCd { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the announcement type name (공고종류).
    /// </summary>
    public string PlnmKindNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the bid form code (입찰형태코드).
    /// </summary>
    public string BidDvsnCd { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the bid form name (입찰형태).
    /// </summary>
    public string BidDvsnNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the announcement name (공고명).
    /// </summary>
    public string PlnmNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the announcing organization name (공고기관명).
    /// </summary>
    public string OrgNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the announcement date (공고일자, YYYYMMDD).
    /// </summary>
    public string PlnmDt { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the organization announcement number (기관공고번호).
    /// </summary>
    public string OrgPlnmNo { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the announcement management number (공고관리번호).
    /// </summary>
    public string PlnmMnmtNo { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the bid method code (입찰방식코드).
    /// </summary>
    public string BidMtdCd { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the bid method name (입찰방식).
    /// </summary>
    public string BidMtdNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the total/unit price division code (총액단가구분코드).
    /// </summary>
    public string TotAmtUnpcDvsnCd { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the total/unit price division name (총액단가구분).
    /// </summary>
    public string TotAmtUnpcDvsnNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the disposal method code (처분방식코드).
    /// </summary>
    public string DpslMtdCd { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the disposal method name (처분방식).
    /// </summary>
    public string DpslMtdNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the property division code (재산구분코드).
    /// </summary>
    public string PrptDvsnCd { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the property division name (재산구분).
    /// </summary>
    public string PrptDvsnNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the bid start datetime (입찰시작일시).
    /// </summary>
    public DateTimeOffset PbctBegnDtm { get; private set; }

    /// <summary>
    ///     Gets the bid close datetime (입찰마감일시).
    /// </summary>
    public DateTimeOffset PbctClsDtm { get; private set; }

    /// <summary>
    ///     Gets the bid opening datetime (개찰일시).
    /// </summary>
    public DateTimeOffset PbctExctDtm { get; private set; }

    /// <summary>
    ///     Gets the category ID (용도코드).
    /// </summary>
    public string CtgrId { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the full category name (용도).
    /// </summary>
    public string CtgrFullNm { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    ///     Gets the last update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    ///     Creates a new institution auction item from raw snapshot data.
    /// </summary>
    public static InstitutionAuctionItem Create(
        long plnmNo,
        long pbctNo,
        string plnmKindCd,
        string plnmKindNm,
        string bidDvsnCd,
        string bidDvsnNm,
        string plnmNm,
        string orgNm,
        string plnmDt,
        string orgPlnmNo,
        string plnmMnmtNo,
        string bidMtdCd,
        string bidMtdNm,
        string totAmtUnpcDvsnCd,
        string totAmtUnpcDvsnNm,
        string dpslMtdCd,
        string dpslMtdNm,
        string prptDvsnCd,
        string prptDvsnNm,
        string pbctBegnDtm,
        string pbctClsDtm,
        string pbctExctDtm,
        string ctgrId,
        string ctgrFullNm)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return new InstitutionAuctionItem
        {
            PlnmNo = plnmNo,
            PbctNo = pbctNo,
            PlnmKindCd = plnmKindCd,
            PlnmKindNm = plnmKindNm,
            BidDvsnCd = bidDvsnCd,
            BidDvsnNm = bidDvsnNm,
            PlnmNm = plnmNm,
            OrgNm = orgNm,
            PlnmDt = plnmDt,
            OrgPlnmNo = orgPlnmNo,
            PlnmMnmtNo = plnmMnmtNo,
            BidMtdCd = bidMtdCd,
            BidMtdNm = bidMtdNm,
            TotAmtUnpcDvsnCd = totAmtUnpcDvsnCd,
            TotAmtUnpcDvsnNm = totAmtUnpcDvsnNm,
            DpslMtdCd = dpslMtdCd,
            DpslMtdNm = dpslMtdNm,
            PrptDvsnCd = prptDvsnCd,
            PrptDvsnNm = prptDvsnNm,
            PbctBegnDtm = ParseOnbidDateTime(pbctBegnDtm),
            PbctClsDtm = ParseOnbidDateTime(pbctClsDtm),
            PbctExctDtm = ParseOnbidDateTime(pbctExctDtm),
            CtgrId = ctgrId,
            CtgrFullNm = ctgrFullNm,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    /// <summary>
    ///     Updates mutable fields from a daily snapshot.
    /// </summary>
    public void UpdateFromSnapshot(
        string plnmKindCd,
        string plnmKindNm,
        string bidDvsnCd,
        string bidDvsnNm,
        string plnmNm,
        string orgNm,
        string plnmDt,
        string orgPlnmNo,
        string plnmMnmtNo,
        string bidMtdCd,
        string bidMtdNm,
        string totAmtUnpcDvsnCd,
        string totAmtUnpcDvsnNm,
        string dpslMtdCd,
        string dpslMtdNm,
        string prptDvsnCd,
        string prptDvsnNm,
        string pbctBegnDtm,
        string pbctClsDtm,
        string pbctExctDtm,
        string ctgrId,
        string ctgrFullNm)
    {
        PlnmKindCd = plnmKindCd;
        PlnmKindNm = plnmKindNm;
        BidDvsnCd = bidDvsnCd;
        BidDvsnNm = bidDvsnNm;
        PlnmNm = plnmNm;
        OrgNm = orgNm;
        PlnmDt = plnmDt;
        OrgPlnmNo = orgPlnmNo;
        PlnmMnmtNo = plnmMnmtNo;
        BidMtdCd = bidMtdCd;
        BidMtdNm = bidMtdNm;
        TotAmtUnpcDvsnCd = totAmtUnpcDvsnCd;
        TotAmtUnpcDvsnNm = totAmtUnpcDvsnNm;
        DpslMtdCd = dpslMtdCd;
        DpslMtdNm = dpslMtdNm;
        PrptDvsnCd = prptDvsnCd;
        PrptDvsnNm = prptDvsnNm;
        PbctBegnDtm = ParseOnbidDateTime(pbctBegnDtm);
        PbctClsDtm = ParseOnbidDateTime(pbctClsDtm);
        PbctExctDtm = ParseOnbidDateTime(pbctExctDtm);
        CtgrId = ctgrId;
        CtgrFullNm = ctgrFullNm;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    ///     Parses an Onbid datetime string (yyyyMMddHHmmss KST) to UTC DateTimeOffset.
    /// </summary>
    internal static DateTimeOffset ParseOnbidDateTime(string raw) =>
        new DateTimeOffset(
            DateTime.ParseExact(raw, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            _kst).ToUniversalTime();
}
